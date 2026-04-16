using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>Horizontal alignment for text rendering.</summary>
public enum TextAlign
{
    /// <summary>Text is drawn starting at the given X position.</summary>
    Left,

    /// <summary>Text is horizontally centred on the given X position.</summary>
    Center,

    /// <summary>Text ends at the given X position.</summary>
    Right
}

/// <summary>
/// A text label entity that draws a single line of text. Handles font caching,
/// alignment, colour, and alpha internally.
/// <para>
/// Position the label using <see cref="Entity.X"/> and <see cref="Entity.Y"/>.
/// The text is drawn at the local origin according to <see cref="Align"/>:
/// <list type="bullet">
///   <item><see cref="TextAlign.Left"/> — text starts at X</item>
///   <item><see cref="TextAlign.Center"/> — text is centred on X</item>
///   <item><see cref="TextAlign.Right"/> — text ends at X</item>
/// </list>
/// Y is always the text baseline.
/// </para>
/// </summary>
public sealed class UiLabel : Entity
{
    // Shared font cache — safe because rendering is single-threaded.
    private static readonly Dictionary<float, SKFont> FontCache = [];
    private readonly SKPaint _paint = new() { IsAntialias = true };

    /// <summary>The text to render. Empty or null is silently skipped.</summary>
    public string Text { get; set; } = "";

    /// <summary>Font size in game-space units.</summary>
    public float FontSize { get; set; } = 16f;

    /// <summary>Text colour.</summary>
    public SKColor Color { get; set; } = SKColors.White;

    /// <summary>Horizontal alignment relative to the entity's X position.</summary>
    public TextAlign Align { get; set; } = TextAlign.Left;

    /// <summary>Returns the rendered width of <see cref="Text"/> at the current <see cref="FontSize"/>.</summary>
    public float MeasureWidth()
    {
        if (string.IsNullOrEmpty(Text)) return 0f;
        return GetFont(FontSize).MeasureText(Text);
    }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f || string.IsNullOrEmpty(Text))
            return;

        var font = GetFont(FontSize);
        _paint.Color = Color.WithAlpha((byte)(255 * Math.Clamp(Alpha, 0f, 1f)));

        float drawX = Align switch
        {
            TextAlign.Center => -font.MeasureText(Text) / 2f,
            TextAlign.Right => -font.MeasureText(Text),
            _ => 0f
        };

        canvas.DrawText(Text, drawX, 0f, font, _paint);
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
