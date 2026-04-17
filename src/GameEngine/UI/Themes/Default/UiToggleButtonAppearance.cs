using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Toggle button appearance for <see cref="UiButton"/> — renders as a
/// button that shows ON/OFF text based on <see cref="UiButton.IsOn"/>.
/// </summary>
public record UiToggleButtonAppearance : UiAppearance<UiButton>
{
    public SKColor OffColor { get; init; } = new(0x44, 0x4F, 0x5E);
    public SKColor OnColor { get; init; } = new(0x46, 0xA4, 0xF6);
    public SKColor TextColor { get; init; } = SKColors.White;
    public SKColor BorderColor { get; init; } = new(0x15, 0x1D, 0x27);
    public float CornerRadius { get; init; } = 14f;
    public float BorderWidth { get; init; } = 2f;

    public static UiToggleButtonAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiButton button)
    {
        var rect = button.LocalRect;
        var btnAppearance = new UiButtonAppearance
        {
            FillColor = button.IsOn ? OnColor : OffColor,
            PressedFillColor = button.IsOn ? OnColor : OffColor,
            TextColor = TextColor,
            BorderColor = BorderColor,
            CornerRadius = CornerRadius,
            BorderWidth = BorderWidth,
            BevelSize = 0f,
        };
        btnAppearance.DrawDirect(canvas, rect, button.IsOn ? "ON" : "OFF",
            pressed: false, enabled: true,
            fontSize: MathF.Min(18f, rect.Height * 0.5f));
    }
}
