using System;
using System.Collections.Generic;
using FinSenseAPI.DTOs.Analytics;

namespace FinSenseAPI.DTOs.Upload;

public class UploadResponseDto
{
    public Guid SessionId { get; set; }
    public List<TransactionDto> Transactions { get; set; }
    public SpendingSummaryDto Summary { get; set; }
    public bool IsGuest { get; set; }

}
