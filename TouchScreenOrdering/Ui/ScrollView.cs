namespace TouchScreenOrdering.Ui;

/// <summary>
/// A vertical, touch-friendly scroll area without scrollbars: drag (finger or mouse)
/// or use the mouse wheel. A drag never counts as a tap on the item under the finger.
/// </summary>
internal sealed class ScrollView : Control
{
    private int _offset;
    private int _downY;
    private int _downOffset;
    private bool _down;

    public ScrollView(Color background)
    {
        SetStyle(ControlStyles.Selectable
                 | ControlStyles.UserPaint
                 | ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = background;
        Content = new Tile { Fill = background, BackColor = background };
        Controls.Add(Content);
        Hook(Content);
    }

    /// <summary>Put scrolling content here (design px coordinates, origin at the top-left of the view).</summary>
    public Tile Content { get; }

    public bool Dragging { get; private set; }

    public static bool IsDragging(Control control)
    {
        for (var p = control.Parent; p != null; p = p.Parent)
            if (p is ScrollView view)
                return view.Dragging;
        return false;
    }

    /// <summary>Removes all content (controls and painted items).</summary>
    public void Clear()
    {
        foreach (var child in Content.Controls.Cast<Control>().ToList())
            child.Dispose();
        Content.Draws.Clear();
        Content.Invalidate();
    }

    public void SetContentHeight(float designHeight)
    {
        Content.Bounds = new Rectangle(0, -_offset, Width, Math.Max(Height, Theme.S(designHeight)));
        ScrollTo(_offset, force: true);
    }

    public void ScrollTo(int offset, bool force = false)
    {
        int max = Math.Max(0, Content.Height - Height);
        offset = Math.Clamp(offset, 0, max);
        if (offset == _offset && !force)
            return;
        _offset = offset;
        Content.Top = -offset;
        Content.Update();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        Content.Width = Width;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        ScrollTo(_offset - e.Delta * Theme.S(160) / 120);
    }

    private void Hook(Control control)
    {
        control.MouseDown += OnChildMouseDown;
        control.MouseMove += OnChildMouseMove;
        control.MouseUp += (_, _) => _down = false;
        control.MouseEnter += (_, _) => { if (CanFocus && !Focused) Focus(); };
        control.ControlAdded += (_, e) => { if (e.Control != null) Hook(e.Control); };
        foreach (Control child in control.Controls)
            Hook(child);
    }

    private void OnChildMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;
        _down = true;
        Dragging = false;
        _downY = Cursor.Position.Y;
        _downOffset = _offset;
    }

    private void OnChildMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_down)
            return;
        int dy = Cursor.Position.Y - _downY;
        if (!Dragging && Math.Abs(dy) > Theme.S(14))
            Dragging = true;
        if (Dragging)
            ScrollTo(_downOffset - dy);
    }
}
