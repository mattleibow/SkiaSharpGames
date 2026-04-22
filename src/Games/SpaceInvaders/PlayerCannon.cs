using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class PlayerCannon : Entity
{
    private static readonly SKPaint _paint = new() { Color = PlayerColor, IsAntialias = true };

    public PlayerCannon()
    {
        Collider = new RectCollider { Width = PlayerWidth, Height = PlayerHeight };
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(-PlayerWidth / 2f, -PlayerHeight / 2f, PlayerWidth, PlayerHeight, _paint);
        canvas.DrawRect(-9f, -PlayerHeight / 2f - 12f, 18f, 12f, _paint);
        canvas.DrawRect(-2f, -PlayerHeight / 2f - 24f, 4f, 12f, _paint);
    }
}
