using TouchScreenOrdering.Screens;
using TouchScreenOrdering.Ui;

namespace TouchScreenOrdering;

/// <summary>
/// Hosts one kiosk screen at a time. The 1080×1920 portrait design is scaled to fit the
/// monitor: windowed on a normal PC, or full screen (F11 / --kiosk) on the kiosk itself.
/// </summary>
internal sealed class MainForm : Form
{
    private readonly KioskContext _kiosk = new();
    private ScreenBase? _screen;
    private ScreenId _current = ScreenId.Welcome;
    private bool _fullScreen;

    public MainForm(bool fullScreen)
    {
        Text = "Grill House · Ordering Kiosk";
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.Black;
        KeyPreview = true;
        MaximizeBox = false;
        DoubleBuffered = true;
        StartPosition = FormStartPosition.Manual;

        _kiosk.Go = Go;
        _kiosk.State.SeedSample(); // sample order from the design so every screen has content

        ApplyWindowMode(fullScreen);
        Show(ScreenId.Welcome);
    }

    private void Go(ScreenId id)
    {
        // Defer the swap: navigation is usually triggered by a button on the screen being replaced.
        if (IsHandleCreated)
            BeginInvoke(new Action(() => Show(id)));
        else
            Show(id);
    }

    private void Show(ScreenId id)
    {
        _current = id;
        ScreenBase next = id switch
        {
            ScreenId.Welcome => new WelcomeScreen(_kiosk),
            ScreenId.Menu => new MenuScreen(_kiosk),
            ScreenId.Item => new ItemScreen(_kiosk),
            ScreenId.Cart => new CartScreen(_kiosk),
            ScreenId.Payment => new PaymentScreen(_kiosk),
            ScreenId.Done => new DoneScreen(_kiosk),
            _ => throw new ArgumentOutOfRangeException(nameof(id)),
        };
        next.Location = new Point((ClientSize.Width - next.Width) / 2, (ClientSize.Height - next.Height) / 2);

        SuspendLayout();
        Controls.Add(next);
        var old = _screen;
        _screen = next;
        if (old != null)
        {
            Controls.Remove(old);
            old.Dispose();
        }
        ResumeLayout();
    }

    private void ApplyWindowMode(bool fullScreen)
    {
        _fullScreen = fullScreen;
        var screen = IsHandleCreated ? Screen.FromControl(this) : Screen.FromPoint(Cursor.Position);

        if (fullScreen)
        {
            FormBorderStyle = FormBorderStyle.None;
            var b = screen.Bounds;
            Theme.SetScale(Math.Min(b.Width / Theme.DesignWidth, b.Height / Theme.DesignHeight));
            Bounds = b;
        }
        else
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            var area = screen.WorkingArea;
            var chrome = SizeFromClientSize(new Size(100, 100)) - new Size(100, 100);
            float scale = Math.Min((area.Width - chrome.Width) / Theme.DesignWidth,
                                   (area.Height - chrome.Height) / Theme.DesignHeight);
            Theme.SetScale(Math.Min(1f, MathF.Floor(scale * 100f) / 100f));
            ClientSize = new Size(Theme.S(Theme.DesignWidth), Theme.S(Theme.DesignHeight));
            Location = new Point(area.X + (area.Width - Width) / 2, area.Y + (area.Height - Height) / 2);
        }

        if (_screen != null)
            Show(_current); // rebuild at the new scale
    }

    /// <summary>Renders every screen (with the sample order) to PNGs: run with --screenshots &lt;folder&gt;.</summary>
    public static void SaveScreenshots(string folder)
    {
        Directory.CreateDirectory(folder);
        Theme.SetScale(0.5f);
        var kiosk = new KioskContext();
        kiosk.State.SeedSample();
        kiosk.State.CurrentDish = Models.Catalog.Dishes[0];

        void Shot(ScreenId id, Func<ScreenBase> make)
        {
            using var host = new Form { FormBorderStyle = FormBorderStyle.None, ShowInTaskbar = false };
            using var screen = make();
            host.ClientSize = screen.Size;
            host.Controls.Add(screen);
            host.CreateControl();
            using var bmp = new Bitmap(screen.Width, screen.Height);
            screen.DrawToBitmap(bmp, new Rectangle(Point.Empty, screen.Size));
            bmp.Save(Path.Combine(folder, $"{(int)id + 1}-{id}.png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        Shot(ScreenId.Welcome, () => new WelcomeScreen(kiosk));
        Shot(ScreenId.Menu, () => new MenuScreen(kiosk));
        Shot(ScreenId.Item, () => new ItemScreen(kiosk));
        Shot(ScreenId.Cart, () => new CartScreen(kiosk));
        Shot(ScreenId.Payment, () => new PaymentScreen(kiosk));
        kiosk.State.PlaceOrder("card");
        Shot(ScreenId.Done, () => new DoneScreen(kiosk));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.F11)
            ApplyWindowMode(!_fullScreen);
        else if (e.KeyCode == Keys.Escape && _fullScreen)
            ApplyWindowMode(false);
    }
}
