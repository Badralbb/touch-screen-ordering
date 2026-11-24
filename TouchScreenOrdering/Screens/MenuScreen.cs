using TouchScreenOrdering.Models;
using TouchScreenOrdering.Ui;
using static TouchScreenOrdering.Ui.Theme;

namespace TouchScreenOrdering.Screens;

/// <summary>2 · Menu — category rail, item grid, order summary bar.</summary>
internal sealed class MenuScreen : ScreenBase
{
    private const float CardW = 370, CardH = 499, ColGap = 28, RowGap = 28;

    private readonly List<(Category Category, Tile Tile)> _categoryTiles = new();
    private readonly TextItem _title;
    private readonly ScrollView _grid;

    public MenuScreen(KioskContext kiosk) : base(kiosk, White)
    {
        BuildHeader();

        // Category rail
        var rail = this.Panel(0, 130, 232, 1606, Rail);
        for (int i = 0; i < Catalog.Categories.Count; i++)
        {
            var category = Catalog.Categories[i];
            var tile = rail.Button(16, 24 + i * 148, 200, 136, Color.Transparent, 28, () => SelectCategory(category.Id));
            tile.Draws.Add(g => PaintCategory(g, category));
            _categoryTiles.Add((category, tile));
        }

        // Title
        _title = this.Label("", Display(64, 700), Ink, 272, 208, 768);
        this.Label("Tap an item to customize it", Body(26), Muted, 272, 271, 768);

        // Item grid (scrolls by dragging)
        _grid = this.Place(new ScrollView(White), 232, 300, 848, 1436);

        BuildOrderBar();
        SelectCategory(State.ActiveCategory);
    }

    private void BuildHeader()
    {
        var pillFont = Body(26, 600);
        float w = 24 + 28 + 10 + TextW("Start over", pillFont) + 24;
        this.Button(40, 28, w, 72, Surface, 36, () =>
            {
                State.ResetOrder();
                Kiosk.Go(ScreenId.Welcome);
            })
            .IconText(Glyph.Restart, 28, 2.25f, "Start over", pillFont, Ink, 10);

        this.Label("Grill House", Display(36), Ink, 340, 64, 400, StringAlignment.Center);

        // Dine option badge
        var dineFont = Body(24, 600);
        float dw = 24 + 26 + 10 + TextW(State.DineLabel, dineFont) + 24;
        var badge = this.Panel(1040 - dw, 32, dw, 64, AccentSoft, 32);
        badge.IconText(State.Dine == DineOption.EatHere ? Glyph.Utensils : Glyph.Bag, 26, 2f,
            State.DineLabel, dineFont, AccentDark, 10);

        this.Box(0, 128, 1080, 2, Divider);
    }

    private void BuildOrderBar()
    {
        this.Box(0, 1736, 1080, 2, Divider);

        // Bag icon with item-count badge
        this.Box(40, 1768, 88, 88, Surface, 28);
        this.Icon(Glyph.Bag, 62, 1790, 44, Ink, 2f);
        int count = State.ItemCount;
        if (count > 0)
        {
            var badgeFont = Body(22, 700);
            string text = count.ToString();
            float bw = Math.Max(40, TextW(text, badgeFont) + 20);
            this.Box(136 - bw, 1760, bw, 40, Accent, 20);
            this.Label(text, badgeFont, White, 136 - bw, 1780, bw, StringAlignment.Center);
        }

        this.Label("Your order", Body(24), Muted, 148, 1798, 400);
        this.Label(Money.Format(State.Subtotal), Body(40, 700), Ink, 148, 1840, 400);

        var font = Body(34, 700);
        float w = 56 + TextW("View order", font) + 16 + 36 + 56;
        this.Button(1040 - w, 1768, w, 112, Accent, 56, () => Kiosk.Go(ScreenId.Cart))
            .IconText(Glyph.ArrowRight, 36, 2.5f, "View order", font, White, 16, iconAfter: true);
    }

    private void SelectCategory(string id)
    {
        State.ActiveCategory = id;
        foreach (var (category, tile) in _categoryTiles)
        {
            tile.Fill = category.Id == id ? Ink : Color.Transparent;
            tile.Invalidate();
        }
        _title.Text = Catalog.Categories.First(c => c.Id == id).Name;
        Invalidate();
        BuildGrid();
    }

    private void PaintCategory(Graphics g, Category category)
    {
        bool on = category.Id == State.ActiveCategory;
        // dot 56 + gap 14 + label ≈ 101 tall, centered in 136
        Draw.FillRound(g, RF(72, 17.5f, 56, 56), F(18), on ? Accent : DotOff);
        Draw.Text(g, category.Name, Body(26, on ? 700 : 600), on ? White : Ink, RF(0, 87.5f, 200, 31),
            StringAlignment.Center);
    }

    private void BuildGrid()
    {
        _grid.Clear();
        var dishes = Catalog.InCategory(State.ActiveCategory).ToList();
        for (int i = 0; i < dishes.Count; i++)
        {
            var dish = dishes[i];
            float x = 40 + (i % 2) * (CardW + ColGap);
            float y = 18 + (i / 2) * (CardH + RowGap);
            var card = _grid.Content.Button(x, y, CardW, CardH, White, 32, () => OpenDish(dish), Divider, 2);
            PaintCard(card, dish);
        }
        int rows = (dishes.Count + 1) / 2;
        _grid.SetContentHeight(18 + rows * (CardH + RowGap) - RowGap + 40);
        _grid.ScrollTo(0);
    }

    private static void PaintCard(Tile card, Dish dish)
    {
        // Photo area
        card.ClippedBox(0, 0, CardW, 248, Surface);
        card.Icon(Glyph.Image, (CardW - 56) / 2, (248 - 56) / 2, 56, Subtle, 1.5f);
        if (!string.IsNullOrEmpty(dish.Tag))
        {
            var tagFont = Body(20, 600);
            float tw = TextW(dish.Tag, tagFont) + 32;
            card.Box(20, 20, tw, 40, Ink, 20);
            card.Label(dish.Tag, tagFont, White, 20, 40, tw, StringAlignment.Center);
        }

        // Text
        card.Label(dish.Name, Body(32, 700), Ink, 28, 291, CardW - 56);
        card.Paragraph(dish.Description, Body(22), Muted, 28, 319, CardW - 56, 64);

        // Price + add
        card.Label(Money.Format(dish.Price), Body(32, 700), Ink, 28, 435, 200);
        card.Box(CardW - 28 - 72, 399, 72, 72, Accent, 36);
        card.Icon(Glyph.Plus, CardW - 28 - 72 + 18, 417, 36, White, 2.5f);
    }

    private void OpenDish(Dish dish)
    {
        State.CurrentDish = dish;
        State.EditingLine = null;
        Kiosk.Go(ScreenId.Item);
    }
}
