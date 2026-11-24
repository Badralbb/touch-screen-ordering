using System.Drawing.Text;

namespace TouchScreenOrdering.Ui;

/// <summary>
/// Design tokens from the kiosk design (1080×1920 portrait) plus the scale that maps
/// design pixels to screen pixels. All layout code uses design pixels.
/// </summary>
internal static class Theme
{
    public const float DesignWidth = 1080f;
    public const float DesignHeight = 1920f;

    public static float Scale { get; private set; } = 0.5f;

    public static void SetScale(float scale)
    {
        Scale = scale;
        FontCache.Clear();
    }

    /// <summary>Design px → device px (rounded).</summary>
    public static int S(float v) => (int)MathF.Round(v * Scale);

    /// <summary>Design px → device px.</summary>
    public static float F(float v) => v * Scale;

    public static Rectangle R(float x, float y, float w, float h)
    {
        int left = S(x), top = S(y);
        return new Rectangle(left, top, S(x + w) - left, S(y + h) - top);
    }

    public static RectangleF RF(float x, float y, float w, float h) =>
        new(x * Scale, y * Scale, w * Scale, h * Scale);

    // ---- Palette ----------------------------------------------------------
    public static readonly Color Accent = Hex(0xC2410C);
    public static readonly Color AccentDark = Hex(0x9A3412);
    public static readonly Color AccentSoft = Hex(0xFDEBDD);
    public static readonly Color Ink = Hex(0x1A1A1A);
    public static readonly Color Ink2 = Hex(0x3D3D3D);
    public static readonly Color Muted = Hex(0x5C5C5C);
    public static readonly Color Subtle = Hex(0x8A8780);
    public static readonly Color Surface = Hex(0xF3F2EF);
    public static readonly Color Rail = Hex(0xF7F6F3);
    public static readonly Color Divider = Hex(0xEEECE8);
    public static readonly Color Border = Hex(0xE2E0DB);
    public static readonly Color DotOff = Hex(0xE7E5E0);
    public static readonly Color CheckOff = Hex(0xB5B2AB);
    public static readonly Color OnDarkMuted = Hex(0xD4D2CC);
    public static readonly Color White = Color.White;

    private static Color Hex(int rgb) => Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);

    // ---- Fonts ------------------------------------------------------------
    // The design uses Inter (body) and Bricolage Grotesque (display). If they are not
    // installed we fall back to Segoe UI / Segoe UI Black, which ship with Windows.
    private static readonly HashSet<string> Installed =
        new(new InstalledFontCollection().Families.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);

    private static string? First(params string[] names) => names.FirstOrDefault(Installed.Contains);

    private static readonly string BodyFamily = First("Inter", "Segoe UI") ?? FontFamily.GenericSansSerif.Name;

    private static readonly string? BodySemibold = BodyFamily == "Inter"
        ? First("Inter SemiBold", "Inter Semi Bold")
        : First("Segoe UI Semibold");

    private static readonly string DisplayFamily = First("Bricolage Grotesque") ?? First("Segoe UI") ?? BodyFamily;

    private static readonly string? DisplayHeavy = Installed.Contains("Bricolage Grotesque")
        ? First("Bricolage Grotesque ExtraBold")
        : First("Segoe UI Black");

    private static readonly Dictionary<(string, float, FontStyle), Font> FontCache = new();

    private static Font Get(string family, float px, FontStyle style)
    {
        var key = (family, px * Scale, style);
        if (!FontCache.TryGetValue(key, out var font))
        {
            font = new Font(family, Math.Max(1f, px * Scale), style, GraphicsUnit.Pixel);
            FontCache[key] = font;
        }
        return font;
    }

    /// <summary>Body text (Inter) at a CSS pixel size and weight.</summary>
    public static Font Body(float px, int weight = 400) => weight switch
    {
        >= 700 => Get(BodyFamily, px, FontStyle.Bold),
        >= 600 => BodySemibold != null ? Get(BodySemibold, px, FontStyle.Regular) : Get(BodyFamily, px, FontStyle.Bold),
        _ => Get(BodyFamily, px, FontStyle.Regular),
    };

    /// <summary>Display text (Bricolage Grotesque) at a CSS pixel size and weight.</summary>
    public static Font Display(float px, int weight = 800) =>
        weight >= 800 && DisplayHeavy != null
            ? Get(DisplayHeavy, px, FontStyle.Regular)
            : Get(DisplayFamily, px, FontStyle.Bold);

    // ---- Measuring --------------------------------------------------------
    private static readonly Bitmap MeasureBitmap = new(1, 1);
    private static readonly Graphics MeasureGraphics = CreateMeasureGraphics();

    private static Graphics CreateMeasureGraphics()
    {
        var g = Graphics.FromImage(MeasureBitmap);
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        return g;
    }

    /// <summary>Width of a single line of text, in design px.</summary>
    public static float TextW(string text, Font font) =>
        MeasureGraphics.MeasureString(text, font, PointF.Empty, StringFormat.GenericTypographic).Width / Scale;
}
