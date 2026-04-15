using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class PlayerCannon : Entity
{
    private static readonly SKPaint _paint = new() { Color = PlayerColor, IsAntialias = true };

    public readonly RectCollider Collider = new() { Width = PlayerWidth, Height = PlayerHeight };

    public void Draw(SKCanvas canvas)
    {
        canvas.DrawRect(X - PlayerWidth / 2f, Y - PlayerHeight / 2f, PlayerWidth, PlayerHeight, _paint);
        canvas.DrawRect(X - 9f, Y - PlayerHeight / 2f - 12f, 18f, 12f, _paint);
        canvas.DrawRect(X - 2f, Y - PlayerHeight / 2f - 24f, 4f, 12f, _paint);
    }
}
