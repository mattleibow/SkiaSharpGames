namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Factory for the neon/cyberpunk UI theme.
/// </summary>
public static class NeonTheme
{
    /// <summary>
    /// Creates a new <see cref="UiTheme"/> with neon/cyberpunk appearances for all controls.
    /// </summary>
    public static UiTheme Create() => new()
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
