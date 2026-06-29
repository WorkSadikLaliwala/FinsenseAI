using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using FinSenseAPI.Services.Interfaces;
using FinSenseAPI.DTOs.Goal;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Models;
using FinSenseAPI.Prompts;
using FinSenseAPI.Helpers;

namespace FinSenseAPI.Services;

//public class GoalService : IGoalService
//{
//    private readonly IGoalRepository _goalRepo;

//    public GoalService(IGoalRepository goalRepo)
//    {
//        _goalRepo = goalRepo;
//    }

//    // Creates a new goal for the user and returns the persisted DTO fields that exist on GoalResponseDto.
//    public async Task<GoalResponseDto> CreateAndEvaluateGoalAsync(GoalRequestDto dto, Guid userId, CancellationToken ct)
//    {
//        var goal = new Goal
//        {
//            UserId = userId,
//            GoalName = dto.GoalName,
//            TargetAmount = dto.TargetAmount,
//            TargetDate = dto.TargetDate,
//            CurrentSavings = dto.CurrentSavings ?? 0m
//        };

//        await _goalRepo.CreateAsync(goal, ct);

//        return new GoalResponseDto
//        {
//            Id = goal.Id,
//            GoalName = goal.GoalName,
//            TargetAmount = goal.TargetAmount,
//            TargetDate = goal.TargetDate,
//            CurrentSavings = goal.CurrentSavings
//        };
//    }

//    // Retrieves user goals and maps them to the existing GoalResponseDto fields.
//    public async Task<IEnumerable<GoalResponseDto>> GetUserGoalsAsync(Guid userId, CancellationToken ct)
//    {
//        var goals = await _goalRepo.GetByUserIdAsync(userId, ct);
//        return goals.Select(g => new GoalResponseDto
//        {
//            Id = g.Id,
//            GoalName = g.GoalName,
//            TargetAmount = g.TargetAmount,
//            TargetDate = g.TargetDate,
//            CurrentSavings = g.CurrentSavings
//        });
//    }

//    // Soft-deletes a goal using the repository implementation.
//    public async Task<bool> DeleteGoalAsync(int id, Guid userId, CancellationToken ct)
//    {
//        return await _goalRepo.SoftDeleteAsync(id, userId, ct);
//    }

//    // Updates a goal via the repository and returns the updated fields present on GoalResponseDto.
//    public async Task<GoalResponseDto> UpdateGoalAsync(int id, UpdateGoalRequestDto request, Guid userId, CancellationToken ct)
//    {
//        var goal = await _goalRepo.GetByIdAsync(id, ct);
//        if (goal == null) throw new InvalidOperationException("Goal not found");
//        if (goal.UserId != userId) throw new UnauthorizedAccessException("No permission to update goal");

//        if (!string.IsNullOrEmpty(request.GoalName)) goal.GoalName = request.GoalName;
//        if (request.TargetAmount.HasValue) goal.TargetAmount = request.TargetAmount.Value;
//        if (request.TargetDate.HasValue) goal.TargetDate = request.TargetDate.Value;
//        if (request.CurrentSavings.HasValue) goal.CurrentSavings = request.CurrentSavings.Value;

//        var updated = await _goalRepo.UpdateAsync(goal, ct);
//        if (updated == null) throw new InvalidOperationException("Update failed");

//        return new GoalResponseDto
//        {
//            Id = updated.Id,
//            GoalName = updated.GoalName,
//            TargetAmount = updated.TargetAmount,
//            TargetDate = updated.TargetDate,
//            CurrentSavings = updated.CurrentSavings
//        };
//    }
//}


