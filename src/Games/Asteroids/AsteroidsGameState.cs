namespace SkiaSharpGames.Asteroids;

internal sealed class AsteroidsGameState
{
    public int Score { get; set; }
    public int Lives { get; set; } = AsteroidsConstants.InitialLives;
    public int Level { get; set; } = 1;
    public int NextExtraLife { get; set; } = AsteroidsConstants.ExtraLifeScore;
}