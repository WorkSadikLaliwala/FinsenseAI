using System;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Models;

namespace FinSenseAPI.Repositories.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task CreateAsync(PasswordResetToken token, CancellationToken ct = default);
    Task<PasswordResetToken?> GetByTokenAsync(Guid token, CancellationToken ct = default);
    Task MarkAsUsedAsync(PasswordResetToken token, CancellationToken ct = default);
}
