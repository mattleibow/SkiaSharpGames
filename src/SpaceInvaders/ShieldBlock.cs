using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class ShieldBlock : Entity
{
    public int HitPoints { get; private set; } = 3;

    public ShieldBlock()
    {
        Collider = new RectCollider { Width = ShieldBlockSize, Height = ShieldBlockSize };
        Sprite = new ShieldBlockSprite();
    }

    public new ShieldBlockSprite Sprite { get => (ShieldBlockSprite)base.Sprite!; init => base.Sprite = value; }

    public void Hit()
    {
        HitPoints--;
        Sprite.HitPoints = HitPoints;
        if (HitPoints <= 0)
            Active = false;
    }
}
