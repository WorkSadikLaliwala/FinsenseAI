using FinSenseAPI.DTOs.User;
using FinSenseAPI.Models;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinSenseAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(IUserRepository userRepo,IPasswordHasher<User> hasher)
    {
        _userRepo = userRepo;
        _hasher = hasher;
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null) return null;
        return new UserProfileDto { FullName = user.FullName, Email = user.Email, CreatedAt = user.CreatedAt };
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateUserProfileRequestDto request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (string.IsNullOrEmpty(request.CurrentPassword) || user.PasswordHash != request.CurrentPassword)
            {
                return null; // signal incorrect current password
            }

            user.PasswordHash = request.NewPassword; // TODO: hash in real implementation
        }

        if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;

        await _userRepo.UpdateAsync(user, ct);

        return new UserProfileDto { FullName = user.FullName, Email = user.Email, CreatedAt = user.CreatedAt };
    }

    public async Task<bool> DeleteAccountAsync(Guid userId, DeleteAccountRequestDto request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null) return false;

        var ispassword =  _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (ispassword == PasswordVerificationResult.Failed)

        {
            return false;
        }

        await _userRepo.DeleteAllUserDataAsync(userId, ct);

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.Email = $"deleted+{user.Id}@example.invalid";
        await _userRepo.UpdateAsync(user, ct);

        return true;
    }
}
