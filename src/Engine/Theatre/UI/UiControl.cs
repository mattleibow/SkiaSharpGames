using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>Base for interactive UI controls with rectangular bounds.</summary>
public abstract class UiControl : UiActor
{
    protected UiControl(float width, float height, UiTheme theme) : base(theme)
    {
        Width = width;
        Height = height;
        Collider = new RectCollider { Width = width, Height = height };
    }

    /// <summary>The shared theme instance (non-null for controls).</summary>
    public new UiTheme Theme => base.Theme!;

    /// <summary>Control width in game-space units.</summary>
    public float Width { get; }

    /// <summary>Control height in game-space units.</summary>
    public float Height { get; }

    /// <summary>The local-space bounding rectangle centred at the origin.</summary>
    public SKRect LocalRect => SKRect.Create(-Width / 2f, -Height / 2f, Width, Height);
}
