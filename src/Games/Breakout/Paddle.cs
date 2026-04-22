using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// The player's paddle entity. Position (X, Y) is the centre of the rectangle.
/// Width is animated; all width-related state (animation, collider sync) lives here.
/// </summary>
internal sealed class Paddle : Actor
{
    private readonly AnimatedFloat _width = new(DefaultPaddleWidth);
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public SKColor Color { get; set; } = PaddleColor;
    public float Height { get; set; } = PaddleHeight;
    public float CornerRadius { get; set; } = 6f;

    public Paddle()
    {
        Collider = new RectCollider { Height = PaddleHeight };
    }

    /// <summary>Current animated width of the paddle.</summary>
    public float Width => _width.Value;

    /// <summary>True while a width animation is in progress.</summary>
    public bool IsWidthAnimating => _width.IsAnimating;

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    /// <summary>Animates the paddle to a new width.</summary>
    public void AnimateWidth(float target, float duration, Func<float, float> easing) =>
        _width.AnimateTo(target, duration, easing);

    /// <summary>Sets the width instantly, syncing the collider immediately.</summary>
    public void SetWidthImmediate(float value)
    {
        _width.SetImmediate(value);
        Collider.Width = value;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _width.Update(deltaTime);
        Collider.Width = _width.Value;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawRoundRect(
            new SKRoundRect(SKRect.Create(0 - Width / 2f, 0 - Height / 2f, Width, Height), CornerRadius),
            _paint);
    }
}
