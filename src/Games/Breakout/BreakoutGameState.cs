namespace SkiaSharpGames.Breakout;

/// <summary>Shared mutable state passed between the Breakout play and end scenes.</summary>
internal sealed class BreakoutGameState
{
    public int Score { get; set; }
    public int Lives { get; set; } = 3;
}
