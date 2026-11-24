using System.ComponentModel;

namespace TouchScreenOrdering.Ui;

/// <summary>
/// The basic building block: a rounded, owner-drawn surface that can hold child controls,
/// paint its own text/icons (<see cref="Draws"/>) and act as a touch button.
/// Radius and border width are in design px.
/// </summary>
internal class Tile : Control
{
    private bool _pressed;
    private bool _interactive;

    public Tile()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor
                 | ControlStyles.UserPaint
                 | ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color Fill { get; set; } = Color.Transparent;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Radius { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.Transparent;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float BorderWidth { get; set; }

    /// <summary>Painters run in order, after the fill and before the border.</summary>
    public List<Action<Graphics>> Draws { get; } = new();

    /// <summary>When true the tile shows press feedback and raises Click.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Interactive
    {
        get => _interactive;
        set
        {
            _interactive = value;
            Cursor = value ? Cursors.Hand : Cursors.Default;
            if (!value) _pressed = false;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Draw.Prepare(g);
        var r = new RectangleF(0, 0, Width, Height);
        float radius = Theme.F(Radius);

        if (Fill.A > 0)
            Draw.FillRound(g, r, radius, Fill);

        foreach (var paint in Draws)
            paint(g);

        if (BorderColor.A > 0 && BorderWidth > 0)
            Draw.StrokeRound(g, r, radius, BorderColor, Theme.F(BorderWidth));

        if (_pressed)
            Draw.FillRound(g, r, radius, Color.FromArgb(32, 0, 0, 0));
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (_interactive && e.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_pressed && (ScrollView.IsDragging(this) || !ClientRectangle.Contains(e.Location)))
        {
            _pressed = false;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_pressed)
        {
            _pressed = false;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_pressed)
        {
            _pressed = false;
            Invalidate();
        }
    }

    protected override void OnClick(EventArgs e)
    {
        if (!_interactive || ScrollView.IsDragging(this))
            return;
        base.OnClick(e);
    }
}
