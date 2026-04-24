namespace SkiaSharpGames.Snake;

/// <summary>Shared mutable state passed between the Snake play and end scenes.</summary>
internal sealed class SnakeGameState
{
    public int Score { get; set; }

    public int HighScore { get; set; }
}