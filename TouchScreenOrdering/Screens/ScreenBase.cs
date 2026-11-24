using TouchScreenOrdering.Models;
using TouchScreenOrdering.Ui;

namespace TouchScreenOrdering.Screens;

internal enum ScreenId
{
    Welcome,
    Menu,
    Item,
    Cart,
    Payment,
    Done,
}

/// <summary>Shared order state plus navigation, handed to every screen.</summary>
internal sealed class KioskContext
{
    public AppState State { get; } = new();
    public Action<ScreenId> Go { get; set; } = _ => { };
}

/// <summary>A full 1080×1920 (design px) kiosk screen.</summary>
internal abstract class ScreenBase : Tile
{
    protected ScreenBase(KioskContext kiosk, Color background)
    {
        Kiosk = kiosk;
        Fill = background;
        BackColor = background;
        Size = new Size(Theme.S(Theme.DesignWidth), Theme.S(Theme.DesignHeight));
    }

    protected KioskContext Kiosk { get; }
    protected AppState State => Kiosk.State;

    /// <summary>Header used by the order and payment screens: round back button + title.</summary>
    protected void BackHeader(string title, ScreenId back)
    {
        this.Button(40, 24, 80, 80, Theme.Surface, 40, () => Kiosk.Go(back))
            .CenterIcon(Glyph.ArrowLeft, 36, Theme.Ink, 2.5f);
        this.Label(title, Theme.Display(52), Theme.Ink, 144, 64, 600);
        this.Box(0, 128, 1080, 2, Theme.Divider);
    }
}
