using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;

namespace SkiaSharpGames.UIGallery;

internal sealed class PlayScreen : GameScreen
{
    private static readonly UiTheme[] ThemeOptions = [UiThemes.BoldCute, UiThemes.Retro, UiThemes.Simple];

    private readonly UIGalleryState _state;
    private readonly IUiThemeProvider _themes;

    private readonly Entity _themeButtons = new();
    private readonly Entity _controls = new();

    private readonly UiButton _themeBoldButton;
    private readonly UiButton _themeRetroButton;
    private readonly UiButton _themeSimpleButton;

    private readonly UiButton _primaryButton;
    private readonly UiButton _overrideButton;
    private readonly UiCheckbox _checkbox;
    private readonly UiSwitch _slideSwitch;
    private readonly UiSwitch _toggleSwitch;
    private readonly UiSlider _slider;
    private readonly UiButton _customButton;
    private readonly UiJoystick _joystick;

    private bool _draggingSlider;
    private bool _draggingJoystick;

    public PlayScreen(UIGalleryState state, IUiThemeProvider themes)
    {
        _state = state;
        _themes = themes;

        // Theme selector buttons
        _themeBoldButton = new UiButton(160f, 42f, themes) { X = 104f, Y = 41f, Label = "Bold/Cute", FontSize = 14f };
        _themeRetroButton = new UiButton(160f, 42f, themes) { X = 274f, Y = 41f, Label = "Retro", FontSize = 14f };
        _themeSimpleButton = new UiButton(160f, 42f, themes) { X = 444f, Y = 41f, Label = "Simple", FontSize = 14f };
        _themeButtons.AddChild(_themeBoldButton);
        _themeButtons.AddChild(_themeRetroButton);
        _themeButtons.AddChild(_themeSimpleButton);

        // Demo controls — state lives on each entity
        _primaryButton = new UiButton(190f, 56f, themes) { X = 135f, Y = 138f, Label = "Button" };
        _overrideButton = new UiButton(190f, 56f, themes) { X = 345f, Y = 138f, Label = "Override" };
        _checkbox = new UiCheckbox(34f, 34f, themes) { X = 57f, Y = 221f, IsChecked = true };
        _slideSwitch = new UiSwitch(110f, 42f, themes, UiSwitchVariant.Sliding) { X = 95f, Y = 289f };
        _toggleSwitch = new UiSwitch(130f, 42f, themes, UiSwitchVariant.ToggleButton) { X = 233f, Y = 289f };
        _slider = new UiSlider(320f, 26f, themes) { X = 200f, Y = 363f, Value = 0.45f };
        _customButton = new UiButton(260f, 58f, themes) { X = 170f, Y = 445f };
        _joystick = new UiJoystick(86f, themes) { X = 620f, Y = 360f };

        _controls.AddChild(_primaryButton);
        _controls.AddChild(_overrideButton);
        _controls.AddChild(_checkbox);
        _controls.AddChild(_slideSwitch);
        _controls.AddChild(_toggleSwitch);
        _controls.AddChild(_slider);
        _controls.AddChild(_customButton);
        _controls.AddChild(_joystick);

        ConfigureOverrides();

        Pointer = new UiPointer();
    }

    public override void OnActivated()
    {
        _primaryButton.IsPressed = false;
        _overrideButton.IsPressed = false;
        _draggingSlider = false;
        _draggingJoystick = false;
        _joystick.ResetDelta();
        ApplyTheme(_state.ThemeIndex);
    }

    public override void OnPointerDown(float x, float y)
    {
        // Pointer position is already set by the engine

        if (Pointer!.FindHit(_themeButtons) is UiButton themeButton)
        {
            if (themeButton == _themeBoldButton) { ApplyTheme(0); return; }
            if (themeButton == _themeRetroButton) { ApplyTheme(1); return; }
            if (themeButton == _themeSimpleButton) { ApplyTheme(2); return; }
        }

        _primaryButton.IsPressed = false;
        _overrideButton.IsPressed = false;

        var hit = Pointer!.FindHit(_controls);

        switch (hit)
        {
            case UiButton btn when btn == _primaryButton:
                _primaryButton.IsPressed = true;
                break;
            case UiButton btn when btn == _overrideButton:
                _overrideButton.IsPressed = true;
                break;
            case UiCheckbox cb:
                cb.IsChecked = !cb.IsChecked;
                break;
            case UiSwitch sw:
                sw.IsOn = !sw.IsOn;
                // Keep both switches in sync for the demo
                _slideSwitch.IsOn = sw.IsOn;
                _toggleSwitch.IsOn = sw.IsOn;
                break;
            case UiSlider sl:
                _draggingSlider = true;
                sl.UpdateValueFromPointer(x);
                break;
            case UiJoystick js:
                _draggingJoystick = true;
                js.UpdateFromPointer(x, y);
                break;
        }
    }

