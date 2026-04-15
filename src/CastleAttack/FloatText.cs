using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

internal sealed class FloatText : Entity
{
    public float Life;
    public string Text = "";
    public SKColor Color = SKColors.White;

    public FloatText()
    {
        Rigidbody = new Rigidbody2D { VelocityY = -28f };
        Sprite = new FloatTextSprite();
    }

    public new FloatTextSprite Sprite { get => (FloatTextSprite)base.Sprite!; init => base.Sprite = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    protected override void OnUpdate(float deltaTime)
    {
        Life -= deltaTime;
        Sprite.Text = Text;
        Sprite.Color = Color;
        Sprite.Life = Life;
        if (Life <= 0f) Active = false;
    }
}
