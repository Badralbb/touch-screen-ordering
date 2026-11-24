# Grill House ordering kiosk (Windows Forms)

The WinForms UI for the touch-screen ordering kiosk. It is built from the "Food Ordering Kiosk" design (1080×1920 portrait, 6 screens).

| # | Screen | File |
|---|--------|------|
| 1 | Welcome: choose eat here or take away | `Screens/WelcomeScreen.cs` |
| 2 | Menu: category rail, item grid, order bar | `Screens/MenuScreen.cs` |
| 3 | Customize item: size, extras, quantity | `Screens/ItemScreen.cs` |
| 4 | Your order: quantities, upsell, totals | `Screens/CartScreen.cs` |
| 5 | Payment: choose a payment method | `Screens/PaymentScreen.cs` |
| 6 | Order confirmed: order number, resets after 20 s | `Screens/DoneScreen.cs` |

## Run

Requires the .NET 9 SDK.

```powershell
dotnet run --project TouchScreenOrdering
```

* The window scales the 1080×1920 design to fit your monitor.
* **F11** switches to full screen (kiosk mode). **Esc** leaves full screen.
* `dotnet run --project TouchScreenOrdering -- --kiosk` starts in full screen.
* `dotnet run --project TouchScreenOrdering -- --screenshots out` saves a PNG of every screen to `out/`.

> If Windows says "An Application Control policy has blocked this file", Smart App Control
> is blocking the unsigned build. You can run it on a PC without Smart App Control, sign the build,
> or change that setting yourself in Windows Security → App & browser control.

## How the UI is built

* `Ui/Theme.cs` holds the design's colors and fonts. The design uses Inter and Bricolage Grotesque, with a fallback to Segoe UI. It also sets the scale from design px to screen px. Every layout coordinate in the screens is in **design px**, taken directly from the design.
* `Ui/Tile.cs` is the single owner-drawn building block. It is a rounded surface or touch button with press feedback.
* `Ui/ScrollView.cs` scrolls by dragging with a finger or the mouse, or with the mouse wheel. It has no scrollbars.
* `Ui/Glyphs.cs` draws the line icons.
* `Models/` holds the menu data, cart and order state. `Catalog.cs` is the place to plug in the real menu.

Placeholders that the design left open are still marked with `[brackets]`, for example `Tax [YOUR RATE]` (10% for now), `[kcal]` and `[X] min`. The language and accessibility buttons and the payment hand-off are marked `TODO`.
