using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.CastleAttack;

internal sealed class FloatText : Actor
{
    public float Life;
    public string Text = "";
    public SKColor Color = SKColors.White;

    private readonly HudLabel _label = new() { FontSize = 16f, Align = TextAlign.Center };

    public FloatText()
    {
        Rigidbody = new Rigidbody2D { VelocityY = -28f };
    }

    public new Rigidbody2D Rigidbody
    {
        get => (Rigidbody2D)base.Rigidbody!;
        init => base.Rigidbody = value;
    }

    protected override void OnUpdate(float deltaTime)
    {
        Life -= deltaTime;
        if (Life <= 0f)
            Active = false;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (string.IsNullOrEmpty(Text))
            return;
        _label.Text = Text;
        _label.Color = Color;
        _label.Alpha = Math.Clamp(Life / 1.4f, 0f, 1f);
        _label.Draw(canvas);
    }
}
