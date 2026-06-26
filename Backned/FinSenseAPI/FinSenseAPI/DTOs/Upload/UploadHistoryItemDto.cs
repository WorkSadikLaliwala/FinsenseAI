using System;

namespace FinSenseAPI.DTOs.Upload;

public class UploadHistoryItemDto
{
    public Guid SessionId { get; set; }
    public Guid? UserId { get; set; }
    public string FileName { get; set; }
    public DateTime UploadedAt { get; set; }
    public int TransactionCount { get; set; }
}
