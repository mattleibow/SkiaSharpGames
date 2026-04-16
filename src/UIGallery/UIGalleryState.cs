using SkiaSharp;

namespace SkiaSharpGames.UIGallery;

internal sealed class UIGalleryState
{
    public int ThemeIndex { get; set; }
    public bool CheckboxChecked { get; set; } = true;
    public bool SwitchOn { get; set; }
    public float SliderValue { get; set; } = 0.45f;
    public bool PrimaryPressed { get; set; }
    public bool OverridePressed { get; set; }
    public SKPoint JoystickDelta { get; set; }
}
