namespace SkiaSharp.Theatre;

/// <summary>Base actor for all UI elements. Holds theme and appearance.</summary>
public abstract class UiActor : Actor
{
    protected UiActor(UiTheme? theme = null) => Theme = theme;

    /// <summary>The shared theme instance.</summary>
    public UiTheme? Theme { get; }
}
