namespace SkiaSharp.Theatre;

/// <summary>Base actor for all UI elements. Holds theme and appearance.</summary>
public abstract class HudActor : Actor
{
    protected HudActor(HudTheme? theme = null) => Theme = theme;

    /// <summary>The shared theme instance.</summary>
    public HudTheme? Theme { get; }
}
