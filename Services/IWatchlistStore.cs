using WatchlistApi.Models;

namespace WatchlistApi.Services;

public interface IWatchlistStore
{
    /// <summary>Returns a snapshot copy of every saved item.</summary>
    IReadOnlyList<WatchlistItem> GetAll();

    /// <summary>
    /// Tries to add an item. Returns false (and the item that already uses
    /// the symbol) if the symbol is already taken, case-insensitively.
    /// </summary>
    bool TryAdd(WatchlistItem item, out WatchlistItem? conflictingItem);
}
