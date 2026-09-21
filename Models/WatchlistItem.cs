namespace WatchlistApi.Models;

/// <summary>
/// A single saved watchlist item, as stored in memory and returned to callers.
/// Symbol is always stored in uppercase.
/// </summary>
public class WatchlistItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Symbol { get; init; }
    public required decimal TargetPrice { get; init; }
    public string? Note { get; init; }
}
