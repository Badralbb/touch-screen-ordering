using TouchScreenOrdering.Models;
using TouchScreenOrdering.Ui;
using static TouchScreenOrdering.Ui.Theme;

namespace TouchScreenOrdering.Screens;

/// <summary>4 · Your order — lines with quantity, upsell, totals.</summary>
internal sealed class CartScreen : ScreenBase
{
    private const float LinePitch = 226; // 200 row + 2 divider + 24 gap

    private readonly ScrollView _list;
    private readonly TextItem _subtotal;
    private readonly TextItem _tax;
    private readonly TextItem _total;
    private readonly Tile _payButton;

    public CartScreen(KioskContext kiosk) : base(kiosk, White)
    {
        BackHeader("Your order", ScreenId.Menu);

        var dineFont = Body(24, 600);
        float dw = TextW(State.DineLabel, dineFont) + 44;
        this.Box(1040 - dw, 37, dw, 54, AccentSoft, 27);
        this.Label(State.DineLabel, dineFont, AccentDark, 1040 - dw, 64, dw, StringAlignment.Center);

        _list = this.Place(new ScrollView(White), 0, 130, 1080, 1413);

        // Totals + actions
        this.Box(0, 1543, 1080, 2, Divider);
        var rowFont = Body(28);
        this.Label("Subtotal", rowFont, Muted, 64, 1594, 400);
        _subtotal = this.Label("", rowFont, Muted, 616, 1594, 400, StringAlignment.Far);
        this.Label("Tax [YOUR RATE]", rowFont, Muted, 64, 1640, 400);
        _tax = this.Label("", rowFont, Muted, 616, 1640, 400, StringAlignment.Far);
        var totalFont = Body(44, 800);
        this.Label("Total", totalFont, Ink, 64, 1704, 400);
        _total = this.Label("", totalFont, Ink, 616, 1704, 400, StringAlignment.Far);

        var moreFont = Body(30, 700);
        float mw = 44 + TextW("Add more", moreFont) + 44;
        this.Button(64, 1760, mw, 112, Color.Transparent, 56, () => Kiosk.Go(ScreenId.Menu), Ink, 2)
            .IconText(null, 0, 0, "Add more", moreFont, Ink, 0);

        float px = 64 + mw + 24;
        _payButton = this.Button(px, 1760, 1016 - px, 112, Accent, 56, () => Kiosk.Go(ScreenId.Payment));
        _payButton.IconText(Glyph.ArrowRight, 36, 2.5f, "Go to payment", Body(34, 700), White, 16, iconAfter: true);

        BuildList();
        RefreshTotals();
    }

    private void BuildList()
    {
        _list.Clear();
        var c = _list.Content;
        var lines = State.Cart;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            float y = 32 + i * LinePitch;

            c.Box(64, y + 24, 152, 152, Surface, 28);
            c.Icon(Glyph.Image, 118, y + 78, 44, Subtle, 1.5f);

            c.Label(line.Name, Body(32, 700), Ink, 244, y + 61, 540);
            c.Label(line.Detail, Body(24), Muted, 244, y + 103, 540);
            var editFont = Body(24, 600);
            float ew = TextW("Edit", editFont);
            c.Button(238, y + 124, ew + 12, 40, Color.Transparent, 8, () => EditLine(line))
                .Label("Edit", editFont, Accent, 6, 20, ew + 4);

            c.Label(Money.Format(line.LineTotal), Body(32, 700), Ink, 616, y + 52, 400, StringAlignment.Far);
            var stepper = c.Panel(816, y + 88, 200, 80, Surface, 40);
            stepper.Button(8, 8, 64, 64, White, 32, () => ChangeQty(line, -1)).CenterIcon(Glyph.Minus, 28, Ink, 2.5f);
            stepper.Label(line.Qty.ToString(), Body(30, 700), Ink, 72, 40, 56, StringAlignment.Center);
            stepper.Button(128, 8, 64, 64, White, 32, () => ChangeQty(line, +1)).CenterIcon(Glyph.Plus, 28, Ink, 2.5f);

            c.Box(64, y + 200, 952, 2, Divider);
        }

        float sy;
        if (lines.Count == 0)
        {
            c.Label("Your order is empty", Body(32, 600), Muted, 64, 90, 952);
            sy = 150;
        }
        else
        {
            sy = 32 + lines.Count * LinePitch - 24 + 40;
        }

        // Complete your meal
        c.Label("Complete your meal", Body(32, 700), Ink, 64, sy + 19, 600);
        for (int i = 0; i < Catalog.UpsellIds.Count; i++)
        {
            var dish = Catalog.Dish(Catalog.UpsellIds[i]);
            var card = c.Button(64 + i * 324, sy + 59, 304, 295, White, 28, () => AddUpsell(dish), Border, 2);
            card.ClippedBox(0, 0, 304, 160, Surface);
            card.Label(dish.Name, Body(26, 700), Ink, 24, 187.5f, 256);
            card.Label(Money.Format(dish.Price), Body(24), Muted, 24, 243, 160);
            card.Box(224, 215, 56, 56, Accent, 28);
            card.Icon(Glyph.Plus, 238, 229, 28, White, 2.5f);
        }

        _list.SetContentHeight(sy + 59 + 295 + 40);
        c.Invalidate(true);
    }

    private void RefreshTotals()
    {
        _subtotal.Text = Money.Format(State.Subtotal);
        _tax.Text = Money.Format(State.Tax);
        _total.Text = Money.Format(State.Total);

        bool canPay = State.ItemCount > 0;
        _payButton.Fill = canPay ? Accent : OnDarkMuted;
        _payButton.Interactive = canPay;
        Invalidate();
    }

    // List rebuilds run after the click finishes, since they dispose the tapped button.
    private void Rebuild() => BeginInvoke(new Action(() =>
    {
        BuildList();
        RefreshTotals();
    }));

    private void ChangeQty(CartLine line, int delta)
    {
        line.Qty = Math.Min(20, line.Qty + delta);
        if (line.Qty <= 0)
            State.Cart.Remove(line);
        Rebuild();
    }

    private void AddUpsell(Dish dish)
    {
        State.EditingLine = null;
        State.AddToCart(dish, "regular", [], 1);
        Rebuild();
    }

    private void EditLine(CartLine line)
    {
        State.CurrentDish = Catalog.Dish(line.DishId);
        State.EditingLine = line;
        Kiosk.Go(ScreenId.Item);
    }
}
