using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class Ship : Entity
{
    public ThrustSprite ThrustSprite { get; }

    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public bool Thrusting { get; set; }
    public float InvincibleTimer { get; set; }
    public bool IsInvincible => InvincibleTimer > 0f;

    public Ship()
    {
        Collider = new CircleCollider { Radius = ShipRadius * 0.65f };
        Sprite = new ShipSprite();
        ThrustSprite = new ThrustSprite { Visible = false };
    }

    protected override void OnUpdate(float deltaTime)
    {
        // Apply drag
        VelocityX *= ShipDrag;
        VelocityY *= ShipDrag;

        // Clamp speed
        float speed = MathF.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);
        if (speed > ShipMaxSpeed)
        {
            float scale = ShipMaxSpeed / speed;
            VelocityX *= scale;
            VelocityY *= scale;
        }

        // Move
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;

        // Screen wrap
        WrapPosition();

        // Update invincibility
        if (InvincibleTimer > 0f)
        {
            InvincibleTimer -= deltaTime;
            // Blink effect
            float blink = MathF.Sin(InvincibleTimer * 15f);
            if (Sprite is not null) Sprite.Alpha = blink > 0f ? 1f : 0.2f;
        }
        else
        {
            if (Sprite is not null) Sprite.Alpha = 1f;
        }

        ThrustSprite.Visible = Thrusting;
        ThrustSprite.Update(deltaTime);
    }

    private void WrapPosition()
    {
        if (X < -ShipRadius) X = GameWidth + ShipRadius;
        else if (X > GameWidth + ShipRadius) X = -ShipRadius;
        if (Y < -ShipRadius) Y = GameHeight + ShipRadius;
        else if (Y > GameHeight + ShipRadius) Y = -ShipRadius;
    }

    public void DrawThrust(SKCanvas canvas)
    {
        if (!Active || !Visible || !ThrustSprite.Visible) return;
        canvas.Save();
        canvas.Concat(LocalMatrix);
        ThrustSprite.Draw(canvas);
        canvas.Restore();
    }
}
