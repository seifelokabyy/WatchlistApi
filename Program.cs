using WatchlistApi.Models;
using WatchlistApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWatchlistStore, WatchlistStore>();

var app = builder.Build();

// POST /watchlist-items - add a new watchlist item.
app.MapPost("/watchlist-items", (CreateWatchlistItemRequest request, IWatchlistStore store) =>
{
    var errors = new Dictionary<string, string[]>();

    var symbol = request.Symbol?.Trim();
    if (string.IsNullOrWhiteSpace(symbol))
    {
        errors["symbol"] = new[] { "Symbol is required." };
    }
    else if (symbol.Length > 10)
    {
        errors["symbol"] = new[] { "Symbol must be at most 10 characters." };
    }

    if (request.TargetPrice is null)
    {
        errors["targetPrice"] = new[] { "TargetPrice is required." };
    }
    else if (request.TargetPrice <= 0)
    {
        errors["targetPrice"] = new[] { "TargetPrice must be greater than zero." };
    }

    if (request.Note is not null && request.Note.Length > 250)
    {
        errors["note"] = new[] { "Note must be at most 250 characters." };
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var item = new WatchlistItem
    {
        Symbol = symbol!.ToUpperInvariant(),
        TargetPrice = request.TargetPrice!.Value,
        Note = request.Note
    };

    if (!store.TryAdd(item, out _))
    {
        return Results.Conflict(new { message = $"An item with symbol '{item.Symbol}' already exists." });
    }

    return Results.Created($"/watchlist-items/{item.Id}", item);
});

// GET /watchlist-items - list every saved item.
app.MapGet("/watchlist-items", (IWatchlistStore store) => Results.Ok(store.GetAll()));

// GET /watchlist-items/best-pair?targetTotal={amount}
app.MapGet("/watchlist-items/best-pair", (decimal? targetTotal, IWatchlistStore store) =>
{
    if (targetTotal is null || targetTotal <= 0)
    {
        var errors = new Dictionary<string, string[]>
        {
            ["targetTotal"] = new[] { "targetTotal is required and must be greater than zero." }
        };
        return Results.ValidationProblem(errors);
    }

    // Snapshot first, then calculate on the copy, so the saved watchlist
    // is never touched while we search for the best pair.
    var snapshot = store.GetAll();
    var result = BestPairFinder.Find(snapshot, targetTotal.Value);

    return Results.Ok(new
    {
        items = result.Items,
        combinedTargetPrice = result.CombinedTargetPrice,
        message = result.Message
    });
});

app.Run();

// Exposes the generated Program class for WebApplicationFactory-based tests,
// if any get added later.
public partial class Program { }
