namespace WatchlistApi.Models;

/// <summary>
/// The outcome of a best-pair search: either the two chosen items and their
/// combined price, or an empty result with an explanatory message.
/// </summary>
public class BestPairResult
{
    public List<WatchlistItem> Items { get; init; } = new();
    public decimal? CombinedTargetPrice { get; init; }
    public string? Message { get; init; }
}
