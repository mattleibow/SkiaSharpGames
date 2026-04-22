namespace SkiaSharpGames.GameEngine.UI;

/// <summary>Base entity for all UI elements. Holds theme and appearance.</summary>
public abstract class UiEntity : Entity
{
    protected UiEntity(UiTheme? theme = null) => Theme = theme;

    /// <summary>The shared theme instance.</summary>
    public UiTheme? Theme { get; }
}
