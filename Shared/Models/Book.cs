namespace Shared.Models;

public class Book
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public bool InStock { get; set; } = true;
    public string? LoanedToUserId { get; set; }
}
