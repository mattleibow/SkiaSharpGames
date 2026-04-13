namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>Shared mutable state passed between the Breakout play and end screens.</summary>
internal sealed class BreakoutGameState
{
    public int Score { get; set; }
    public int Lives { get; set; } = 3;
}
