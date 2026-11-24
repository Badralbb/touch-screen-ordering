using TouchScreenOrdering.Ui;
using static TouchScreenOrdering.Ui.Theme;

namespace TouchScreenOrdering.Screens;

/// <summary>5 · Payment — amount and payment method.</summary>
internal sealed class PaymentScreen : ScreenBase
{
    public PaymentScreen(KioskContext kiosk) : base(kiosk, White)
    {
        BackHeader("Payment", ScreenId.Cart);

        int count = State.ItemCount;
        this.Label("Amount to pay", Body(30), Muted, 64, 228, 952, StringAlignment.Center);
        this.Label(Models.Money.Format(State.Total), Display(144), Ink, 64, 330, 952, StringAlignment.Center);
        this.Label($"{count} item{(count == 1 ? "" : "s")} · {State.DineLabel}", Body(26), Muted, 64, 430, 952,
            StringAlignment.Center);

        this.Label("How would you like to pay?", Body(34, 700), Ink, 64, 545, 952);
        Option(594, primary: true, Glyph.Card, "Card or phone", "Tap, insert or swipe on the reader below", "card");
        Option(842, primary: false, Glyph.Qr, "Scan QR code", "Pay with your banking or wallet app", "qr");
        Option(1090, primary: false, Glyph.Cash, "Pay at the counter", "Cash or card — show your order number", "counter");

        this.Icon(Glyph.ArrowDown, 504, 1737, 72, Accent, 2f);
        this.Label("Card reader is below the screen", Body(26), Muted, 64, 1848, 952, StringAlignment.Center);
    }

    private void Option(float y, bool primary, Glyph glyph, string title, string subtitle, string method)
    {
        var tile = this.Button(64, y, 952, 220, primary ? Accent : White, 40, () => Pay(method),
            primary ? null : Border, primary ? 0 : 2);
        tile.Box(48, 46, 128, 128, primary ? AccentDark : Surface, 32);
        tile.Icon(glyph, 78, 76, 68, primary ? White : Ink, 1.75f);
        tile.Label(title, Body(44, 700), primary ? White : Ink, 216, 90.5f, 700);
        tile.Label(subtitle, Body(26), primary ? White : Muted, 216, 140.5f, 700);
    }

    private void Pay(string method)
    {
        // TODO: hand off to the payment terminal / QR provider here.
        State.PlaceOrder(method);
        Kiosk.Go(ScreenId.Done);
    }
}