    public override void OnPointerMove(float x, float y)
    {
        if (_draggingSlider)
            _slider.UpdateValueFromPointer(x);

        if (_draggingJoystick)
            _joystick.UpdateFromPointer(x, y);
    }

    public override void OnPointerUp(float x, float y)
    {
        _primaryButton.IsPressed = false;
        _overrideButton.IsPressed = false;
        _draggingSlider = false;
        _draggingJoystick = false;
        _joystick.ResetDelta();
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        var theme = _themes.Theme;
        using var backgroundPaint = new SKPaint { IsAntialias = true, Color = theme.Button.FillColor.WithAlpha(35) };
        using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
        using var headerFont = new SKFont(SKTypeface.Default, 26f);
        using var labelFont = new SKFont(SKTypeface.Default, 18f);

        canvas.Clear(new SKColor(0x10, 0x14, 0x1F));
        canvas.DrawRect(12f, 12f, width - 24f, height - 24f, backgroundPaint);

        DrawText(canvas, textPaint, headerFont, "UI Gallery", SKColors.White, 24f, 90f);
        DrawText(canvas, textPaint, labelFont, $"Theme: {theme.Name}", new SKColor(0xD0, 0xDC, 0xEA), 620f, 46f);

        // Update theme button styles to show selection
        UpdateThemeButtonStyles();

        _themeButtons.Draw(canvas);
        _controls.Draw(canvas);

        DrawText(canvas, textPaint, labelFont, "Checkbox", SKColors.White, 86f, 229f);
        DrawText(canvas, textPaint, labelFont, "Switch (slide + toggle)", SKColors.White, 40f, 332f);
        DrawText(canvas, textPaint, labelFont, $"Slider: {(int)(_slider.Value * 100f)}", SKColors.White, 40f, 402f);
        DrawText(canvas, textPaint, labelFont, "Custom draw hook", SKColors.White, 40f, 494f);

        var norm = _joystick.NormalizedDelta;
        DrawText(canvas, textPaint, labelFont, $"Joystick X:{norm.X:F2} Y:{norm.Y:F2}", new SKColor(0xD0, 0xDC, 0xEA), 500f, 500f);

        Pointer?.Draw(canvas);
    }

    private void ConfigureOverrides()
    {
        // Override button uses a custom green style
        _overrideButton.StyleOverride = _themes.Theme.Button with
        {
            FillColor = new SKColor(0x39, 0xC6, 0x91),
            PressedFillColor = new SKColor(0x1F, 0x8A, 0x64),
            BevelLightColor = new SKColor(0x9A, 0xFF, 0xD8),
            BevelShadowColor = new SKColor(0x0F, 0x4F, 0x3A),
            CornerRadius = 22f,
            BorderColor = SKColors.White,
        };

        // Custom button uses a completely custom draw callback
        _customButton.CustomDraw = static (c, rect, style, _, _) =>
        {
            using var fill = new SKPaint { IsAntialias = true, Color = new SKColor(0x4A, 0x2E, 0xFF) };
            using var ring = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColors.White, StrokeWidth = 3f };
            c.DrawRoundRect(rect, 10f, 10f, fill);
            c.DrawRoundRect(rect, 10f, 10f, ring);
            using var labelPaint = new SKPaint { IsAntialias = true, Color = style.TextColor };
            using var labelFont = new SKFont(SKTypeface.Default, 16f);
            c.DrawText("Custom canvas draw", rect.MidX, rect.MidY + 6f, SKTextAlign.Center, labelFont, labelPaint);
        };
    }

    private void UpdateThemeButtonStyles()
    {
        var themeName = _themes.Theme.Name;
        UpdateThemeButtonStyle(_themeBoldButton, UiThemes.BoldCute, themeName == UiThemes.BoldCute.Name);
        UpdateThemeButtonStyle(_themeRetroButton, UiThemes.Retro, themeName == UiThemes.Retro.Name);
        UpdateThemeButtonStyle(_themeSimpleButton, UiThemes.Simple, themeName == UiThemes.Simple.Name);
    }

    private static void UpdateThemeButtonStyle(UiButton button, UiTheme theme, bool selected)
    {
        button.StyleOverride = selected
            ? theme.Button with { BorderColor = SKColors.White, BorderWidth = theme.Button.BorderWidth + 1f }
            : theme.Button;
        button.IsPressed = selected;
    }

    private void ApplyTheme(int index)
    {
        _state.ThemeIndex = Math.Clamp(index, 0, ThemeOptions.Length - 1);
        _themes.Theme = ThemeOptions[_state.ThemeIndex];
    }

    private static void DrawText(SKCanvas canvas, SKPaint textPaint, SKFont font, string text, SKColor color, float x, float y)
    {
        textPaint.Color = color;
        canvas.DrawText(text, x, y, SKTextAlign.Left, font, textPaint);
    }
}
