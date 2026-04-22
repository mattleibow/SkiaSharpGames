using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

internal sealed class FloatText : Entity
{
    public const float DefaultLife = 1.4f;

    public float Life;
    public string Text = "";
    public SKColor Color = SKColors.White;
    private readonly UiLabel _label;

    public FloatText()
    {
        Rigidbody = new Rigidbody2D { VelocityY = -28f };
        _label = new UiLabel
        {
            Size = 16f,
            Align = TextAlign.Center
        };
        AddChild(_label);
    }

    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    protected override void OnUpdate(float deltaTime)
    {
        Life -= deltaTime;
        _label.Text = Text;
        _label.Color = Color;
        _label.Alpha = Math.Clamp(Life / DefaultLife, 0f, 1f);
        if (Life <= 0f) Active = false;
    }
}
