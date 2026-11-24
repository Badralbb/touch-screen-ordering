namespace TouchScreenOrdering.Models;

/// <summary>
/// Menu data taken from the design. Replace with your real menu (database / API) later.
/// Values in [brackets] are placeholders the design left open.
/// </summary>
internal static class Catalog
{
    public static readonly IReadOnlyList<Category> Categories =
    [
        new("popular", "Popular"),
        new("burgers", "Burgers"),
        new("bowls", "Bowls"),
        new("sides", "Sides"),
        new("drinks", "Drinks"),
        new("desserts", "Desserts"),
    ];

    public static readonly IReadOnlyList<Dish> Dishes =
    [
        new("smash", "Classic Smash", "Double beef, cheddar, pickles, house sauce", 9.50m, "Best seller",
            ["popular", "burgers"], HasExtras: true,
            LongDescription: "Two smashed beef patties, melted cheddar, pickles and house sauce on a toasted brioche bun.",
            Allergens: "gluten, dairy"),
        new("chicken", "Crispy Chicken", "Buttermilk chicken, slaw, spicy mayo", 8.90m, "",
            ["popular", "burgers"], HasExtras: true),
        new("garden", "Garden Bowl", "Quinoa, roasted veg, avocado, tahini", 10.20m, "Vegetarian",
            ["popular", "bowls"], HasExtras: true),
        new("fries", "Loaded Fries", "Cheese sauce, jalapeño, crispy onion", 5.40m, "",
            ["popular", "sides"], HasExtras: true),
        new("lemonade", "Lemonade", "Fresh-squeezed, lightly sweet", 3.20m, "",
            ["popular", "drinks"], HasExtras: false),
        new("brownie", "Choc Brownie", "Warm, with vanilla ice cream", 4.60m, "New",
            ["popular", "desserts"], HasExtras: false),
        new("rings", "Onion Rings", "[Description]", 4.20m, "", ["sides"], HasExtras: false),
        new("icedtea", "Iced Tea", "[Description]", 2.90m, "", ["drinks"], HasExtras: false),
        new("softserve", "Soft Serve", "[Description]", 2.50m, "", ["desserts"], HasExtras: false),
    ];

    /// <summary>"Complete your meal" suggestions on the order screen.</summary>
    public static readonly IReadOnlyList<string> UpsellIds = ["icedtea", "rings", "softserve"];

    public static readonly IReadOnlyList<SizeOption> Sizes =
    [
        new("regular", "Regular", 0m),
        new("large", "Large", 2.00m),
    ];

    public static readonly IReadOnlyList<ExtraOption> Extras =
    [
        new("cheese", "Extra cheese", 1.00m),
        new("bacon", "Crispy bacon", 1.50m),
        new("avocado", "Avocado", 1.50m),
        new("onion", "No onion", 0m),
    ];

    public static Dish Dish(string id) => Dishes.First(d => d.Id == id);

    public static IEnumerable<Dish> InCategory(string categoryId) =>
        Dishes.Where(d => d.Categories.Contains(categoryId));
}
