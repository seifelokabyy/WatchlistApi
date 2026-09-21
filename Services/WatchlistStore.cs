using WatchlistApi.Models;

namespace WatchlistApi.Services;

/// <summary>
/// Simple in-memory store, registered as a singleton so all requests share
/// the same data for the lifetime of the process. A plain lock is enough
/// here: the assignment only calls for in-memory storage with no
/// persistence, and a watchlist is expected to stay small, so we don't
/// need a more elaborate concurrent data structure.
/// </summary>
public class WatchlistStore : IWatchlistStore
{
    private readonly List<WatchlistItem> _items = new();
    private readonly object _lock = new();

    public IReadOnlyList<WatchlistItem> GetAll()
    {
        lock (_lock)
        {
            // Copy out so callers can never mutate our internal list.
            return _items.ToList();
        }
    }

    public bool TryAdd(WatchlistItem item, out WatchlistItem? conflictingItem)
    {
        lock (_lock)
        {
            conflictingItem = _items.FirstOrDefault(existing =>
                string.Equals(existing.Symbol, item.Symbol, StringComparison.OrdinalIgnoreCase));

            if (conflictingItem is not null)
            {
                return false;
            }

            _items.Add(item);
            return true;
        }
    }
}
