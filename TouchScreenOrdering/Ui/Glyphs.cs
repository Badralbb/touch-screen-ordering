using System.Drawing.Drawing2D;

namespace TouchScreenOrdering.Ui;

internal enum Glyph
{
    Utensils,
    Globe,
    Image,
    Bag,
    Accessibility,
    Restart,
    Plus,
    Minus,
    ArrowLeft,
    ArrowRight,
    ArrowDown,
    Check,
    Card,
    Qr,
    Cash,
}

/// <summary>
/// Stroke icons drawn on a 24×24 grid (same shapes as the line icons in the design).
/// </summary>
internal static class Glyphs
{
    public static void Draw(Graphics g, Glyph glyph, RectangleF r, Color color, float stroke)
    {
        var state = g.Save();
        g.TranslateTransform(r.X, r.Y);
        g.ScaleTransform(r.Width / 24f, r.Height / 24f);
        using var pen = new Pen(color, stroke)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round,
        };

        switch (glyph)
        {
            case Glyph.Utensils:
            {
                using var fork = new GraphicsPath();
                fork.AddLine(3, 2, 3, 9);
                fork.AddArc(3, 7, 4, 4, 180, -90);
                fork.AddLine(5, 11, 9, 11);
                fork.AddArc(7, 7, 4, 4, 90, -90);
                fork.AddLine(11, 9, 11, 2);
                g.DrawPath(pen, fork);
                g.DrawLine(pen, 7, 2, 7, 22);

                using var knife = new GraphicsPath();
                knife.AddLine(21, 15, 21, 2);
                knife.AddArc(16, 2, 10, 10, 270, -90);
                knife.AddLine(16, 7, 16, 13);
                knife.AddArc(16, 11, 4, 4, 180, -90);
                knife.AddLine(18, 15, 21, 15);
                g.DrawPath(pen, knife);
                g.DrawLine(pen, 21, 15, 21, 22);
                break;
            }
            case Glyph.Globe:
                g.DrawEllipse(pen, 2, 2, 20, 20);
                g.DrawLine(pen, 2, 12, 22, 12);
                g.DrawEllipse(pen, 8.2f, 2, 7.6f, 20);
                break;
            case Glyph.Image:
                Rounded(g, pen, 3, 3, 18, 18, 2);
                g.DrawEllipse(pen, 7, 7, 4, 4);
                Lines(g, pen, 21, 15, 17.9f, 11.9f, 16.5f, 11.3f, 15.1f, 11.9f, 6, 21);
                break;
            case Glyph.Bag:
            {
                using var bag = new GraphicsPath();
                bag.AddLine(6, 2, 3, 6);
                bag.AddLine(3, 6, 3, 20);
                bag.AddArc(3, 18, 4, 4, 180, -90);
                bag.AddLine(5, 22, 19, 22);
                bag.AddArc(17, 18, 4, 4, 90, -90);
                bag.AddLine(21, 20, 21, 6);
                bag.AddLine(21, 6, 18, 2);
                bag.CloseFigure();
                g.DrawPath(pen, bag);
                g.DrawLine(pen, 3, 6, 21, 6);
                g.DrawArc(pen, 8, 6, 8, 8, 0, 180);
                break;
            }
            case Glyph.Accessibility:
                g.DrawEllipse(pen, 10, 2, 4, 4);
                g.DrawLine(pen, 5, 9, 19, 9);
                g.DrawLine(pen, 12, 9, 12, 15);
                Lines(g, pen, 8, 22, 12, 15, 16, 22);
                break;
            case Glyph.Restart:
                g.DrawArc(pen, 3, 3, 18, 18, 180, -311.8f);
                g.DrawLine(pen, 6, 5.3f, 3, 8);
                Lines(g, pen, 3, 3, 3, 8, 8, 8);
                break;
            case Glyph.Plus:
                g.DrawLine(pen, 12, 5, 12, 19);
                g.DrawLine(pen, 5, 12, 19, 12);
                break;
            case Glyph.Minus:
                g.DrawLine(pen, 5, 12, 19, 12);
                break;
            case Glyph.ArrowLeft:
                g.DrawLine(pen, 19, 12, 5, 12);
                Lines(g, pen, 11, 18, 5, 12, 11, 6);
                break;
            case Glyph.ArrowRight:
                g.DrawLine(pen, 5, 12, 19, 12);
                Lines(g, pen, 13, 6, 19, 12, 13, 18);
                break;
            case Glyph.ArrowDown:
                g.DrawLine(pen, 12, 5, 12, 19);
                Lines(g, pen, 19, 12, 12, 19, 5, 12);
                break;
            case Glyph.Check:
                Lines(g, pen, 20, 6, 9, 17, 4, 12);
                break;
            case Glyph.Card:
                Rounded(g, pen, 2, 5, 20, 14, 2);
                g.DrawLine(pen, 2, 10, 22, 10);
                g.DrawLine(pen, 6, 15, 10, 15);
                break;
            case Glyph.Qr:
                Rounded(g, pen, 3, 3, 7, 7, 1);
                Rounded(g, pen, 14, 3, 7, 7, 1);
                Rounded(g, pen, 3, 14, 7, 7, 1);
                g.DrawRectangle(pen, 14, 14, 3, 3);
                Lines(g, pen, 21, 14, 21, 21, 17, 21);
                break;
            case Glyph.Cash:
                Rounded(g, pen, 2, 6, 20, 12, 2);
                g.DrawEllipse(pen, 9.5f, 9.5f, 5, 5);
                g.DrawLine(pen, 6, 12, 6.01f, 12);
                g.DrawLine(pen, 18, 12, 18.01f, 12);
                break;
        }

        g.Restore(state);
    }

    private static void Lines(Graphics g, Pen pen, params float[] xy)
    {
        var points = new PointF[xy.Length / 2];
        for (int i = 0; i < points.Length; i++)
            points[i] = new PointF(xy[i * 2], xy[i * 2 + 1]);
        g.DrawLines(pen, points);
    }

    private static void Rounded(Graphics g, Pen pen, float x, float y, float w, float h, float r)
    {
        using var path = TouchScreenOrdering.Ui.Draw.RoundRect(new RectangleF(x, y, w, h), r);
        g.DrawPath(pen, path);
    }
}
