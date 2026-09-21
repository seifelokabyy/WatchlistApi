namespace WatchlistApi.Models;

/// <summary>
/// The JSON body accepted by POST /watchlist-items.
/// Every field is nullable/optional here on purpose: we run our own
/// validation in the endpoint so we can return clear, specific error
/// messages instead of relying only on model binding defaults.
/// </summary>
public class CreateWatchlistItemRequest
{
    public string? Symbol { get; set; }
    public decimal? TargetPrice { get; set; }
    public string? Note { get; set; }
}
