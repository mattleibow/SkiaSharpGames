using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>Base for interactive UI controls with rectangular bounds.</summary>
public abstract class HudControl : HudActor
{
    protected HudControl(float width, float height, HudTheme theme) : base(theme)
    {
        Width = width;
        Height = height;
        Collider = new RectCollider { Width = width, Height = height };
    }

    /// <summary>The shared theme instance (non-null for controls).</summary>
    public new HudTheme Theme => base.Theme!;

    /// <summary>Control width in game-space units.</summary>
    public float Width { get; }

    /// <summary>Control height in game-space units.</summary>
    public float Height { get; }

    /// <summary>The local-space bounding rectangle centred at the origin.</summary>
    public SKRect LocalRect => SKRect.Create(-Width / 2f, -Height / 2f, Width, Height);
}
