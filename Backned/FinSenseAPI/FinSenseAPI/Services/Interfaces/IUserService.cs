using System;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.DTOs.User;

namespace FinSenseAPI.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct);
    Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateUserProfileRequestDto request, CancellationToken ct);
    Task<bool> DeleteAccountAsync(Guid userId, DeleteAccountRequestDto request, CancellationToken ct);
}