public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IClaudeService _claudeService;

    public GoalService(
        IGoalRepository goalRepo,
        ITransactionRepository transactionRepo,
        IClaudeService claudeService)
    {
        _goalRepo = goalRepo;
        _transactionRepo = transactionRepo;
        _claudeService = claudeService;
    }

    // =========================
    // CREATE + AI EVALUATION
    // =========================
    public async Task<GoalResponseDto> EvaluateGoalAsync(
        GoalRequestDto dto,
        Guid userId,
        CancellationToken ct)
    {
        // 1. Save Goal
        var goal = new Goal
        {
            UserId = userId,
            GoalName = dto.GoalName,
            TargetAmount = dto.TargetAmount,
            TargetDate = DateTime.SpecifyKind(dto.TargetDate, DateTimeKind.Utc),
            CurrentSavings = dto.CurrentSavings,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        await _goalRepo.CreateAsync(goal, ct);

        // 2. Get transactions for analysis
        var now = DateTime.UtcNow;

        var transactions = (await _transactionRepo.GetBySessionIdAsync(dto.SessionId, ct)).ToList();

        var referenceDate = transactions.Any()
            ? transactions.Min(t => t.Date)
            : DateTime.UtcNow;

        var income = transactions.Where(t => t.Type == "Credit").Sum(t => t.Amount);
        var expenses = transactions.Where(t => t.Type == "Debit").Sum(t => t.Amount);
        var surplus = income - expenses;

        // 3. Calculate goal metrics
        var amountRemaining = dto.TargetAmount - dto.CurrentSavings;

        //var monthsRemaining = Math.Max(1,
        //    ((dto.TargetDate.Year - now.Year) * 12)
        //    + dto.TargetDate.Month
        //    - now.Month);
        var monthsRemaining = Math.Max(1,
    ((dto.TargetDate.Year - referenceDate.Year) * 12)
    + dto.TargetDate.Month
    - referenceDate.Month);

        var requiredMonthlySaving = Math.Round(amountRemaining / monthsRemaining, 2);
        var shortfall = Math.Round(Math.Max(0, requiredMonthlySaving - surplus), 2);

        // 4. Build prompt
        var prompt = ClaudePrompts.GoalFeasibilityAnalyzer(
            dto.GoalName,
            dto.TargetAmount,
            dto.CurrentSavings,
            monthsRemaining,
            requiredMonthlySaving,
            surplus
        );

        // 5. Call Claude
        var rawResponse = await _claudeService.CallAsync(
            systemPrompt: "You are a financial AI assistant.",
            userMessage: prompt,
            maxTokens: 400,
            ct);

        // 6. Parse Claude response
        if (!JsonHelper.TryDeserialize<GoalResponseDto>(rawResponse, out var aiResult))
        {
            throw new InvalidOperationException("Failed to parse AI response.");
        }

        // 7. Fill system-calculated fields
        aiResult!.AmountRemaining = amountRemaining;
        aiResult.MonthsRemaining = monthsRemaining;
        aiResult.RequiredMonthlySaving = requiredMonthlySaving;
        aiResult.CurrentMonthlySurplus = surplus;
        aiResult.Shortfall = shortfall;

        // 8. Update goal status
        goal.Status = aiResult.Status;
        await _goalRepo.UpdateAsync(goal, ct);

        return aiResult;
    }

    // =========================
    // GET ALL GOALS
    // =========================
    public async Task<IEnumerable<GoalDto>> GetUserGoalsAsync(
        Guid userId,
        CancellationToken ct)
    {
        var goals = await _goalRepo.GetByUserIdAsync(userId, ct);

        return goals.Select(g => new GoalDto
        {
            Id = g.Id,
            GoalName = g.GoalName,
            TargetAmount = g.TargetAmount,
            TargetDate = g.TargetDate,
            CurrentSavings = g.CurrentSavings,
            Status = g.Status
        });
    }

    // =========================
    // UPDATE GOAL
    // =========================
    public async Task<GoalDto> UpdateGoalAsync(
        int id,
        UpdateGoalRequestDto request,
        Guid userId,
        CancellationToken ct)
    {
        var goal = await _goalRepo.GetByIdAsync(id, ct);

        if (goal == null)
            throw new InvalidOperationException("Goal not found");

        if (goal.UserId != userId)
            throw new UnauthorizedAccessException();

        if (!string.IsNullOrEmpty(request.GoalName))
            goal.GoalName = request.GoalName;

        if (request.TargetAmount.HasValue)
            goal.TargetAmount = request.TargetAmount.Value;

        if (request.TargetDate.HasValue)
            goal.TargetDate = DateTime.SpecifyKind(request.TargetDate.Value, DateTimeKind.Utc);

        if (request.CurrentSavings.HasValue)
            goal.CurrentSavings = request.CurrentSavings.Value;

        var updated = await _goalRepo.UpdateAsync(goal, ct);

        if (updated == null)
            throw new InvalidOperationException("Update failed");

        return new GoalDto
        {
            Id = updated.Id,
            GoalName = updated.GoalName,
            TargetAmount = updated.TargetAmount,
            TargetDate = updated.TargetDate,
            CurrentSavings = updated.CurrentSavings,
            Status = updated.Status
        };
    }

    // =========================
    // DELETE GOAL
    // =========================
    public async Task<bool> DeleteGoalAsync(
        int id,
        Guid userId,
        CancellationToken ct)
    {
        return await _goalRepo.SoftDeleteAsync(id, userId, ct);
    }
}