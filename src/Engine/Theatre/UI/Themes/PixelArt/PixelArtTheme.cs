namespace SkiaSharp.Theatre;

/// <summary>
/// Factory for the pixel-art UI theme. Retro CRT-style visuals with no anti-aliasing,
/// sharp 90° edges, and an earthy green/brown/yellow colour palette.
/// </summary>
public static class PixelArtTheme
{
    /// <summary>Creates a new <see cref="HudTheme"/> with pixel-art appearances for all controls.</summary>
    public static HudTheme Create() => new()
    {
        Button = PixelArtButtonAppearance.Default,
        Checkbox = PixelArtCheckboxAppearance.Default,
        Switch = PixelArtSwitchAppearance.Default,
        Slider = PixelArtSliderAppearance.Default,
        Joystick = PixelArtJoystickAppearance.Default,
        Pointer = PixelArtPointerAppearance.Default,
        Label = PixelArtLabelAppearance.Default,
    };
}
