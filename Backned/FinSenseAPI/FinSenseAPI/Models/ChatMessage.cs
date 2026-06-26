namespace FinSenseAPI.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public string Role { get; set; }           // user or assistant
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }

}
