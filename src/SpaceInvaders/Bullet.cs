using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class Bullet : Entity
{
    public bool FromEnemy { get; }

    public Bullet(bool fromEnemy, float speedY)
    {
        FromEnemy = fromEnemy;
        Collider = new RectCollider { Width = BulletWidth, Height = BulletHeight };
        Rigidbody = new Rigidbody2D { VelocityY = speedY };
        Sprite = new BulletSprite { FromEnemy = fromEnemy };
    }
}
