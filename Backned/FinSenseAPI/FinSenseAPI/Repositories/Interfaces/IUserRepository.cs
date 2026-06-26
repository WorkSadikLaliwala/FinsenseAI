using System;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Models;

namespace FinSenseAPI.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task CreateAsync(User user, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task DeleteAllUserDataAsync(Guid userId, CancellationToken ct = default);
}
