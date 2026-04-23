using SkiaSharp;
using SkiaSharp.Theatre;
using SkiaSharp.Theatre.Themes.Default;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class HudControlEntityTests
{
    private static readonly HudTheme Theme = DefaultTheme.Create();

    private static T WithTheme<T>(T actor) where T : SceneNode
    {
        actor.HudTheme = Theme;
        return actor;
    }

    // ── HudButton ──────────────────────────────────────────────────────

    [Fact]
    public void HudButton_HasCollider()
    {
        var button = new HudButton(100f, 40f);
        Assert.NotNull(button.Collider);
        Assert.IsType<RectCollider>(button.Collider);
    }

    [Fact]
    public void HudButton_DefaultState()
    {
        var button = new HudButton(100f, 40f) { Label = "Go" };
        Assert.Equal("Go", button.Label);
        Assert.False(button.IsPressed);
        Assert.True(button.IsEnabled);
        Assert.Equal(18f, button.FontSize);
        Assert.Null(button.Appearance);
    }

    [Fact]
    public void HudButton_Appearance_UsesThemeByDefault()
    {
        var button = new HudButton(100f, 40f);
        Assert.Null(button.Appearance);
        // OnDraw falls back to Theme.Button
    }

    [Fact]
    public void HudButton_Appearance_UsesOverrideWhenSet()
    {
        var button = new HudButton(100f, 40f);
        var custom = DefaultButtonAppearance.Default with { CornerRadius = 99f };
        button.Appearance = custom;
        Assert.Same(custom, button.Appearance);
    }

    [Fact]
    public void HudButton_DimensionsMatch()
    {
        var button = new HudButton(150f, 60f);
        Assert.Equal(150f, button.Width);
        Assert.Equal(60f, button.Height);

        var collider = (RectCollider)button.Collider!;
        Assert.Equal(150f, collider.Width);
        Assert.Equal(60f, collider.Height);
    }

    [Fact]
    public void HudButton_Draws()
    {
        using var bitmap = new SKBitmap(200, 100);
        using var canvas = new SKCanvas(bitmap);

        var button = WithTheme(new HudButton(100f, 40f) { Label = "Test", X = 100f, Y = 50f });
        button.Draw(canvas); // Should not throw
    }

    [Fact]
    public void HudButton_CustomAppearance_IsInvoked()
    {
        using var bitmap = new SKBitmap(200, 100);
        using var canvas = new SKCanvas(bitmap);

        bool called = false;
        var button = new HudButton(100f, 40f)
        {
            Appearance = new DelegateButtonAppearance(() => called = true)
        };
        button.Draw(canvas);
        Assert.True(called);
    }

    [Fact]
    public void HudButton_CollisionDetection_Works()
    {
        var group = new Actor();
        var button = new HudButton(100f, 40f) { X = 200f, Y = 100f };
        group.Children.Add(button);

        var probe = new Actor { Collider = new CircleCollider { Radius = 1f } };

        probe.X = 200f; probe.Y = 100f;
        var hit = group.FindChildCollision(probe, out _);
        Assert.Same(button, hit);

        probe.X = 0f; probe.Y = 0f;
        hit = group.FindChildCollision(probe, out _);
        Assert.Null(hit);
    }

    // ── HudCheckbox ────────────────────────────────────────────────────

    [Fact]
    public void HudCheckbox_HasCollider()
    {
        var cb = new HudCheckbox(30f, 30f);
        Assert.NotNull(cb.Collider);
    }

    [Fact]
    public void HudCheckbox_DefaultState()
    {
        var cb = new HudCheckbox(30f, 30f);
        Assert.False(cb.IsChecked);
        Assert.Null(cb.Appearance);
    }

    [Fact]
    public void HudCheckbox_ToggleState()
    {
        var cb = new HudCheckbox(30f, 30f);
        cb.IsChecked = true;
        Assert.True(cb.IsChecked);
        cb.IsChecked = !cb.IsChecked;
        Assert.False(cb.IsChecked);
    }

    [Fact]
    public void HudCheckbox_Appearance_UsesOverrideWhenSet()
    {
        var cb = new HudCheckbox(30f, 30f);
        var custom = DefaultCheckboxAppearance.Default with { CornerRadius = 99f };
        cb.Appearance = custom;
        Assert.Same(custom, cb.Appearance);
    }

    [Fact]
    public void HudCheckbox_Draws()
    {
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        var cb = WithTheme(new HudCheckbox(30f, 30f) { IsChecked = true, X = 50f, Y = 50f });
        cb.Draw(canvas);
    }

    // ── HudSwitch ──────────────────────────────────────────────────────

    [Fact]
    public void HudSwitch_HasCollider()
    {
        var sw = new HudSwitch(100f, 40f);
        Assert.NotNull(sw.Collider);
    }

    [Fact]
    public void HudSwitch_DefaultState()
    {
        var sw = new HudSwitch(100f, 40f);
        Assert.False(sw.IsOn);
    }

    [Fact]
    public void HudButton_ToggleButton_ViaAppearance()
    {
        var btn = new HudButton(100f, 40f) { IsToggle = true };
        btn.Appearance = DefaultToggleButtonAppearance.Default;
        Assert.IsType<DefaultToggleButtonAppearance>(btn.Appearance);
    }

    [Fact]
    public void HudSwitch_Appearance_UsesOverrideWhenSet()
    {
        var sw = new HudSwitch(100f, 40f);
        var custom = DefaultSwitchAppearance.Default with { CornerRadius = 99f };
        sw.Appearance = custom;
        Assert.Same(custom, sw.Appearance);
    }

    [Fact]
    public void HudSwitch_And_ToggleButton_Draw()
    {
        using var bitmap = new SKBitmap(200, 100);
        using var canvas = new SKCanvas(bitmap);

        var sliding = WithTheme(new HudSwitch(100f, 40f) { IsOn = true });
        sliding.Draw(canvas);

        var toggle = WithTheme(new HudButton(100f, 40f) { IsToggle = true, IsOn = false, Appearance = DefaultToggleButtonAppearance.Default });
        toggle.Draw(canvas);
    }

    // ── HudSlider ──────────────────────────────────────────────────────

    [Fact]
    public void HudSlider_HasCollider()
    {
        var slider = new HudSlider(200f, 20f);
        Assert.NotNull(slider.Collider);
    }

    [Fact]
    public void HudSlider_DefaultValue()
    {
        var slider = new HudSlider(200f, 20f);
        Assert.Equal(0.5f, slider.Value);
    }

    [Fact]
    public void HudSlider_UpdateValueFromPointer()
    {
        var slider = new HudSlider(200f, 20f) { X = 200f, Y = 100f };
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
    public void HudSlider_Appearance_UsesOverrideWhenSet()
    {
        var slider = new HudSlider(200f, 20f);
        var custom = DefaultSliderAppearance.Default with { TrackHeight = 99f };
        slider.Appearance = custom;
        Assert.Same(custom, slider.Appearance);
    }

    [Fact]
    public void HudSlider_Draws()
    {
        using var bitmap = new SKBitmap(300, 100);
        using var canvas = new SKCanvas(bitmap);

        var slider = WithTheme(new HudSlider(200f, 20f) { Value = 0.7f });
        slider.Draw(canvas);
    }

    // ── HudJoystick ────────────────────────────────────────────────────

    [Fact]
    public void HudJoystick_HasCollider()
    {
        var js = new HudJoystick(80f);
        Assert.NotNull(js.Collider);
        Assert.IsType<CircleCollider>(js.Collider);
    }

    [Fact]
    public void HudJoystick_DefaultState()
    {
        var js = new HudJoystick(80f);
        Assert.Equal(SKPoint.Empty, js.Delta);
        Assert.Equal(SKPoint.Empty, js.NormalizedDelta);
        Assert.Equal(80f, js.Radius);
        Assert.Equal(0.6f, js.MaxRadiusFraction);
    }

    [Fact]
    public void HudJoystick_UpdateFromPointer_ClampsToMaxRadius()
    {
        var js = new HudJoystick(100f) { X = 300f, Y = 300f };
        // MaxRadiusFraction = 0.6, so max travel = 60

        js.UpdateFromPointer(400f, 300f); // 100px to the right, should clamp to 60
        float length = MathF.Sqrt(js.Delta.X * js.Delta.X + js.Delta.Y * js.Delta.Y);
        Assert.Equal(60f, length, 1);
    }

    [Fact]
    public void HudJoystick_UpdateFromPointer_SmallDelta()
    {
        var js = new HudJoystick(100f) { X = 300f, Y = 300f };

        js.UpdateFromPointer(310f, 305f); // Small delta, within range
        Assert.Equal(10f, js.Delta.X, 1);
        Assert.Equal(5f, js.Delta.Y, 1);
    }

    [Fact]
    public void HudJoystick_NormalizedDelta()
    {
        var js = new HudJoystick(100f) { X = 300f, Y = 300f };
        // Max travel = 60

        js.Delta = new SKPoint(30f, -30f);
        var norm = js.NormalizedDelta;
        Assert.Equal(0.5f, norm.X, 2);
        Assert.Equal(-0.5f, norm.Y, 2);
    }

    [Fact]
    public void HudJoystick_ResetDelta()
    {
        var js = new HudJoystick(100f);
        js.Delta = new SKPoint(20f, 10f);
        js.ResetDelta();
        Assert.Equal(SKPoint.Empty, js.Delta);
    }

    [Fact]
    public void HudJoystick_Appearance_UsesOverrideWhenSet()
    {
        var js = new HudJoystick(80f);
        var custom = DefaultJoystickAppearance.Default with { BorderWidth = 99f };
        js.Appearance = custom;
        Assert.Same(custom, js.Appearance);
    }

    [Fact]
    public void HudJoystick_Draws()
    {
        using var bitmap = new SKBitmap(300, 300);
        using var canvas = new SKCanvas(bitmap);

        var js = WithTheme(new HudJoystick(80f) { X = 150f, Y = 150f });
        js.Delta = new SKPoint(10f, -5f);
        js.Draw(canvas);
    }

    // ── Actor group collision ────────────────────────────────────────

    [Fact]
    public void MixedControls_CollisionDetection()
    {
        var group = new Actor();
        var button = new HudButton(100f, 40f) { X = 100f, Y = 50f };
        var checkbox = new HudCheckbox(30f, 30f) { X = 100f, Y = 120f };
        var slider = new HudSlider(200f, 20f) { X = 200f, Y = 200f };
        var joystick = new HudJoystick(60f) { X = 400f, Y = 300f };

        group.Children.Add(button);
        group.Children.Add(checkbox);
        group.Children.Add(slider);
        group.Children.Add(joystick);

        var probe = new Actor { Collider = new CircleCollider { Radius = 1f } };

        // Hit button
        probe.X = 100f; probe.Y = 50f;
        var hit = group.FindChildCollision(probe, out _);
        Assert.IsType<HudButton>(hit);

        // Hit checkbox
        probe.X = 100f; probe.Y = 120f;
        hit = group.FindChildCollision(probe, out _);
        Assert.IsType<HudCheckbox>(hit);

        // Hit slider
        probe.X = 200f; probe.Y = 200f;
        hit = group.FindChildCollision(probe, out _);
        Assert.IsType<HudSlider>(hit);

        // Hit joystick
        probe.X = 400f; probe.Y = 300f;
        hit = group.FindChildCollision(probe, out _);
        Assert.IsType<HudJoystick>(hit);

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

        var group = new Actor { HudTheme = Theme };
        group.Children.Add(new HudButton(100f, 40f) { X = 100f, Y = 50f, Label = "OK" });
        group.Children.Add(new HudCheckbox(30f, 30f) { X = 100f, Y = 120f, IsChecked = true });
        group.Children.Add(new HudSwitch(80f, 30f) { X = 100f, Y = 170f, IsOn = true });
        group.Children.Add(new HudSlider(200f, 20f) { X = 200f, Y = 200f, Value = 0.75f });
        group.Children.Add(new HudJoystick(60f) { X = 400f, Y = 300f });

        group.Draw(canvas); // Should render all controls without error
    }

    /// <summary>Test helper: a button appearance that calls a delegate on draw.</summary>
    private sealed record DelegateButtonAppearance(Action OnDrawCalled) : HudAppearance<HudButton>
    {
        public override void Draw(SKCanvas canvas, HudButton actor)
        {
            OnDrawCalled();
        }
    }
}
