using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>A burning oil puddle that persists on the ground and damages enemies walking through it.</summary>
internal sealed class OilPuddle : Entity
{
    public float Life = OilPuddleDuration;

    public OilPuddle(float x)
    {
        X = x;
        Y = GroundY - OilPuddleHeight / 2f;
        Collider = new RectCollider { Width = OilPuddleWidth, Height = OilPuddleHeight };
        Sprite = new OilPuddleSprite();
    }

    public new OilPuddleSprite Sprite { get => (OilPuddleSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    protected override void OnUpdate(float deltaTime)
    {
        Life -= deltaTime;
        Sprite.Life = Life;
        if (Life <= 0f) Active = false;
    }
}
