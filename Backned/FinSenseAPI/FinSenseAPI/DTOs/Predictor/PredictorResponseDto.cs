using System;

namespace FinSenseAPI.DTOs.Predictor;

public class PredictorResponseDto
{
    public decimal ProjectedBalance { get; set; }
    public decimal TotalSpentSoFar { get; set; }
    public decimal ProjectedFutureSpend { get; set; }
    public decimal DailyAverage { get; set; }
    public int DaysRemaining { get; set; }
    public decimal MonthlyIncome { get; set; }
    public string Status { get; set; }
    public string AICommentary { get; set; }
    public string TopSpendingCategory { get; set; }

}
