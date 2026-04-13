using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

internal sealed class FallingPowerUp
{
    public float X, Y;   // centre
    public PowerUpType Type;
    public readonly RectSprite Sprite = new()
    {
        Width = BreakoutGameEngine.PowerUpW, Height = BreakoutGameEngine.PowerUpH, CornerRadius = 5f, ShowShine = false
    };
}
