namespace TouchScreenOrdering.Models;

/// <summary>Everything the kiosk remembers during one customer's order.</summary>
internal sealed class AppState
{
    /// <summary>Placeholder rate — the design shows "Tax [YOUR RATE]".</summary>
    public const decimal TaxRate = 0.10m;

    public DineOption Dine { get; set; } = DineOption.EatHere;
    public List<CartLine> Cart { get; } = new();
    public string ActiveCategory { get; set; } = "popular";
    public Dish? CurrentDish { get; set; }
    public CartLine? EditingLine { get; set; }

    public int NextOrderNumber { get; private set; } = 127;
    public int LastOrderNumber { get; private set; }
    public decimal LastPaid { get; private set; }
    public string LastPaymentMethod { get; private set; } = "";

    public int ItemCount => Cart.Sum(l => l.Qty);
    public decimal Subtotal => Cart.Sum(l => l.LineTotal);
    public decimal Tax => Math.Round(Subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
    public decimal Total => Subtotal + Tax;
    public string DineLabel => Dine == DineOption.EatHere ? "Eat here" : "Take away";

    /// <summary>The sample order shown in the design (4 items, $24.30), so every screen has content.</summary>
    public void SeedSample()
    {
        AddToCart(Catalog.Dish("smash"), "large", ["cheese"], 1);
        AddToCart(Catalog.Dish("fries"), "regular", [], 1);
        var lemonade = Catalog.Dish("lemonade");
        Cart.Add(new CartLine
        {
            DishId = lemonade.Id,
            Name = lemonade.Name,
            Detail = "Regular · Less ice",
            UnitPrice = lemonade.Price,
            Qty = 2,
        });
    }

    public void ResetOrder()
    {
        Cart.Clear();
        Dine = DineOption.EatHere;
        ActiveCategory = "popular";
        CurrentDish = null;
        EditingLine = null;
    }

    public static decimal UnitPrice(Dish dish, string sizeId, IEnumerable<string> extraIds) =>
        dish.Price
        + Catalog.Sizes.First(s => s.Id == sizeId).Add
        + Catalog.Extras.Where(e => extraIds.Contains(e.Id)).Sum(e => e.Price);

    /// <summary>Adds a configured dish; replaces <see cref="EditingLine"/> when editing.</summary>
    public void AddToCart(Dish dish, string sizeId, IReadOnlyCollection<string> extraIds, int qty)
    {
        var size = Catalog.Sizes.First(s => s.Id == sizeId);
        var extras = Catalog.Extras.Where(e => extraIds.Contains(e.Id)).ToList();
        string detail = string.Join(" · ", new[] { size.Name }.Concat(extras.Select(e => e.Name)));

        int index = -1;
        if (EditingLine != null)
        {
            index = Cart.IndexOf(EditingLine);
            Cart.Remove(EditingLine);
            EditingLine = null;
        }

        var existing = Cart.FirstOrDefault(l => l.DishId == dish.Id && l.Detail == detail);
        if (existing != null)
        {
            existing.Qty = Math.Min(20, existing.Qty + qty);
            return;
        }

        var line = new CartLine
        {
            DishId = dish.Id,
            Name = dish.Name,
            Detail = detail,
            UnitPrice = UnitPrice(dish, sizeId, extraIds),
            SizeId = sizeId,
            ExtraIds = extras.Select(e => e.Id).ToArray(),
            Qty = qty,
        };
        if (index >= 0 && index <= Cart.Count)
            Cart.Insert(index, line);
        else
            Cart.Add(line);
    }

    public void PlaceOrder(string paymentMethod)
    {
        LastOrderNumber = NextOrderNumber++;
        LastPaid = Total;
        LastPaymentMethod = paymentMethod;
        Cart.Clear();
    }
}
