# Instrument Watchlist API

A small ASP.NET Core Web API for tracking instruments a user wants to watch,
with an endpoint that finds the best pair of items whose combined target
price stays as close as possible to a given total. Data is stored in memory
and is lost when the app restarts.

## Build and run

```bash
cd WatchlistApi
dotnet build
dotnet run
```

By default the API listens on the URL printed in the console (usually
something like `http://localhost:5000` or `https://localhost:5001` — check
your terminal output, it can vary by machine). Swap in whichever port is
shown when you try the requests below.

## Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/watchlist-items` | Add a watchlist item |
| GET | `/watchlist-items` | List all saved items |
| GET | `/watchlist-items/best-pair?targetTotal={amount}` | Find the closest-matching pair |

## Example requests (curl)

Add items:

```bash
curl -i -X POST http://localhost:5000/watchlist-items \
  -H "Content-Type: application/json" \
  -d '{"symbol":"msft","targetPrice":120,"note":"watch for earnings"}'

curl -i -X POST http://localhost:5000/watchlist-items \
  -H "Content-Type: application/json" \
  -d '{"symbol":"AAPL","targetPrice":80}'

curl -i -X POST http://localhost:5000/watchlist-items \
  -H "Content-Type: application/json" \
  -d '{"symbol":"NVDA","targetPrice":70}'

curl -i -X POST http://localhost:5000/watchlist-items \
  -H "Content-Type: application/json" \
  -d '{"symbol":"IBM","targetPrice":30}'
```

Check duplicate rejection (should return `409 Conflict`):

```bash
curl -i -X POST http://localhost:5000/watchlist-items \
  -H "Content-Type: application/json" \
  -d '{"symbol":"msft","targetPrice":999}'
```

Check validation (should return `400` with field errors):

```bash
curl -i -X POST http://localhost:5000/watchlist-items \
  -H "Content-Type: application/json" \
  -d '{"symbol":"","targetPrice":0}'
```

List items:

```bash
curl -i http://localhost:5000/watchlist-items
```

Best pair — exact match (expect AAPL + MSFT, combined 200):

```bash
curl -i "http://localhost:5000/watchlist-items/best-pair?targetTotal=200"
```

Best pair — closest match (expect AAPL + NVDA, combined 150):

```bash
curl -i "http://localhost:5000/watchlist-items/best-pair?targetTotal=155"
```

Best pair — no match (expect empty items, `null` combined price):

```bash
curl -i "http://localhost:5000/watchlist-items/best-pair?targetTotal=90"
```

Best pair — missing/invalid targetTotal (expect `400`):

```bash
curl -i "http://localhost:5000/watchlist-items/best-pair"
curl -i "http://localhost:5000/watchlist-items/best-pair?targetTotal=0"
```

## Assumptions

- `symbol` is trimmed before validation and storage, so leading/trailing
  spaces around an otherwise valid symbol are not treated as an error.
- The duplicate-symbol check and the uppercase conversion both use
  case-insensitive/invariant comparison, so `msft` and `MSFT` are the same
  item.
- The best-pair endpoint only ever considers two *distinct* saved items —
  never the same item twice — as the assignment specifies.
- No authentication, persistence, or concurrency beyond a basic in-memory
  lock is required, since the assignment explicitly allows data loss on
  restart and doesn't ask for a database.

## An issue I ran through

The trickiest part was the tie-breaking rule for best-pair: "sort the
symbols within each pair alphabetically, then return the pair that comes
first alphabetically." It's easy to only sort the *final* pair for display
and forget that ties must also be broken by comparing the *sorted* symbols
of each candidate pair, not the pair in whatever order it was discovered.
I handled this by building a sorted 2-element symbol array for every
candidate pair during the search itself, and comparing that array against
the current best whenever two pairs have the same combined sum (see
`IsAlphabeticallyEarlier` in `BestPairFinder.cs`).

## What I'd improve with more time

- Add automated tests (e.g. `WebApplicationFactory` integration tests) that
  cover every example in the assignment, instead of only manual curl checks.
- Add OpenAPI/Swagger UI for easier manual exploration.
- Make the best-pair search more efficient for large watchlists (it's
  currently O(n²), which is fine for a small in-memory list but would not
  scale to a very large one).
- Add a `GET /watchlist-items/{id}` and a delete endpoint for completeness.

## How the best-pair logic works

`BestPairFinder.Find` takes a snapshot of the current items and the
requested `targetTotal`, then checks every unordered pair of two different
items (an O(n²) brute-force scan, which is simple to read and verify for a
small in-memory list):

1. Skip any pair whose combined `targetPrice` exceeds `targetTotal`.
2. Among the remaining pairs, keep the one with the highest combined value.
3. If a new pair ties the current best combined value, compare the two
   pairs' symbols after sorting each pair alphabetically, and keep whichever
   pair is alphabetically first.
4. If no pair qualifies (including when fewer than two items exist), return
   an empty `items` array, `combinedTargetPrice: null`, and the message
   `"No matching pair"`.

Because the search only reads a snapshot returned by the store (a copy of
the list, taken under a lock) and never writes back to it, the saved
watchlist is never modified while the calculation runs.

## Tools and resources used

- .NET SDK and ASP.NET Core documentation (docs.microsoft.com / learn.microsoft.com).
- Claude (Anthropic), an AI assistant, was used to help design and write
  the initial version of this project's code and this README, based on
  the assignment's requirements. All code was reviewed and can be
  explained line by line.
