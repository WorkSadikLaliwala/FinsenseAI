using FinSenseAPI.Config;
using FinSenseAPI.DTOs.Auth;
using FinSenseAPI.Helpers;
using FinSenseAPI.Models;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinSenseAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordResetTokenRepository _prtRepo;
    private readonly IPasswordHasher<User> _hasher;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IUserRepository userRepo, IPasswordResetTokenRepository prtRepo, IPasswordHasher<User> hasher, IOptions<JwtOptions> jwtOptions)
    {
        _userRepo = userRepo;
        _prtRepo = prtRepo;
        _hasher = hasher;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            PasswordHash = string.Empty
        };

        // Hash and store password
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        await _userRepo.CreateAsync(user);

        var token = JwtHelper.GenerateToken(user, _jwtOptions.Secret, _jwtOptions.ExpiryDays);
        var refresh = JwtHelper.GenerateRefreshToken();
        user.RefreshToken = refresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);
        await _userRepo.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.ExpiryDays),
            RefreshToken = refresh
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponseDto();
        }

        // Check active status
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Your account has been deactivated.");
        }

        // verify password
        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return new AuthResponseDto();
        }

        var token = JwtHelper.GenerateToken(user, _jwtOptions.Secret, _jwtOptions.ExpiryDays);
        var refresh = JwtHelper.GenerateRefreshToken();
        user.RefreshToken = refresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);
        await _userRepo.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.ExpiryDays),
            RefreshToken = refresh
        };
    }

    public Task ForgotPasswordAsync(string email)
    {
        // generate token and store
        return ForgotPasswordAsync(email, CancellationToken.None);
    }

    public Task ResetPasswordAsync(string token, string newPassword)
    {
        return ResetPasswordAsync(token, newPassword, CancellationToken.None);
    }

    // new overloads with cancellation tokens
    public async Task<string> ForgotPasswordAsync(string email, CancellationToken ct)
    {
        var user = await _userRepo.GetByEmailAsync(email, ct);
        if (user == null) return string.Empty;

        var prt = new PasswordResetToken
        {
            Token = Guid.NewGuid(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            IsUsed = false
        };

        await _prtRepo.CreateAsync(prt, ct);

        return prt.Token.ToString(); // return token directly
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct)
    {
        if (!Guid.TryParse(token, out var gToken)) return;

        var prt = await _prtRepo.GetByTokenAsync(gToken, ct);
        if (prt == null) return;
        if (prt.IsUsed) return;
        if (prt.ExpiresAt < DateTime.UtcNow) return;

        var user = await _userRepo.GetByIdAsync(prt.UserId, ct);
        if (user == null) return;

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        await _userRepo.UpdateAsync(user, ct);

        await _prtRepo.MarkAsUsedAsync(prt, ct);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepo.GetByRefreshTokenAsync(refreshToken);
        if (user == null || user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            return new AuthResponseDto();
        }

        var token = JwtHelper.GenerateToken(user, _jwtOptions.Secret, _jwtOptions.ExpiryDays);
        var newRefresh = JwtHelper.GenerateRefreshToken();
        user.RefreshToken = newRefresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);
        await _userRepo.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.ExpiryDays),
            RefreshToken = newRefresh
        };
    }
}
