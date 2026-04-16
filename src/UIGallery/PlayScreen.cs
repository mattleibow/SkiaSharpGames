using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;

namespace SkiaSharpGames.UIGallery;

internal sealed class PlayScreen(UIGalleryState state, IUiThemeProvider themes) : GameScreen
{
    private static readonly UiTheme[] ThemeOptions = [UiThemes.BoldCute, UiThemes.Retro, UiThemes.Simple];

    private static readonly SKPaint _bgPaint = new() { IsAntialias = true };
    private static readonly SKPaint _textPaint = new() { IsAntialias = true, Color = SKColors.White };
    private static readonly SKFont _headerFont = new(SKTypeface.Default, 26f);
    private static readonly SKFont _labelFont = new(SKTypeface.Default, 18f);

    private static readonly SKRect BoldThemeRect = SKRect.Create(24f, 20f, 160f, 42f);
    private static readonly SKRect RetroThemeRect = SKRect.Create(194f, 20f, 160f, 42f);
    private static readonly SKRect SimpleThemeRect = SKRect.Create(364f, 20f, 160f, 42f);

    private static readonly SKRect PrimaryButtonRect = SKRect.Create(40f, 110f, 190f, 56f);
    private static readonly SKRect OverrideButtonRect = SKRect.Create(250f, 110f, 190f, 56f);
    private static readonly SKRect CheckboxRect = SKRect.Create(40f, 204f, 34f, 34f);
    private static readonly SKRect SwitchRect = SKRect.Create(40f, 268f, 110f, 42f);
    private static readonly SKRect ToggleRect = SKRect.Create(168f, 268f, 130f, 42f);
    private static readonly SKRect SliderRect = SKRect.Create(40f, 350f, 320f, 26f);
    private static readonly SKRect CustomButtonRect = SKRect.Create(40f, 416f, 260f, 58f);

    private static readonly SKPoint JoystickCenter = new(620f, 360f);
    private const float JoystickRadius = 86f;

    private bool _draggingSlider;
    private bool _draggingJoystick;

    public override void OnActivated() => ApplyTheme(state.ThemeIndex);

    public override void OnPointerDown(float x, float y)
    {
        if (UiControls.HitTest(BoldThemeRect, x, y)) { ApplyTheme(0); return; }
        if (UiControls.HitTest(RetroThemeRect, x, y)) { ApplyTheme(1); return; }
        if (UiControls.HitTest(SimpleThemeRect, x, y)) { ApplyTheme(2); return; }

        state.PrimaryPressed = UiControls.HitTest(PrimaryButtonRect, x, y);
        state.OverridePressed = UiControls.HitTest(OverrideButtonRect, x, y);

        if (UiControls.HitTest(CheckboxRect, x, y))
            state.CheckboxChecked = !state.CheckboxChecked;

        if (UiControls.HitTest(SwitchRect, x, y) || UiControls.HitTest(ToggleRect, x, y))
            state.SwitchOn = !state.SwitchOn;

        if (UiControls.HitTest(SliderRect, x, y))
        {
            _draggingSlider = true;
            state.SliderValue = UiControls.SliderValueFromX(SliderRect, x);
        }

        var joystickDistance = Distance(new SKPoint(x, y), JoystickCenter);
        if (joystickDistance <= JoystickRadius)
        {
            _draggingJoystick = true;
            state.JoystickDelta = UiControls.ClampJoystick(new SKPoint(x - JoystickCenter.X, y - JoystickCenter.Y), JoystickRadius * 0.6f);
        }
    }

    public override void OnPointerMove(float x, float y)
    {
        if (_draggingSlider)
            state.SliderValue = UiControls.SliderValueFromX(SliderRect, x);

        if (_draggingJoystick)
            state.JoystickDelta = UiControls.ClampJoystick(new SKPoint(x - JoystickCenter.X, y - JoystickCenter.Y), JoystickRadius * 0.6f);
    }

