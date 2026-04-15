namespace SkiaSharpGames.Catch;

/// <summary>Shared mutable state passed between the Catch play and end screens.</summary>
internal sealed class CatchGameState
{
    public int Score { get; set; }

    public int Lives { get; set; }
}
