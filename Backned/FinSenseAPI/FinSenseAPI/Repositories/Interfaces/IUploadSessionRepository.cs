using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Models;

namespace FinSenseAPI.Repositories.Interfaces;

public interface IUploadSessionRepository
{
    Task CreateAsync(UploadSession session, CancellationToken ct = default);
    Task<UploadSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<UploadSession>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<UploadSession?> GetByHashAndUserAsync(string hash, Guid? userId, CancellationToken ct = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task DeleteExpiredGuestSessionsAsync(CancellationToken ct = default);
}
