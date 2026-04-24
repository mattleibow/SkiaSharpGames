namespace SkiaSharp.Theatre;

/// <summary>Horizontal alignment for text rendering.</summary>
public enum TextAlign
{
    /// <summary>Text is drawn starting at the given X position.</summary>
    Left,

    /// <summary>Text is horizontally centred on the given X position.</summary>
    Center,

    /// <summary>Text ends at the given X position.</summary>
    Right,
}

/// <summary>
/// A text label actor that draws a single line of text. Handles font caching,
/// alignment, colour, and alpha internally.
/// <para>
/// Position the label using <see cref="Actor.X"/> and <see cref="Actor.Y"/>.
/// The text is drawn at the local origin according to <see cref="Align"/>:
/// <list type="bullet">
///   <item><see cref="TextAlign.Left"/> — text starts at X</item>
///   <item><see cref="TextAlign.Center"/> — text is centred on X</item>
///   <item><see cref="TextAlign.Right"/> — text ends at X</item>
/// </list>
/// Y is always the text baseline.
/// </para>
/// </summary>
public sealed class HudLabel : HudActor
{
    /// <summary>The text to render. Empty or null is silently skipped.</summary>
    public string Text { get; set; } = "";

    /// <summary>Font size in game-space units.</summary>
    public float FontSize { get; set; } = 16f;

    /// <summary>Text colour.</summary>
    public SKColor Color { get; set; } = SKColors.White;

    /// <summary>Horizontal alignment relative to the actor's X position.</summary>
    public TextAlign Align { get; set; } = TextAlign.Left;

    /// <summary>
    /// Optional per-label appearance override. When null, uses the theme's label appearance
    /// or a built-in fallback.
    /// </summary>
    public HudAppearance<HudLabel>? Appearance { get; set; }

    /// <summary>
    /// Returns the rendered width of <see cref="Text"/> at the current <see cref="FontSize"/>.
    /// </summary>
    public float MeasureWidth()
    {
        if (string.IsNullOrEmpty(Text))
            return 0f;
        var fonts = ResolvedHudTheme?.Fonts ?? FontProvider.Default;
        return fonts.GetFont(FontSize).MeasureText(Text);
    }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        var appearance = Appearance ?? ResolvedHudTheme?.Label;
        if (appearance is not null)
        {
            appearance.Draw(canvas, this);
            return;
        }

        // Minimal fallback when no theme/appearance is configured
        if (Alpha <= 0f || string.IsNullOrEmpty(Text))
            return;

        var font = (ResolvedHudTheme?.Fonts ?? FontProvider.Default).GetFont(FontSize);
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Color = Color.WithAlpha((byte)(255 * Math.Clamp(Alpha, 0f, 1f))),
        };

        float drawX = Align switch
        {
            TextAlign.Center => -font.MeasureText(Text) / 2f,
            TextAlign.Right => -font.MeasureText(Text),
            _ => 0f,
        };

        canvas.DrawText(Text, drawX, 0f, font, paint);
    }
}
