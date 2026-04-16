namespace SkiaSharpGames.GameEngine.UI;

public interface IUiThemeProvider
{
    UiTheme Theme { get; set; }
}

public sealed class UiThemeProvider(UiTheme theme) : IUiThemeProvider
{
    public UiTheme Theme { get; set; } = theme;
}
