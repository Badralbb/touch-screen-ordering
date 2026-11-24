using TouchScreenOrdering.Models;
using TouchScreenOrdering.Ui;
using static TouchScreenOrdering.Ui.Theme;

namespace TouchScreenOrdering.Screens;

/// <summary>1 · Welcome — pick eat here or take away.</summary>
internal sealed class WelcomeScreen : ScreenBase
{
    public WelcomeScreen(KioskContext kiosk) : base(kiosk, White)
    {
        // Header: logo + name, language
        this.Panel(64, 48, 72, 72, Accent, 20).CenterIcon(Glyph.Utensils, 40, White, 2f);
        this.Label("Grill House", Display(40), Ink, 152, 84, 500);

        PillButton(Glyph.Globe, "English", y: 48, onTap: () => { /* TODO: language picker */ });

        // Hero placeholder
        this.Panel(64, 168, 952, 880, Surface, 48)
            .IconAboveText(Glyph.Image, 96, 1.5f, Muted, "Hero food photo or promo video", Body(28, 500), Muted, 20);

        // Headline
        var h1 = Display(96);
        this.Label("Order here.", h1, Ink, 64, 1168, 952);
        this.Label("Skip the line.", h1, Ink, 64, 1264, 952);
        this.Label("Where would you like to eat today?", Body(32), Muted, 64, 1347, 952);

        // Dine options
        DineTile(64, Accent, Glyph.Utensils, "Eat here", DineOption.EatHere);
        DineTile(556, Ink, Glyph.Bag, "Take away", DineOption.TakeAway);

        // Footer
        this.Label("Tap a button to start your order", Body(26), Muted, 64, 1828, 560);
        PillButton(Glyph.Accessibility, "Accessibility mode", y: 1792, onTap: () => { /* TODO: accessibility mode */ });
    }

    private void PillButton(Glyph glyph, string text, float y, Action onTap)
    {
        var font = Body(26, 600);
        float w = 28 + 30 + 12 + TextW(text, font) + 28;
        this.Button(1016 - w, y, w, 72, White, 36, onTap, Border, 2)
            .IconText(glyph, 30, 2f, text, font, Ink, 12);
    }

    private void DineTile(float x, Color fill, Glyph glyph, string text, DineOption option)
    {
        this.Button(x, 1423, 460, 300, fill, 40, () =>
            {
                State.Dine = option;
                Kiosk.Go(ScreenId.Menu);
            })
            .IconAboveText(glyph, 80, 1.75f, White, text, Display(52, 700), White, 24);
    }
}
