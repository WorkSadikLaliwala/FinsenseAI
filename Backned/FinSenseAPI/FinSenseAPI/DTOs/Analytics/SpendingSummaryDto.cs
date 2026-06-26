using System;
using System.Collections.Generic;

namespace FinSenseAPI.DTOs.Analytics;

public class SpendingSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal TotalSpending { get; set; }
    public decimal NetSavings { get; set; }
    public List<CategoryBreakdownDto> ByCategory { get; set; }
    public List<FinSenseAPI.DTOs.Upload.TransactionDto> TopTransactions { get; set; }

}
