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
public sealed class UiLabel : UiActor
{
    /// <summary>
    /// Creates a new label entity with an optional theme.
    /// When theme is null, uses <see cref="UiLabelAppearance.Default"/>.
    /// </summary>
    public UiLabel(UiTheme? theme = null) : base(theme) { }

    /// <summary>The text to render. Empty or null is silently skipped.</summary>
    public string Text { get; set; } = "";

    /// <summary>Font size in game-space units.</summary>
    public float FontSize { get; set; } = 16f;

    /// <summary>Text colour.</summary>
    public SKColor Color { get; set; } = SKColors.White;

    /// <summary>Horizontal alignment relative to the entity's X position.</summary>
    public TextAlign Align { get; set; } = TextAlign.Left;

    /// <summary>
    /// Optional per-label appearance override. When null, uses the theme's default or
    /// <see cref="UiLabelAppearance.Default"/>.
    /// </summary>
    public UiAppearance<UiLabel>? Appearance { get; set; }

    /// <summary>Returns the rendered width of <see cref="Text"/> at the current <see cref="FontSize"/>.</summary>
    public float MeasureWidth()
    {
        if (string.IsNullOrEmpty(Text)) return 0f;
        return UiLabelAppearance.GetFont(FontSize).MeasureText(Text);
    }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme?.Label ?? UiLabelAppearance.Default).Draw(canvas, this);
    }
}
