using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class PongBall : Entity
{
    public PongBall()
    {
        Collider = new CircleCollider { Radius = BallRadius };
        Rigidbody = new Rigidbody2D();
        Sprite = new BallSprite
        {
            Radius = BallRadius,
            Color = SKColors.White,
            GlowRadius = 2f,
            GlowColor = SKColors.White,
        };
    }

    public new BallSprite Sprite { get => (BallSprite)base.Sprite!; init => base.Sprite = value; }
    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
