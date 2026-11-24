using TouchScreenOrdering.Models;
using TouchScreenOrdering.Ui;
using static TouchScreenOrdering.Ui.Theme;

namespace TouchScreenOrdering.Screens;

/// <summary>6 · Order confirmed — order number, then resets to the welcome screen.</summary>
internal sealed class DoneScreen : ScreenBase
{
    private const int ResetSeconds = 20;

    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private readonly TextItem _countdown;
    private int _remaining = ResetSeconds;

    public DoneScreen(KioskContext kiosk) : base(kiosk, Ink)
    {
        this.Box(440, 200, 200, 200, Accent, 100);
        this.Icon(Glyph.Check, 484, 244, 112, White, 2.5f);
        this.Label("Thank you!", Display(96), White, 64, 522, 952, StringAlignment.Center);
        this.Label("Your order is being prepared", Body(32), OnDarkMuted, 64, 618, 952, StringAlignment.Center);

        var card = this.Panel(64, 734, 952, 651, White, 48);
        card.Label("Your order number", Body(30, 600), Muted, 0, 82, 952, StringAlignment.Center);
        card.Label(State.LastOrderNumber.ToString(), Display(240), Accent, 0, 236, 952, StringAlignment.Center);
        card.Label("Watch the screen — we'll call your number when it's ready", Body(28), Muted, 0, 389, 952,
            StringAlignment.Center);
        InfoBox(card, 64, "Estimated wait", "[X] min");
        InfoBox(card, 486, "Paid", Money.Format(State.LastPaid));

        this.Label("Please take your receipt from the printer below", Body(28), OnDarkMuted, 64, 1450, 952,
            StringAlignment.Center);

        this.Button(64, 1691, 952, 112, White, 56, Finish)
            .IconText(null, 0, 0, "Done", Body(34, 700), Ink, 0);
        _countdown = this.Label("", Body(24), CheckOff, 64, 1841, 952, StringAlignment.Center);

        UpdateCountdown();
        _timer.Tick += (_, _) =>
        {
            _remaining--;
            if (_remaining <= 0)
                Finish();
            else
                UpdateCountdown();
        };
        _timer.Start();
    }

    private static void InfoBox(Tile card, float x, string label, string value)
    {
        card.Box(x, 454, 402, 133, Surface, 28);
        card.Label(label, Body(22), Muted, x + 32, 495.5f, 340);
        card.Label(value, Body(36, 700), Ink, x + 32, 537, 340);
    }

    private void UpdateCountdown()
    {
        _countdown.Text = $"This screen resets in {_remaining} seconds";
        Invalidate();
    }

    private void Finish()
    {
        _timer.Stop();
        State.ResetOrder();
        Kiosk.Go(ScreenId.Welcome);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _timer.Dispose();
        base.Dispose(disposing);
    }
}
