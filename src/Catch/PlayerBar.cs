using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Catch;

internal sealed class PlayerBar : Entity
{
    public PlayerBar()
    {
        Collider = new RectCollider { Width = CatchConstants.BarWidth, Height = CatchConstants.BarHeight };
        Sprite = new BarSprite();
    }

    public new BarSprite Sprite { get => (BarSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
}
