namespace SkiaSharp.Theatre;

/// <summary>
/// Factory for the neon/cyberpunk UI theme.
/// </summary>
public static class NeonTheme
{
    /// <summary>
    /// Creates a new <see cref="HudTheme"/> with neon/cyberpunk appearances for all controls.
    /// </summary>
    public static HudTheme Create() => new()
    {
        Button = NeonButtonAppearance.Default,
        Checkbox = NeonCheckboxAppearance.Default,
        Switch = NeonSwitchAppearance.Default,
        Slider = NeonSliderAppearance.Default,
        Joystick = NeonJoystickAppearance.Default,
        Label = NeonLabelAppearance.Default,
        Pointer = NeonPointerAppearance.Default,
    };
}
