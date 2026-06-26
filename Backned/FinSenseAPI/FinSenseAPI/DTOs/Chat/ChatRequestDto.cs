using System;
using System.Collections.Generic;

namespace FinSenseAPI.DTOs.Chat;

public class ChatRequestDto
{
    public string Message { get; set; }
    public Guid SessionId { get; set; }
    public List<ChatHistoryItemDto> ChatHistory { get; set; }
    public int GuestMessageCount { get; set; }

}
