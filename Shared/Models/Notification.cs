namespace Shared.Models;

public class Notification
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Book? Book { get; set; }
}
