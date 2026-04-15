using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Catch;

internal sealed class PlayerBar : Entity
{
    public readonly RectCollider Collider = new() { Width = CatchConstants.BarWidth, Height = CatchConstants.BarHeight };

    public readonly BarSprite Sprite = new();
}
