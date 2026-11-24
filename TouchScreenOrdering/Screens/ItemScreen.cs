using TouchScreenOrdering.Models;
using TouchScreenOrdering.Ui;
using static TouchScreenOrdering.Ui.Theme;

namespace TouchScreenOrdering.Screens;

/// <summary>3 Â· Customize item â€” size, extras, quantity.</summary>
internal sealed class ItemScreen : ScreenBase
{
    private readonly Dish _dish;
    private readonly HashSet<string> _extras;
    private readonly List<(SizeOption Size, Tile Tile)> _sizeTiles = new();
    private readonly List<(ExtraOption Extra, Tile Tile)> _extraTiles = new();
    private readonly TextItem _qtyText;
    private readonly TextItem _totalText;
    private readonly Tile _stepper;
    private readonly Tile _addButton;
    private string _size;
    private int _qty;

    public ItemScreen(KioskContext kiosk) : base(kiosk, White)
    {
        _dish = State.CurrentDish ?? Catalog.Dishes[0];
        var editing = State.EditingLine;
        _size = editing?.SizeId ?? "regular";
        _extras = editing != null
            ? new HashSet<string>(editing.ExtraIds)
            : _dish.HasExtras ? new HashSet<string> { "cheese" } : new HashSet<string>();
        _qty = editing?.Qty ?? 1;

        // Photo + back
        var photo = this.Panel(0, 0, 1080, 600, Surface);
        photo.IconAboveText(Glyph.Image, 96, 1.5f, Subtle, "Item photo", Body(26, 500), Muted, 16);
        photo.Button(40, 40, 88, 88, White, 44, Back).CenterIcon(Glyph.ArrowLeft, 40, Ink, 2.5f);

        // Name, price, description, chips
        this.Label(_dish.Name, Display(64), Ink, 64, 686, 720);
        this.Label(Money.Format(_dish.Price), Body(40, 700), Ink, 716, 694, 300, StringAlignment.Far);
        this.Paragraph(_dish.LongDescription ?? _dish.Description, Body(28), Muted, 64, 737, 952, 80);

        var chipFont = Body(22, 600);
        float cx = 64;
        foreach (var chip in new[] { "[kcal] kcal", "Contains: " + (_dish.Allergens ?? "[allergens]") })
        {
            float w = TextW(chip, chipFont) + 36;
            this.Box(cx, 836, w, 42, Surface, 21);
            this.Label(chip, chipFont, Ink2, cx, 857, w, StringAlignment.Center);
            cx += w + 12;
        }

        // Size
        this.Label("Choose a size", Body(32, 700), Ink, 64, 942, 600);
        for (int i = 0; i < Catalog.Sizes.Count; i++)
        {
            var size = Catalog.Sizes[i];
            string note = size.Add > 0
                ? "+ " + Money.Format(size.Add)
                : _dish.Categories.Contains("burgers") ? "Single patty meal" : "Standard size";
            var tile = this.Button(64 + i * 486, 982, 466, 132, White, 28, () =>
            {
                _size = size.Id;
                UpdateSelection();
            });
            tile.Draws.Add(g =>
            {
                var color = _size == size.Id ? Ink : Ink2;
                Draw.Text(g, size.Name, Body(30, 700), color, RF(32, 18.5f, 400, 60));
                Draw.Text(g, note, Body(24), color, RF(32, 63, 400, 48));
            });
            _sizeTiles.Add((size, tile));
        }

        // Extras (only for dishes that take them)
        if (_dish.HasExtras)
        {
            var headFont = Body(32, 700);
            this.Label("Add extras", headFont, Ink, 64, 1177, 400);
            this.Label("Â· optional", Body(26, 500), Muted, 64 + TextW("Add extras", headFont) + 10, 1179, 300);

            for (int i = 0; i < Catalog.Extras.Count; i++)
            {
                var extra = Catalog.Extras[i];
                var tile = this.Button(64, 1217 + i * 120, 952, 104, White, 24, () =>
                {
                    if (!_extras.Remove(extra.Id))
                        _extras.Add(extra.Id);
                    UpdateSelection();
                });
                string price = extra.Price > 0 ? "+ " + Money.Format(extra.Price) : "Free";
                tile.Draws.Add(g =>
                {
                    bool on = _extras.Contains(extra.Id);
                    if (on)
                    {
                        Draw.FillRound(g, RF(28, 28, 48, 48), F(12), Accent);
                        Glyphs.Draw(g, Glyph.Check, RF(38, 38, 28, 28), White, 3f);
                    }
                    else
                    {
                        Draw.StrokeRound(g, RF(28, 28, 48, 48), F(12), CheckOff, F(3));
                    }
                    Draw.Text(g, extra.Name, Body(28, 600), Ink, RF(100, 24, 600, 56));
                    Draw.Text(g, price, Body(26), Muted, RF(624, 24, 300, 56), StringAlignment.Far);
                });
                _extraTiles.Add((extra, tile));
            }
        }

        // Bottom bar: quantity + add
        this.Box(0, 1726, 1080, 2, Divider);
        _stepper = this.Panel(64, 1760, 280, 112, Surface, 56);
        _stepper.Button(12, 12, 88, 88, White, 44, () => SetQty(_qty - 1)).CenterIcon(Glyph.Minus, 36, Ink, 2.5f);
        _qtyText = _stepper.Label("", Body(40, 700), Ink, 108, 56, 64, StringAlignment.Center);
        _stepper.Button(180, 12, 88, 88, White, 44, () => SetQty(_qty + 1)).CenterIcon(Glyph.Plus, 36, Ink, 2.5f);

        var addFont = Body(34, 700);
        _addButton = this.Button(372, 1760, 644, 112, Accent, 56, AddToOrder);
        _addButton.Label(editing != null ? "Update order" : "Add to order", addFont, White, 48, 56, 400);
        _totalText = _addButton.Label("", addFont, White, 48, 56, 548, StringAlignment.Far);

        UpdateSelection();
    }

    private void SetQty(int qty)
    {
        _qty = Math.Clamp(qty, 1, 20);
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        foreach (var (size, tile) in _sizeTiles)
        {
            bool on = size.Id == _size;
            tile.Fill = on ? AccentSoft : White;
            tile.BorderColor = on ? Accent : Border;
            tile.BorderWidth = on ? 4 : 2;
            tile.Invalidate();
        }
        foreach (var (extra, tile) in _extraTiles)
        {
            bool on = _extras.Contains(extra.Id);
            tile.BorderColor = on ? Accent : Border;
            tile.BorderWidth = on ? 3 : 2;
            tile.Invalidate();
        }

        _qtyText.Text = _qty.ToString();
        _stepper.Invalidate();
        _totalText.Text = Money.Format(AppState.UnitPrice(_dish, _size, _extras) * _qty);
        _addButton.Invalidate();
    }

    private void Back()
    {
        bool editing = State.EditingLine != null;
        State.EditingLine = null;
        Kiosk.Go(editing ? ScreenId.Cart : ScreenId.Menu);
    }

    private void AddToOrder()
    {
        State.AddToCart(_dish, _size, _extras, _qty);
        Kiosk.Go(ScreenId.Cart);
    }
}
