using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Models;

namespace FinSenseAPI.Repositories.Interfaces;

public interface IGoalRepository
{
    Task CreateAsync(Goal goal, CancellationToken ct = default);
    Task<IEnumerable<Goal>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Goal?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Goal?> UpdateAsync(Goal goal, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(int goalId, Guid userId, CancellationToken ct = default);
}