    public override void OnPointerUp(float x, float y)
    {
        state.PrimaryPressed = false;
        state.OverridePressed = false;
        _draggingSlider = false;
        _draggingJoystick = false;
        state.JoystickDelta = SKPoint.Empty;
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        var theme = themes.Theme;

        _bgPaint.Color = theme.Button.FillColor.WithAlpha(35);
        canvas.Clear(new SKColor(0x10, 0x14, 0x1F));
        canvas.DrawRect(12f, 12f, width - 24f, height - 24f, _bgPaint);

        DrawText(canvas, "UI Gallery", _headerFont, SKColors.White, 24f, 90f);
        DrawText(canvas, $"Theme: {theme.Name}", _labelFont, new SKColor(0xD0, 0xDC, 0xEA), 620f, 46f);

        DrawThemeButtons(canvas, theme);

        UiControls.DrawButton(canvas, PrimaryButtonRect, "Button", theme.Button, state.PrimaryPressed);

        var overrideStyle = theme.Button with
        {
            FillColor = new SKColor(0x39, 0xC6, 0x91),
            PressedFillColor = new SKColor(0x1F, 0x8A, 0x64),
            BevelLightColor = new SKColor(0x9A, 0xFF, 0xD8),
            BevelShadowColor = new SKColor(0x0F, 0x4F, 0x3A),
            CornerRadius = 22f,
            BorderColor = SKColors.White,
        };
        UiControls.DrawButton(canvas, OverrideButtonRect, "Override", overrideStyle, state.OverridePressed);

        UiControls.DrawCheckbox(canvas, CheckboxRect, state.CheckboxChecked, theme.Checkbox);
        DrawText(canvas, "Checkbox", _labelFont, SKColors.White, 86f, 229f);

        UiControls.DrawSwitch(canvas, SwitchRect, state.SwitchOn, theme.Switch, UiSwitchVariant.Sliding);
        UiControls.DrawSwitch(canvas, ToggleRect, state.SwitchOn, theme.Switch, UiSwitchVariant.ToggleButton);
        DrawText(canvas, "Switch (slide + toggle)", _labelFont, SKColors.White, 40f, 332f);

        UiControls.DrawSlider(canvas, SliderRect, state.SliderValue, theme.Slider);
        DrawText(canvas, $"Slider: {(int)(state.SliderValue * 100f)}", _labelFont, SKColors.White, 40f, 402f);

        UiControls.DrawButton(
            canvas,
            CustomButtonRect,
            "",
            theme.Button,
            state.PrimaryPressed,
            customDraw: static (c, rect, style, _, _) =>
            {
                using var fill = new SKPaint { IsAntialias = true, Color = new SKColor(0x4A, 0x2E, 0xFF) };
                using var ring = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColors.White, StrokeWidth = 3f };
                c.DrawRoundRect(rect, 10f, 10f, fill);
                c.DrawRoundRect(rect, 10f, 10f, ring);
                using var labelPaint = new SKPaint { IsAntialias = true, Color = style.TextColor, TextAlign = SKTextAlign.Center };
                using var labelFont = new SKFont(SKTypeface.Default, 16f);
                c.DrawText("Custom canvas draw", rect.MidX, rect.MidY + 6f, labelFont, labelPaint);
            });

        DrawText(canvas, "Custom draw hook", _labelFont, SKColors.White, 40f, 494f);

        UiControls.DrawJoystick(canvas, JoystickCenter, JoystickRadius, state.JoystickDelta, theme.Joystick);

        float joyX = state.JoystickDelta.X / (JoystickRadius * 0.6f);
        float joyY = state.JoystickDelta.Y / (JoystickRadius * 0.6f);
        DrawText(canvas, $"Joystick X:{joyX:F2} Y:{joyY:F2}", _labelFont, new SKColor(0xD0, 0xDC, 0xEA), 500f, 500f);
    }

    private static float Distance(SKPoint a, SKPoint b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private void ApplyTheme(int index)
    {
        state.ThemeIndex = Math.Clamp(index, 0, ThemeOptions.Length - 1);
        themes.Theme = ThemeOptions[state.ThemeIndex];
    }

    private static void DrawThemeButtons(SKCanvas canvas, UiTheme theme)
    {
        DrawThemeButton(canvas, BoldThemeRect, "Bold/Cute", theme.Name == UiThemes.BoldCute.Name, UiThemes.BoldCute.Button);
        DrawThemeButton(canvas, RetroThemeRect, "Retro", theme.Name == UiThemes.Retro.Name, UiThemes.Retro.Button);
        DrawThemeButton(canvas, SimpleThemeRect, "Simple", theme.Name == UiThemes.Simple.Name, UiThemes.Simple.Button);
    }

    private static void DrawThemeButton(SKCanvas canvas, SKRect rect, string label, bool selected, UiButtonStyle style)
    {
        var themed = selected
            ? style with { BorderColor = SKColors.White, BorderWidth = style.BorderWidth + 1f }
            : style;
        UiControls.DrawButton(canvas, rect, label, themed, pressed: selected, fontSize: 14f);
    }

    private static void DrawText(SKCanvas canvas, string text, SKFont font, SKColor color, float x, float y)
    {
        _textPaint.Color = color;
        _textPaint.TextAlign = SKTextAlign.Left;
        canvas.DrawText(text, x, y, font, _textPaint);
    }
}
