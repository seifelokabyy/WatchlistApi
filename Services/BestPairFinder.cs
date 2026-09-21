using WatchlistApi.Models;

namespace WatchlistApi.Services;

/// <summary>
/// Pure calculation, kept separate from storage on purpose: given a
/// snapshot of items and a target total, finds the pair of two *different*
/// items whose combined targetPrice is as close as possible to
/// targetTotal without going over it.
/// </summary>
public static class BestPairFinder
{
    public static BestPairResult Find(IReadOnlyList<WatchlistItem> items, decimal targetTotal)
    {
        WatchlistItem? bestA = null;
        WatchlistItem? bestB = null;
        decimal bestSum = -1;
        string[]? bestSortedSymbols = null;

        // O(n^2) brute force over every unordered pair. The watchlist is
        // expected to be small, so this is simple and easy to verify by
        // reading it, which matters more here than raw performance.
        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                var a = items[i];
                var b = items[j];
                var sum = a.TargetPrice + b.TargetPrice;

                if (sum > targetTotal)
                {
                    continue;
                }

                var sortedSymbols = new[] { a.Symbol, b.Symbol }
                    .OrderBy(s => s, StringComparer.Ordinal)
                    .ToArray();

                var isBetter =
                    sum > bestSum ||
                    (sum == bestSum && IsAlphabeticallyEarlier(sortedSymbols, bestSortedSymbols!));

                if (isBetter)
                {
                    bestSum = sum;
                    bestA = a;
                    bestB = b;
                    bestSortedSymbols = sortedSymbols;
                }
            }
        }

        if (bestA is null || bestB is null)
        {
            return new BestPairResult
            {
                Items = new List<WatchlistItem>(),
                CombinedTargetPrice = null,
                Message = "No matching pair"
            };
        }

        var orderedPair = new[] { bestA, bestB }
            .OrderBy(x => x.Symbol, StringComparer.Ordinal)
            .ToList();

        return new BestPairResult
        {
            Items = orderedPair,
            CombinedTargetPrice = bestSum,
            Message = null
        };
    }

    /// <summary>
    /// Compares two already-sorted 2-symbol arrays the way the assignment
    /// describes ties: compare the first symbol, and if those match,
    /// compare the second.
    /// </summary>
    private static bool IsAlphabeticallyEarlier(string[] candidate, string[] currentBest)
    {
        for (var k = 0; k < candidate.Length; k++)
        {
            var comparison = string.CompareOrdinal(candidate[k], currentBest[k]);
            if (comparison != 0)
            {
                return comparison < 0;
            }
        }

        return false;
    }
}
