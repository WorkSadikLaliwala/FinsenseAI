using System;

namespace FinSenseAPI.DTOs.User;

public class UserProfileDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
