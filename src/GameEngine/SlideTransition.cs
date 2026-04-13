using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>Direction from which the slide panel enters the screen.</summary>
public enum SlideDirection { Up, Down, Left, Right }

/// <summary>
/// A wipe transition: a solid-colour panel slides in from one edge to cover the screen,
/// then slides out to reveal the new state.
/// </summary>
public sealed class SlideTransition : IScreenTransition
{
    /// <summary>The edge from which the panel enters. Defaults to <see cref="SlideDirection.Up"/>.</summary>
    public SlideDirection Direction { get; init; } = SlideDirection.Up;

    /// <summary>The colour of the sliding panel.</summary>
    public SKColor Color { get; init; } = new SKColor(0x0D, 0x1B, 0x2A);

    /// <inheritdoc />
    public void Draw(SKCanvas canvas, float coverage, (int width, int height) dimensions)
    {
        if (coverage <= 0f) return;

        float w = dimensions.width, h = dimensions.height;

        // The panel always fills the full game area; we translate it
        // so that at coverage=0 it is fully off-screen and at coverage=1 it is fully on-screen.
        float x = 0f, y = 0f;
        switch (Direction)
        {
            case SlideDirection.Up:    y = -(1f - coverage) * h; break;
            case SlideDirection.Down:  y =  (1f - coverage) * h; break;
            case SlideDirection.Left:  x = -(1f - coverage) * w; break;
            case SlideDirection.Right: x =  (1f - coverage) * w; break;
        }

        using var paint = new SKPaint { Color = Color };
        canvas.DrawRect(SKRect.Create(x, y, w, h), paint);
    }
}
