using System;

namespace FinSenseAPI.DTOs.Chat;

public class ChatResponseDto
{
    public string Reply { get; set; }
    public bool IsGuest { get; set; }
    public int MessagesRemaining { get; set; }
    public bool LimitReached { get; set; }

}
