using SkiaSharp;
using SkiaSharpGames.GameEngine.UI;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class UiControlEntityTests
{
    private static readonly UiTheme ThemeProvider = new UiTheme();

    // ── UiButton ──────────────────────────────────────────────────────

    [Fact]
    public void UiButton_HasCollider()
    {
        var button = new UiButton(100f, 40f, ThemeProvider);
        Assert.NotNull(button.Collider);
        Assert.IsType<RectCollider>(button.Collider);
    }

    [Fact]
    public void UiButton_DefaultState()
    {
        var button = new UiButton(100f, 40f, ThemeProvider) { Label = "Go" };
        Assert.Equal("Go", button.Label);
        Assert.False(button.IsPressed);
        Assert.True(button.IsEnabled);
        Assert.Equal(18f, button.FontSize);
        Assert.Null(button.Appearance);
    }

    [Fact]
    public void UiButton_Appearance_UsesThemeByDefault()
    {
        var button = new UiButton(100f, 40f, ThemeProvider);
        Assert.Null(button.Appearance);
        // OnDraw falls back to ThemeProvider.Button
    }

    [Fact]
    public void UiButton_Appearance_UsesOverrideWhenSet()
    {
        var button = new UiButton(100f, 40f, ThemeProvider);
        var custom = UiButtonAppearance.Default with { CornerRadius = 99f };
        button.Appearance = custom;
        Assert.Same(custom, button.Appearance);
    }

    [Fact]
    public void UiButton_DimensionsMatch()
    {
        var button = new UiButton(150f, 60f, ThemeProvider);
        Assert.Equal(150f, button.Width);
        Assert.Equal(60f, button.Height);

        var collider = (RectCollider)button.Collider!;
        Assert.Equal(150f, collider.Width);
        Assert.Equal(60f, collider.Height);
    }

    [Fact]
    public void UiButton_Draws()
    {
        using var bitmap = new SKBitmap(200, 100);
        using var canvas = new SKCanvas(bitmap);

        var button = new UiButton(100f, 40f, ThemeProvider) { Label = "Test", X = 100f, Y = 50f };
        button.Draw(canvas); // Should not throw
    }

    [Fact]
    public void UiButton_CustomAppearance_IsInvoked()
    {
        using var bitmap = new SKBitmap(200, 100);
        using var canvas = new SKCanvas(bitmap);

        bool called = false;
        var button = new UiButton(100f, 40f, ThemeProvider)
        {
            Appearance = new DelegateButtonAppearance(() => called = true)
        };
        button.Draw(canvas);
        Assert.True(called);
    }

    [Fact]
    public void UiButton_CollisionDetection_Works()
    {
        var group = new Entity();
        var button = new UiButton(100f, 40f, ThemeProvider) { X = 200f, Y = 100f };
        group.AddChild(button);

        var probe = new Entity { Collider = new CircleCollider { Radius = 1f } };

        probe.X = 200f; probe.Y = 100f;
        var hit = group.FindChildCollision(probe, out _);
        Assert.Same(button, hit);

        probe.X = 0f; probe.Y = 0f;
        hit = group.FindChildCollision(probe, out _);
        Assert.Null(hit);
    }

    // ── UiCheckbox ────────────────────────────────────────────────────

    [Fact]
    public void UiCheckbox_HasCollider()
    {
        var cb = new UiCheckbox(30f, 30f, ThemeProvider);
        Assert.NotNull(cb.Collider);
    }

    [Fact]
    public void UiCheckbox_DefaultState()
    {
        var cb = new UiCheckbox(30f, 30f, ThemeProvider);
        Assert.False(cb.IsChecked);
        Assert.Null(cb.Appearance);
    }

    [Fact]
    public void UiCheckbox_ToggleState()
    {
        var cb = new UiCheckbox(30f, 30f, ThemeProvider);
        cb.IsChecked = true;
        Assert.True(cb.IsChecked);
        cb.IsChecked = !cb.IsChecked;
        Assert.False(cb.IsChecked);
    }

    [Fact]
    public void UiCheckbox_Appearance_UsesOverrideWhenSet()
    {
        var cb = new UiCheckbox(30f, 30f, ThemeProvider);
        var custom = UiCheckboxAppearance.Default with { CornerRadius = 99f };
        cb.Appearance = custom;
        Assert.Same(custom, cb.Appearance);
    }

    [Fact]
    public void UiCheckbox_Draws()
    {
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        var cb = new UiCheckbox(30f, 30f, ThemeProvider) { IsChecked = true, X = 50f, Y = 50f };
        cb.Draw(canvas);
    }

    // ── UiSwitch ──────────────────────────────────────────────────────

    [Fact]
    public void UiSwitch_HasCollider()
    {
        var sw = new UiSwitch(100f, 40f, ThemeProvider);
        Assert.NotNull(sw.Collider);
    }

    [Fact]
    public void UiSwitch_DefaultState()
    {
        var sw = new UiSwitch(100f, 40f, ThemeProvider);
        Assert.False(sw.IsOn);
    }

    [Fact]
    public void UiButton_ToggleButton_ViaAppearance()
    {
        var btn = new UiButton(100f, 40f, ThemeProvider) { IsToggle = true };
        btn.Appearance = UiToggleButtonAppearance.Default;
        Assert.IsType<UiToggleButtonAppearance>(btn.Appearance);
    }

    [Fact]
    public void UiSwitch_Appearance_UsesOverrideWhenSet()
    {
        var sw = new UiSwitch(100f, 40f, ThemeProvider);
        var custom = UiSwitchAppearance.Default with { CornerRadius = 99f };
        sw.Appearance = custom;
        Assert.Same(custom, sw.Appearance);
    }

    [Fact]
    public void UiSwitch_And_ToggleButton_Draw()
    {
        using var bitmap = new SKBitmap(200, 100);
        using var canvas = new SKCanvas(bitmap);

        var sliding = new UiSwitch(100f, 40f, ThemeProvider) { IsOn = true };
        sliding.Draw(canvas);

        var toggle = new UiButton(100f, 40f, ThemeProvider) { IsToggle = true, IsOn = false, Appearance = UiToggleButtonAppearance.Default };
        toggle.Draw(canvas);
    }

    // ── UiSlider ──────────────────────────────────────────────────────

    [Fact]
    public void UiSlider_HasCollider()
    {
        var slider = new UiSlider(200f, 20f, ThemeProvider);
        Assert.NotNull(slider.Collider);
    }

    [Fact]
    public void UiSlider_DefaultValue()
    {
        var slider = new UiSlider(200f, 20f, ThemeProvider);
        Assert.Equal(0.5f, slider.Value);
    }

    [Fact]
    public void UiSlider_UpdateValueFromPointer()
    {
        var slider = new UiSlider(200f, 20f, ThemeProvider) { X = 200f, Y = 100f };
        // World position is 200, width is 200, so left = 100, right = 300

        slider.UpdateValueFromPointer(100f); // Left edge
        Assert.Equal(0f, slider.Value, 2);

        slider.UpdateValueFromPointer(300f); // Right edge
        Assert.Equal(1f, slider.Value, 2);

        slider.UpdateValueFromPointer(200f); // Center
        Assert.Equal(0.5f, slider.Value, 2);

        // Clamps below
        slider.UpdateValueFromPointer(0f);
        Assert.Equal(0f, slider.Value, 2);

        // Clamps above
        slider.UpdateValueFromPointer(500f);
        Assert.Equal(1f, slider.Value, 2);
    }

    [Fact]
    public void UiSlider_Appearance_UsesOverrideWhenSet()
    {
        var slider = new UiSlider(200f, 20f, ThemeProvider);
        var custom = UiSliderAppearance.Default with { TrackHeight = 99f };
        slider.Appearance = custom;
        Assert.Same(custom, slider.Appearance);
    }

    [Fact]
    public void UiSlider_Draws()
    {
        using var bitmap = new SKBitmap(300, 100);
        using var canvas = new SKCanvas(bitmap);

        var slider = new UiSlider(200f, 20f, ThemeProvider) { Value = 0.7f };
        slider.Draw(canvas);
    }

    // ── UiJoystick ────────────────────────────────────────────────────

    [Fact]
    public void UiJoystick_HasCollider()
    {
        var js = new UiJoystick(80f, ThemeProvider);
        Assert.NotNull(js.Collider);
        Assert.IsType<CircleCollider>(js.Collider);
    }

    [Fact]
    public void UiJoystick_DefaultState()
    {
        var js = new UiJoystick(80f, ThemeProvider);
        Assert.Equal(SKPoint.Empty, js.Delta);
        Assert.Equal(SKPoint.Empty, js.NormalizedDelta);
        Assert.Equal(80f, js.Radius);
        Assert.Equal(0.6f, js.MaxRadiusFraction);
    }

    [Fact]
    public void UiJoystick_UpdateFromPointer_ClampsToMaxRadius()
    {
        var js = new UiJoystick(100f, ThemeProvider) { X = 300f, Y = 300f };
        // MaxRadiusFraction = 0.6, so max travel = 60

        js.UpdateFromPointer(400f, 300f); // 100px to the right, should clamp to 60
        float length = MathF.Sqrt(js.Delta.X * js.Delta.X + js.Delta.Y * js.Delta.Y);
        Assert.Equal(60f, length, 1);
    }

    [Fact]
    public void UiJoystick_UpdateFromPointer_SmallDelta()
    {
        var js = new UiJoystick(100f, ThemeProvider) { X = 300f, Y = 300f };

        js.UpdateFromPointer(310f, 305f); // Small delta, within range
        Assert.Equal(10f, js.Delta.X, 1);
        Assert.Equal(5f, js.Delta.Y, 1);
    }

    [Fact]
    public void UiJoystick_NormalizedDelta()
    {
        var js = new UiJoystick(100f, ThemeProvider) { X = 300f, Y = 300f };
        // Max travel = 60

        js.Delta = new SKPoint(30f, -30f);
        var norm = js.NormalizedDelta;
        Assert.Equal(0.5f, norm.X, 2);
        Assert.Equal(-0.5f, norm.Y, 2);
    }

    [Fact]
    public void UiJoystick_ResetDelta()
    {
        var js = new UiJoystick(100f, ThemeProvider);
        js.Delta = new SKPoint(20f, 10f);
        js.ResetDelta();
        Assert.Equal(SKPoint.Empty, js.Delta);
    }

    [Fact]
    public void UiJoystick_Appearance_UsesOverrideWhenSet()
    {
        var js = new UiJoystick(80f, ThemeProvider);
        var custom = UiJoystickAppearance.Default with { BorderWidth = 99f };
        js.Appearance = custom;
        Assert.Same(custom, js.Appearance);
    }

    [Fact]
    public void UiJoystick_Draws()
    {
        using var bitmap = new SKBitmap(300, 300);
        using var canvas = new SKCanvas(bitmap);

        var js = new UiJoystick(80f, ThemeProvider) { X = 150f, Y = 150f };
        js.Delta = new SKPoint(10f, -5f);
        js.Draw(canvas);
    }

    // ── Entity group collision ────────────────────────────────────────

    [Fact]
    public void MixedControls_CollisionDetection()
    {
        var group = new Entity();
        var button = new UiButton(100f, 40f, ThemeProvider) { X = 100f, Y = 50f };
        var checkbox = new UiCheckbox(30f, 30f, ThemeProvider) { X = 100f, Y = 120f };
        var slider = new UiSlider(200f, 20f, ThemeProvider) { X = 200f, Y = 200f };
        var joystick = new UiJoystick(60f, ThemeProvider) { X = 400f, Y = 300f };

        group.AddChild(button);
        group.AddChild(checkbox);
        group.AddChild(slider);
        group.AddChild(joystick);

        var probe = new Entity { Collider = new CircleCollider { Radius = 1f } };

        // Hit button
        probe.X = 100f; probe.Y = 50f;
        var hit = group.FindChildCollision(probe, out _);
        Assert.IsType<UiButton>(hit);

        // Hit checkbox
        probe.X = 100f; probe.Y = 120f;
        hit = group.FindChildCollision(probe, out _);
        Assert.IsType<UiCheckbox>(hit);

        // Hit slider
        probe.X = 200f; probe.Y = 200f;
        hit = group.FindChildCollision(probe, out _);
        Assert.IsType<UiSlider>(hit);

        // Hit joystick
        probe.X = 400f; probe.Y = 300f;
        hit = group.FindChildCollision(probe, out _);
        Assert.IsType<UiJoystick>(hit);

        // Miss all
        probe.X = 600f; probe.Y = 600f;
        hit = group.FindChildCollision(probe, out _);
        Assert.Null(hit);
    }

    [Fact]
    public void Controls_DrawViaEntityGroup()
    {
        using var bitmap = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bitmap);

        var group = new Entity();
        group.AddChild(new UiButton(100f, 40f, ThemeProvider) { X = 100f, Y = 50f, Label = "OK" });
        group.AddChild(new UiCheckbox(30f, 30f, ThemeProvider) { X = 100f, Y = 120f, IsChecked = true });
        group.AddChild(new UiSwitch(80f, 30f, ThemeProvider) { X = 100f, Y = 170f, IsOn = true });
        group.AddChild(new UiSlider(200f, 20f, ThemeProvider) { X = 200f, Y = 200f, Value = 0.75f });
        group.AddChild(new UiJoystick(60f, ThemeProvider) { X = 400f, Y = 300f });

        group.Draw(canvas); // Should render all controls without error
    }

    /// <summary>Test helper: a button appearance that calls a delegate on draw.</summary>
    private sealed record DelegateButtonAppearance(Action OnDrawCalled) : UiAppearance<UiButton>
    {
        public override void Draw(SKCanvas canvas, UiButton entity)
        {
            OnDrawCalled();
        }
    }
}
