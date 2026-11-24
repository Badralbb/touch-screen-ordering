using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace TouchScreenOrdering.Ui;

/// <summary>GDI+ drawing helpers. All coordinates here are device pixels.</summary>
internal static class Draw
{
    public static void Prepare(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
    }

    public static GraphicsPath RoundRect(RectangleF r, float radius)
    {
        var path = new GraphicsPath();
        float d = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
        if (d < 1f)
        {
            path.AddRectangle(r);
            return path;
        }
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void FillRound(Graphics g, RectangleF r, float radius, Color color)
    {
        using var brush = new SolidBrush(color);
        using var path = RoundRect(r, radius);
        g.FillPath(brush, path);
    }

    public static void StrokeRound(Graphics g, RectangleF r, float radius, Color color, float width)
    {
        r.Inflate(-width / 2f, -width / 2f);
        using var pen = new Pen(color, width);
        using var path = RoundRect(r, Math.Max(0, radius - width / 2f));
        g.DrawPath(pen, path);
    }

    public static void Text(Graphics g, string text, Font font, Color color, RectangleF r,
        StringAlignment align = StringAlignment.Near,
        StringAlignment lineAlign = StringAlignment.Center,
        bool wrap = false)
    {
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        format.Alignment = align;
        format.LineAlignment = lineAlign;
        format.FormatFlags |= StringFormatFlags.NoClip;
        if (wrap)
        {
            format.FormatFlags &= ~StringFormatFlags.NoWrap;
            format.Trimming = StringTrimming.Word;
        }
        else
        {
            format.FormatFlags |= StringFormatFlags.NoWrap;
            format.Trimming = StringTrimming.None;
        }
        using var brush = new SolidBrush(color);
        g.DrawString(text, font, brush, r, format);
    }

    private static float Measure(Graphics g, string text, Font font) =>
        g.MeasureString(text, font, PointF.Empty, StringFormat.GenericTypographic).Width;

    /// <summary>An icon and a line of text side by side, centered in <paramref name="r"/>.</summary>
    public static void IconText(Graphics g, RectangleF r, Glyph? glyph, float iconSize, float stroke,
        string text, Font font, Color color, float gap, bool iconAfter = false)
    {
        float tw = Measure(g, text, font);
        float iw = glyph.HasValue ? iconSize + gap : 0;
        float x = r.X + (r.Width - tw - iw) / 2f;
        float cy = r.Y + r.Height / 2f;

        float textX = iconAfter ? x : x + iw;
        float iconX = iconAfter ? x + tw + gap : x;

        if (glyph.HasValue)
            Glyphs.Draw(g, glyph.Value, new RectangleF(iconX, cy - iconSize / 2f, iconSize, iconSize), color, stroke);
        Text(g, text, font, color, new RectangleF(textX, r.Y, tw + 4, r.Height));
    }

    /// <summary>An icon stacked over a line of text, centered in <paramref name="r"/>.</summary>
    public static void IconAboveText(Graphics g, RectangleF r, Glyph glyph, float iconSize, float stroke, Color iconColor,
        string text, Font font, Color textColor, float gap)
    {
        float th = font.Size * 1.2f;
        float y = r.Y + (r.Height - iconSize - gap - th) / 2f;
        Glyphs.Draw(g, glyph, new RectangleF(r.X + (r.Width - iconSize) / 2f, y, iconSize, iconSize), iconColor, stroke);
        Text(g, text, font, textColor, new RectangleF(r.X, y + iconSize + gap, r.Width, th), StringAlignment.Center);
    }
}
