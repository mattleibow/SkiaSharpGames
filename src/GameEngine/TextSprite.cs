using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A sprite that draws a single line of text. Handles font caching,
/// alignment, colour, and alpha internally.
/// <para>
/// Set <see cref="Text"/> each frame (or once for static labels) and call
/// <see cref="Draw"/>. The position passed to <c>Draw</c> is interpreted
/// according to <see cref="Align"/>:
/// <list type="bullet">
///   <item><see cref="TextAlign.Left"/> — text starts at X</item>
///   <item><see cref="TextAlign.Center"/> — text is centred on X</item>
///   <item><see cref="TextAlign.Right"/> — text ends at X</item>
/// </list>
/// Y is always the text baseline.
/// </para>
/// </summary>
public sealed class TextSprite : Sprite
{
    // Shared font cache — safe because rendering is single-threaded.
    private static readonly Dictionary<float, SKFont> FontCache = [];
    private readonly SKPaint _paint = new() { IsAntialias = true };

    /// <summary>The text to render. Empty or null is silently skipped.</summary>
    public string Text { get; set; } = "";

    /// <summary>Font size in game-space units.</summary>
    public float Size { get; set; } = 16f;

    /// <summary>Text colour.</summary>
    public SKColor Color { get; set; } = SKColors.White;

    /// <summary>Horizontal alignment relative to the X passed to <see cref="Draw"/>.</summary>
    public TextAlign Align { get; set; } = TextAlign.Left;

    /// <summary>Returns the rendered width of <see cref="Text"/> at the current <see cref="Size"/>.</summary>
    public float MeasureWidth()
    {
        if (string.IsNullOrEmpty(Text)) return 0f;
        return GetFont(Size).MeasureText(Text);
    }

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible || Alpha <= 0f || string.IsNullOrEmpty(Text))
            return;

        var font = GetFont(Size);
        _paint.Color = Color.WithAlpha((byte)(255 * Math.Clamp(Alpha, 0f, 1f)));

        float drawX = Align switch
        {
            TextAlign.Center => x - font.MeasureText(Text) / 2f,
            TextAlign.Right => x - font.MeasureText(Text),
            _ => x
        };

        canvas.DrawText(Text, drawX, y, font, _paint);
    }

    private static SKFont GetFont(float size)
    {
        float key = MathF.Round(size, 2);
        if (!FontCache.TryGetValue(key, out var font))
        {
            font = new SKFont(SKTypeface.Default, key) { Edging = SKFontEdging.Antialias };
            FontCache[key] = font;
        }
        return font;
    }
}
