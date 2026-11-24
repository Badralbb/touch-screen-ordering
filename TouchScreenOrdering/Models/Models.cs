using System.Globalization;

namespace TouchScreenOrdering.Models;

internal enum DineOption
{
    EatHere,
    TakeAway,
}

internal sealed record Category(string Id, string Name);

internal sealed record Dish(
    string Id,
    string Name,
    string Description,
    decimal Price,
    string Tag,
    string[] Categories,
    bool HasExtras,
    string? LongDescription = null,
    string? Allergens = null);

internal sealed record SizeOption(string Id, string Name, decimal Add);

internal sealed record ExtraOption(string Id, string Name, decimal Price);

internal sealed class CartLine
{
    public required string DishId { get; init; }
    public required string Name { get; init; }
    public required string Detail { get; init; }
    public required decimal UnitPrice { get; init; }
    public string SizeId { get; init; } = "regular";
    public string[] ExtraIds { get; init; } = [];
    public int Qty { get; set; } = 1;
    public decimal LineTotal => UnitPrice * Qty;
}

internal static class Money
{
    public static string Format(decimal value) => "$" + value.ToString("0.00", CultureInfo.InvariantCulture);
}
