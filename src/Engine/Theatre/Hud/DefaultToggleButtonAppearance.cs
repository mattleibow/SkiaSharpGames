using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Toggle button appearance for <see cref="HudButton"/> — renders as a
/// button that shows ON/OFF text based on <see cref="HudButton.IsOn"/>.
/// </summary>
public record DefaultToggleButtonAppearance : HudAppearance<HudButton>
{
    public SKColor OffColor { get; init; } = new(0x44, 0x4F, 0x5E);
    public SKColor OnColor { get; init; } = new(0x46, 0xA4, 0xF6);
    public SKColor TextColor { get; init; } = SKColors.White;
    public SKColor BorderColor { get; init; } = new(0x15, 0x1D, 0x27);
    public float CornerRadius { get; init; } = 14f;
    public float BorderWidth { get; init; } = 2f;

    private DefaultButtonAppearance? _cachedOnAppearance;
    private DefaultButtonAppearance? _cachedOffAppearance;

    public static DefaultToggleButtonAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudButton button)
    {
        var rect = button.LocalRect;
        var btnAppearance = button.IsOn
            ? (_cachedOnAppearance ??= MakeAppearance(OnColor))
            : (_cachedOffAppearance ??= MakeAppearance(OffColor));
        btnAppearance.DrawDirect(
            canvas,
            rect,
            button.IsOn ? "ON" : "OFF",
            pressed: false,
            enabled: true,
            fontSize: MathF.Min(18f, rect.Height * 0.5f)
        );
    }

    private DefaultButtonAppearance MakeAppearance(SKColor fill) =>
        new()
        {
            FillColor = fill,
            PressedFillColor = fill,
            TextColor = TextColor,
            BorderColor = BorderColor,
            CornerRadius = CornerRadius,
            BorderWidth = BorderWidth,
            BevelSize = 0f,
        };
}
