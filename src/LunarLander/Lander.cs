using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>
/// The player's lander entity. Demonstrates rotation and child entities:
/// flame sprites are children that rotate with the lander automatically.
/// </summary>
internal sealed class Lander : Entity
{
    public Lander()
    {
        Collider = new RectCollider { Width = LanderWidth, Height = LanderHeight };
        Rigidbody = new Rigidbody2D();
        Sprite = new LanderSprite();

        // Child entities for thruster flames — they orbit with lander rotation!
        MainFlame = new Entity
        {
            Y = LanderHeight / 2f + 2f,
            Sprite = new FlameSprite(),
            Visible = false
        };
        LeftFlame = new Entity
        {
            X = -LanderWidth / 2f,
            Y = -4f,
            Rotation = 0.5f,
            Sprite = new FlameSprite { Intensity = 0.5f },
            Visible = false
        };
        RightFlame = new Entity
        {
            X = LanderWidth / 2f,
            Y = -4f,
            Rotation = -0.5f,
            Sprite = new FlameSprite { Intensity = 0.5f },
            Visible = false
        };

        AddChild(MainFlame);
        AddChild(LeftFlame);
        AddChild(RightFlame);
    }

    public Entity MainFlame { get; }
    public Entity LeftFlame { get; }
    public Entity RightFlame { get; }

    public new LanderSprite Sprite { get => (LanderSprite)base.Sprite!; init => base.Sprite = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
