using System;

namespace FinSenseAPI.DTOs.Goal;

public class GoalResponseDto
{
    public string Status { get; set; } = string.Empty;

    public decimal AmountRemaining { get; set; }

    public int MonthsRemaining { get; set; }

    public decimal RequiredMonthlySaving { get; set; }

    public decimal CurrentMonthlySurplus { get; set; }

    public decimal Shortfall { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Tip { get; set; } = string.Empty;
}


public class GoalDto
{
    public int Id { get; set; }

    public string GoalName { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public DateTime TargetDate { get; set; }

    public decimal CurrentSavings { get; set; }

    public string Status { get; set; } = string.Empty;
}

