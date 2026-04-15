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

    /// <summary>Current animated width of the paddle.</summary>
    public float Width => _width.Value;

    /// <summary>True while a width animation is in progress.</summary>
    public bool IsWidthAnimating => _width.IsAnimating;

    /// <summary>Advances the width animation and keeps the collider and sprite in sync.</summary>
    public void Update(float deltaTime)
    {
        _width.Update(deltaTime);
        Collider.Width = _width.Value;
        Sprite.Width = _width.Value;
    }

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

    /// <summary>
    /// Collider.Width is kept in sync by <see cref="Update"/> and <see cref="SetWidthImmediate"/>.
    /// Never patch it externally.
    /// </summary>
    public readonly RectCollider Collider = new() { Height = PaddleHeight };

    public readonly PaddleSprite Sprite = new();
}
