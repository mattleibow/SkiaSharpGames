using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// The player's paddle entity. Position (X, Y) is the centre of the rectangle.
/// Width is animated; all width-related state (animation, collider sync, sprite sync) lives here.
/// </summary>
internal sealed class Paddle : Entity
{
    private readonly AnimatedFloat _width = new(DefaultPaddleWidth);

    public Paddle()
    {
        Collider = new RectCollider { Height = PaddleHeight };
        Sprite = new PaddleSprite();
    }

    /// <summary>Current animated width of the paddle.</summary>
    public float Width => _width.Value;

    /// <summary>True while a width animation is in progress.</summary>
    public bool IsWidthAnimating => _width.IsAnimating;

    public new PaddleSprite Sprite { get => (PaddleSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    /// <summary>Animates the paddle to a new width.</summary>
    public void AnimateWidth(float target, float duration, Func<float, float> easing) =>
        _width.AnimateTo(target, duration, easing);

    /// <summary>Sets the width instantly, syncing the collider and sprite immediately.</summary>
    public void SetWidthImmediate(float value)
    {
        _width.SetImmediate(value);
        Collider.Width = value;
        Sprite.Width = value;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _width.Update(deltaTime);
        Collider.Width = _width.Value;
        Sprite.Width = _width.Value;
    }
}
