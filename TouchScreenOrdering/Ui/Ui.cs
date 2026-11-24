namespace TouchScreenOrdering.Ui;

/// <summary>A piece of painted text whose content/color can change at runtime (invalidate the owner after).</summary>
internal sealed class TextItem(string text, Color color)
{
    public string Text { get; set; } = text;
    public Color Color { get; set; } = color;
}

/// <summary>
/// Layout helpers. Every x / y / w / h here is in design px relative to the parent,
/// exactly as in the 1080×1920 design.
/// </summary>
internal static class Ui
{
    public static T Place<T>(this Control parent, T child, float x, float y, float w, float h) where T : Control
    {
        child.Bounds = Theme.R(x, y, w, h);
        parent.Controls.Add(child);
        return child;
    }

    /// <summary>A non-interactive rounded surface that can hold children.</summary>
    public static Tile Panel(this Control parent, float x, float y, float w, float h, Color fill, float radius = 0) =>
        parent.Place(new Tile { Fill = fill, Radius = radius }, x, y, w, h);

    /// <summary>A tappable rounded surface.</summary>
    public static Tile Button(this Control parent, float x, float y, float w, float h, Color fill, float radius,
        Action onTap, Color? border = null, float borderWidth = 0)
    {
        var tile = new Tile
        {
            Fill = fill,
            Radius = radius,
            BorderColor = border ?? Color.Transparent,
            BorderWidth = borderWidth,
            Interactive = true,
        };
        tile.Click += (_, _) => onTap();
        return parent.Place(tile, x, y, w, h);
    }

    /// <summary>A single line of text vertically centered on <paramref name="cy"/>.</summary>
    public static TextItem Label(this Tile t, string text, Font font, Color color, float x, float cy, float w,
        StringAlignment align = StringAlignment.Near)
    {
        var item = new TextItem(text, color);
        float px = font.Size / Theme.Scale;
        t.Draws.Add(g => Draw.Text(g, item.Text, font, item.Color, Theme.RF(x, cy - px, w, px * 2), align));
        return item;
    }

    /// <summary>Wrapping text in a box, top-aligned.</summary>
    public static void Paragraph(this Tile t, string text, Font font, Color color, float x, float y, float w, float h) =>
        t.Draws.Add(g => Draw.Text(g, text, font, color, Theme.RF(x, y, w, h), StringAlignment.Near, StringAlignment.Near, wrap: true));

    /// <summary>A filled rounded rectangle (also used for divider lines).</summary>
    public static void Box(this Tile t, float x, float y, float w, float h, Color fill, float radius = 0) =>
        t.Draws.Add(g => Draw.FillRound(g, Theme.RF(x, y, w, h), Theme.F(radius), fill));

    /// <summary>A filled rectangle clipped to the tile's rounded shape (e.g. a photo area at the top of a card).</summary>
    public static void ClippedBox(this Tile t, float x, float y, float w, float h, Color fill) =>
        t.Draws.Add(g =>
        {
            using var shape = Draw.RoundRect(new RectangleF(0, 0, t.Width, t.Height), Theme.F(t.Radius));
            var state = g.Save();
            g.SetClip(shape, System.Drawing.Drawing2D.CombineMode.Intersect);
            using var brush = new SolidBrush(fill);
            g.FillRectangle(brush, Theme.RF(x, y, w, h));
            g.Restore(state);
        });

    public static void Icon(this Tile t, Glyph glyph, float x, float y, float size, Color color, float stroke = 2f) =>
        t.Draws.Add(g => Glyphs.Draw(g, glyph, Theme.RF(x, y, size, size), color, stroke));

    public static void CenterIcon(this Tile t, Glyph glyph, float size, Color color, float stroke = 2f) =>
        t.Draws.Add(g =>
        {
            float s = Theme.F(size);
            Glyphs.Draw(g, glyph, new RectangleF((t.Width - s) / 2f, (t.Height - s) / 2f, s, s), color, stroke);
        });

    /// <summary>Icon + text centered in the tile (pill buttons).</summary>
    public static void IconText(this Tile t, Glyph? glyph, float iconSize, float stroke, string text, Font font,
        Color color, float gap, bool iconAfter = false) =>
        t.Draws.Add(g => Draw.IconText(g, new RectangleF(0, 0, t.Width, t.Height), glyph, Theme.F(iconSize), stroke,
            text, font, color, Theme.F(gap), iconAfter));

    /// <summary>Icon stacked over text, centered in the tile.</summary>
    public static void IconAboveText(this Tile t, Glyph glyph, float iconSize, float stroke, Color iconColor,
        string text, Font font, Color textColor, float gap) =>
        t.Draws.Add(g => Draw.IconAboveText(g, new RectangleF(0, 0, t.Width, t.Height), glyph, Theme.F(iconSize),
            stroke, iconColor, text, font, textColor, Theme.F(gap)));
}
