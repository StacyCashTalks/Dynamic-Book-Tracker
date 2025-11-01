namespace Shared.Models;

public class WatchList
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BookId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
