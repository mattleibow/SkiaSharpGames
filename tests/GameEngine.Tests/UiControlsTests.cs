using SkiaSharp;
using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class HudControlsTests
{
    [Fact]
    public void DefaultButtonAppearance_DrawDirect_CoversDefaultPaths()
    {
        using var bitmap = new SKBitmap(200, 120);
        using var canvas = new SKCanvas(bitmap);
        var rect = SKRect.Create(20f, 20f, 140f, 60f);

        DefaultButtonAppearance.Default.DrawDirect(canvas, rect, "PLAY", pressed: false, enabled: true);
        (DefaultButtonAppearance.Default with { BevelSize = 0f }).DrawDirect(canvas, rect, "PLAY", pressed: true, enabled: false);
    }

    [Fact]
    public void HudCheckboxAppearance_Draw_CoversCheckedUnchecked()
    {
        using var bitmap = new SKBitmap(120, 120);
        using var canvas = new SKCanvas(bitmap);
        var tp = new HudTheme();

        var cb = new HudCheckbox(36f, 36f, tp) { X = 50f, Y = 50f };
        cb.Draw(canvas);

        cb.IsChecked = true;
        cb.Draw(canvas);
    }

    [Fact]
    public void HudSwitchAppearance_Draw_CoversSlidingAndToggle()
    {
        using var bitmap = new SKBitmap(220, 120);
        using var canvas = new SKCanvas(bitmap);
        var tp = new HudTheme();

        var sliding = new HudSwitch(120f, 44f, tp) { IsOn = true };
        sliding.Draw(canvas);

        sliding.IsOn = false;
        sliding.Draw(canvas);

        var toggle = new HudButton(120f, 44f, tp) { IsToggle = true, IsOn = true, Appearance = DefaultToggleButtonAppearance.Default };
        toggle.Draw(canvas);
    }

    [Fact]
    public void HudSliderAppearance_Draw_ClampsAndDraws()
    {
        using var bitmap = new SKBitmap(260, 100);
        using var canvas = new SKCanvas(bitmap);
        var tp = new HudTheme();

        var slider = new HudSlider(200f, 20f, tp) { Value = -2f };
        slider.Draw(canvas);

        slider.Value = 2f;
        slider.Draw(canvas);
    }

    [Fact]
    public void HudJoystickAppearance_Draw()
    {
        using var bitmap = new SKBitmap(260, 260);
        using var canvas = new SKCanvas(bitmap);
        var tp = new HudTheme();

        var js = new HudJoystick(60f, tp) { X = 120f, Y = 120f };
        js.Draw(canvas);

        js.Delta = new SKPoint(5f, -5f);
        js.Draw(canvas);
    }

    [Fact]
    public void ClampJoystick_WhenInsideRadius_ReturnsOriginalDelta()
    {
        var delta = new SKPoint(2f, 3f);
        var clamped = HudJoystick.ClampJoystick(delta, 10f);

        Assert.Equal(delta, clamped);
    }
}
