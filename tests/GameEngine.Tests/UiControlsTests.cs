using SkiaSharp;
using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class UiControlsTests
{
    [Fact]
    public void UiButtonAppearance_DrawDirect_CoversDefaultPaths()
    {
        using var bitmap = new SKBitmap(200, 120);
        using var canvas = new SKCanvas(bitmap);
        var rect = SKRect.Create(20f, 20f, 140f, 60f);

        UiButtonAppearance.Default.DrawDirect(canvas, rect, "PLAY", pressed: false, enabled: true);
        (UiButtonAppearance.Default with { BevelSize = 0f }).DrawDirect(canvas, rect, "PLAY", pressed: true, enabled: false);
    }

    [Fact]
    public void UiCheckboxAppearance_Draw_CoversCheckedUnchecked()
    {
        using var bitmap = new SKBitmap(120, 120);
        using var canvas = new SKCanvas(bitmap);
        var tp = new UiTheme();

        var cb = new UiCheckbox(36f, 36f, tp) { X = 50f, Y = 50f };
        cb.Draw(canvas);

        cb.IsChecked = true;
        cb.Draw(canvas);
    }

    [Fact]
    public void UiSwitchAppearance_Draw_CoversSlidingAndToggle()
    {
        using var bitmap = new SKBitmap(220, 120);
        using var canvas = new SKCanvas(bitmap);
        var tp = new UiTheme();

        var sliding = new UiSwitch(120f, 44f, tp) { IsOn = true };
        sliding.Draw(canvas);

        sliding.IsOn = false;
        sliding.Draw(canvas);

        var toggle = new UiButton(120f, 44f, tp) { IsToggle = true, IsOn = true, Appearance = UiToggleButtonAppearance.Default };
        toggle.Draw(canvas);
    }

    [Fact]
    public void UiSliderAppearance_Draw_ClampsAndDraws()
    {
        using var bitmap = new SKBitmap(260, 100);
        using var canvas = new SKCanvas(bitmap);
        var tp = new UiTheme();

        var slider = new UiSlider(200f, 20f, tp) { Value = -2f };
        slider.Draw(canvas);

        slider.Value = 2f;
        slider.Draw(canvas);
    }

    [Fact]
    public void UiJoystickAppearance_Draw()
    {
        using var bitmap = new SKBitmap(260, 260);
        using var canvas = new SKCanvas(bitmap);
        var tp = new UiTheme();

        var js = new UiJoystick(60f, tp) { X = 120f, Y = 120f };
        js.Draw(canvas);

        js.Delta = new SKPoint(5f, -5f);
        js.Draw(canvas);
    }

    [Fact]
    public void ClampJoystick_WhenInsideRadius_ReturnsOriginalDelta()
    {
        var delta = new SKPoint(2f, 3f);
        var clamped = UiJoystick.ClampJoystick(delta, 10f);

        Assert.Equal(delta, clamped);
    }
}
