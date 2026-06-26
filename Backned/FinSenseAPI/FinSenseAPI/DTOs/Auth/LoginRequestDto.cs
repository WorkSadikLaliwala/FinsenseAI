using System;

namespace FinSenseAPI.DTOs.Auth;

public class LoginRequestDto
{
    public string Email { get; set; }
    public string Password { get; set; }

}
