namespace FinSenseAPI.Models;

public class UploadSession
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }          // null = guest
    public string FileName { get; set; }
    public string? FileHash { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsGuest { get; set; }
    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    public DateTime? ExpiresAt { get; set; }

}
