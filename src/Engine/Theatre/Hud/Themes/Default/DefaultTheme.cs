namespace SkiaSharp.Theatre;

/// <summary>
/// Factory for the default UI theme. All appearances use their built-in defaults.
/// </summary>
public static class DefaultTheme
{
    /// <summary>
    /// Creates a new <see cref="HudTheme"/> with default appearances for all controls.
    /// </summary>
    public static HudTheme Create() => new()
    {
        Button = DefaultButtonAppearance.Default,
        Checkbox = DefaultCheckboxAppearance.Default,
        Switch = DefaultSwitchAppearance.Default,
        Slider = DefaultSliderAppearance.Default,
        Joystick = DefaultJoystickAppearance.Default,
        Pointer = DefaultPointerAppearance.Default,
        Label = DefaultLabelAppearance.Default,
    };
}
