namespace FinSenseAPI.DTOs.User;

public class UpdateUserProfileRequestDto
{
    public string? FullName { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
