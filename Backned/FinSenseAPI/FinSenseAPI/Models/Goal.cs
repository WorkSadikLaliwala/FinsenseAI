namespace FinSenseAPI.Models;

public class Goal
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string GoalName { get; set; }
    public decimal TargetAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public decimal CurrentSavings { get; set; }
    public string Status { get; set; }         // On Track, At Risk, Not Feasible
    public DateTime CreatedAt { get; set; }
    public User User { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

}
