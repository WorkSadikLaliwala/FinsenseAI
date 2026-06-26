using System;

namespace FinSenseAPI.DTOs.Goal;

public class GoalRequestDto
{
    public string GoalName { get; set; }
    public decimal TargetAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public decimal CurrentSavings { get; set; }
    public Guid SessionId { get; set; }

}
