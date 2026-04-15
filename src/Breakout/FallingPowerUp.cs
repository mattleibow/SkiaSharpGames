using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

internal sealed class FallingPowerUp : Entity
{
    public PowerUpType Type;

    public FallingPowerUp()
    {
        Rigidbody = new Rigidbody2D { VelocityY = PowerUpSpeed };
        Collider = new RectCollider { Width = PowerUpW, Height = PowerUpH };
        Sprite = new PowerUpSprite { Width = PowerUpW, Height = PowerUpH };
    }

    public new PowerUpSprite Sprite { get => (PowerUpSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
