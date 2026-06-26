using System;

namespace FinSenseAPI.DTOs.Goal;

public class UpdateGoalRequestDto
{
    public string? GoalName { get; set; }
    public decimal? TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public decimal? CurrentSavings { get; set; }
}
