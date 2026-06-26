using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Data;
using FinSenseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FinSenseAPI.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly AppDbContext _db;

    public GoalRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(Goal goal, CancellationToken ct = default)
    {
        await _db.Goals.AddAsync(goal, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Goal>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Goals.Where(g => g.UserId == userId && !g.IsDeleted).ToListAsync(ct);
    }

    public async Task<Goal?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Goals.FindAsync(new object[] { id }, ct);
    }
    public async Task<Goal?> UpdateAsync(Goal goal, CancellationToken ct = default)
    {
        _db.Goals.Update(goal);
        await _db.SaveChangesAsync(ct);
        return goal;
    }

    public async Task<bool> SoftDeleteAsync(int goalId, Guid userId, CancellationToken ct = default)
    {
        var goal = await _db.Goals.FindAsync(new object[] { goalId }, ct);
        if (goal == null) return false;
        if (goal.UserId != userId) return false;
        goal.IsDeleted = true;
        goal.DeletedAt = DateTime.UtcNow;
        _db.Goals.Update(goal);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
