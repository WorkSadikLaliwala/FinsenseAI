using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.DTOs.Goal;

namespace FinSenseAPI.Services.Interfaces;

//public interface IGoalService
//{
//    Task<GoalResponseDto> CreateAndEvaluateGoalAsync(GoalRequestDto dto, Guid userId, CancellationToken ct);
//    Task<IEnumerable<GoalResponseDto>> GetUserGoalsAsync(Guid userId, CancellationToken ct);
//    Task<bool> DeleteGoalAsync(int id, Guid userId, CancellationToken ct);
//    Task<GoalResponseDto> UpdateGoalAsync(int id, UpdateGoalRequestDto request, Guid userId, CancellationToken ct);
//}

public interface IGoalService
{
    Task<GoalResponseDto> EvaluateGoalAsync(
        GoalRequestDto dto,
        Guid userId,
        CancellationToken ct);

    Task<IEnumerable<GoalDto>> GetUserGoalsAsync(
        Guid userId,
        CancellationToken ct);

    Task<GoalDto> UpdateGoalAsync(
        int id,
        UpdateGoalRequestDto request,
        Guid userId,
        CancellationToken ct);

    Task<bool> DeleteGoalAsync(
        int id,
        Guid userId,
        CancellationToken ct);
}