namespace FinSenseAPI.Models;

public class Transaction
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }           // Debit or Credit
    public string Category { get; set; }       // assigned by Claude
    public UploadSession Session { get; set; }

}
