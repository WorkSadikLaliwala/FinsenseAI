using System;

namespace FinSenseAPI.Models;

public class PasswordResetToken
{
    public Guid Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public User User { get; set; }
}
