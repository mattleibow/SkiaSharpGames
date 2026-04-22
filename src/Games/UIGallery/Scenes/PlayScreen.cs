using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.UIGallery;

internal sealed class PlayScreen : Scene
{
    private static readonly string[] ThemeNames = ["Bold/Cute", "Retro", "Simple", "Pixel Art", "Neon"];
    private static readonly HudTheme[] ThemeOptions = [HudThemes.BoldCute, HudThemes.Retro, HudThemes.Simple, HudThemes.PixelArt, HudThemes.Neon];

    private readonly UIGalleryState _state;
    private readonly HudTheme _theme;

    private readonly Actor _themeButtons = new();
    private readonly Actor _controls = new();

    private readonly HudButton[] _themeButtonList;

    private readonly HudButton _primaryButton;
    private readonly HudButton _overrideButton;
    private readonly HudCheckbox _checkbox;
    private readonly HudSwitch _slideSwitch;
    private readonly HudButton _toggleButton;
    private readonly HudSlider _slider;
    private readonly HudButton _customButton;
    private readonly HudJoystick _joystick;

    private bool _draggingSlider;
    private bool _draggingJoystick;

    public PlayScreen(UIGalleryState state, HudTheme theme)
    {
        _state = state;
        _theme = theme;

        // Theme selector buttons — one per theme, spaced dynamically
        _themeButtonList = new HudButton[ThemeNames.Length];
        float btnWidth = 120f;
        float gap = 10f;
        float totalWidth = ThemeNames.Length * btnWidth + (ThemeNames.Length - 1) * gap;
        float startX = (800f - totalWidth) / 2f + btnWidth / 2f;
        for (int i = 0; i < ThemeNames.Length; i++)
        {
            var btn = new HudButton(btnWidth, 36f, theme)
            {
                X = startX + i * (btnWidth + gap),
                Y = 41f,
                Label = ThemeNames[i],
                FontSize = 12f
            };
            _themeButtonList[i] = btn;
            _themeButtons.AddChild(btn);
        }

        // Demo controls — state lives on each actor
        _primaryButton = new HudButton(190f, 56f, theme) { X = 135f, Y = 138f, Label = "Button" };
        _overrideButton = new HudButton(190f, 56f, theme) { X = 345f, Y = 138f, Label = "Override" };
        _checkbox = new HudCheckbox(34f, 34f, theme) { X = 57f, Y = 221f, IsChecked = true };
        _slideSwitch = new HudSwitch(110f, 42f, theme) { X = 95f, Y = 289f };
        _toggleButton = new HudButton(130f, 42f, theme) { X = 233f, Y = 289f, IsToggle = true, Appearance = DefaultToggleButtonAppearance.Default };
        _slider = new HudSlider(320f, 26f, theme) { X = 200f, Y = 363f, Value = 0.45f };
        _customButton = new HudButton(260f, 58f, theme) { X = 170f, Y = 445f };
        _joystick = new HudJoystick(86f, theme) { X = 620f, Y = 360f };

        _controls.AddChild(_primaryButton);
        _controls.AddChild(_overrideButton);
        _controls.AddChild(_checkbox);
        _controls.AddChild(_slideSwitch);
        _controls.AddChild(_toggleButton);
        _controls.AddChild(_slider);
        _controls.AddChild(_customButton);
        _controls.AddChild(_joystick);

        ConfigureOverrides();

        Pointer = new HudPointer();
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

        if (Pointer!.FindHit(_themeButtons) is HudButton themeButton)
        {
            int idx = Array.IndexOf(_themeButtonList, themeButton);
            if (idx >= 0) { ApplyTheme(idx); return; }
        }

        _primaryButton.IsPressed = false;
        _overrideButton.IsPressed = false;

        var hit = Pointer!.FindHit(_controls);

        switch (hit)
        {
            case HudButton btn when btn == _primaryButton:
                _primaryButton.IsPressed = true;
                break;
            case HudButton btn when btn == _overrideButton:
                _overrideButton.IsPressed = true;
                break;
            case HudButton btn when btn == _toggleButton:
                btn.IsOn = !btn.IsOn;
                _slideSwitch.IsOn = btn.IsOn;
                break;
            case HudCheckbox cb:
                cb.IsChecked = !cb.IsChecked;
                break;
            case HudSwitch sw:
                sw.IsOn = !sw.IsOn;
                _toggleButton.IsOn = sw.IsOn;
                break;
            case HudSlider sl:
                _draggingSlider = true;
                sl.UpdateValueFromPointer(x);
                break;
            case HudJoystick js:
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
        var theme = _theme;
        using var backgroundPaint = new SKPaint { IsAntialias = true, Color = ((DefaultButtonAppearance)theme.Button).FillColor.WithAlpha(35) };
        using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
        using var headerFont = new SKFont(SKTypeface.Default, 26f);
        using var labelFont = new SKFont(SKTypeface.Default, 18f);

        canvas.Clear(new SKColor(0x10, 0x14, 0x1F));
        canvas.DrawRect(12f, 12f, width - 24f, height - 24f, backgroundPaint);

        DrawText(canvas, textPaint, headerFont, "UI Gallery", SKColors.White, 24f, 90f);
        DrawText(canvas, textPaint, labelFont, $"Theme: {ThemeNames[_state.ThemeIndex]}", new SKColor(0xD0, 0xDC, 0xEA), 620f, 46f);

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
        // Override button uses a custom green appearance
        _overrideButton.Appearance = DefaultButtonAppearance.Default with
        {
            FillColor = new SKColor(0x39, 0xC6, 0x91),
            PressedFillColor = new SKColor(0x1F, 0x8A, 0x64),
            BevelLightColor = new SKColor(0x9A, 0xFF, 0xD8),
            BevelShadowColor = new SKColor(0x0F, 0x4F, 0x3A),
            CornerRadius = 22f,
            BorderColor = SKColors.White,
        };

        // Custom button uses a completely custom appearance subclass
        _customButton.Appearance = new CustomPurpleAppearance();
    }

    private void UpdateThemeButtonStyles()
    {
        int idx = _state.ThemeIndex;
        for (int i = 0; i < _themeButtonList.Length; i++)
            UpdateThemeButtonStyle(_themeButtonList[i], ThemeOptions[i], idx == i);
    }

    private static void UpdateThemeButtonStyle(HudButton button, HudTheme themeTemplate, bool selected)
    {
        var baseAppearance = themeTemplate.Button;
        if (selected && baseAppearance is DefaultButtonAppearance typed)
            button.Appearance = typed with { BorderColor = SKColors.White, BorderWidth = typed.BorderWidth + 1f };
        else
            button.Appearance = baseAppearance;
        button.IsPressed = selected;
    }

    private void ApplyTheme(int index)
    {
        _state.ThemeIndex = Math.Clamp(index, 0, ThemeOptions.Length - 1);
        _theme.ApplyFrom(ThemeOptions[_state.ThemeIndex]);
    }

    private static void DrawText(SKCanvas canvas, SKPaint textPaint, SKFont font, string text, SKColor color, float x, float y)
    {
        textPaint.Color = color;
        canvas.DrawText(text, x, y, SKTextAlign.Left, font, textPaint);
    }

    /// <summary>Custom appearance that draws a purple button with a white ring.</summary>
    private sealed record CustomPurpleAppearance : HudAppearance<HudButton>
    {
        public override void Draw(SKCanvas canvas, HudButton button)
        {
            var rect = button.LocalRect;
            using var fill = new SKPaint { IsAntialias = true, Color = new SKColor(0x4A, 0x2E, 0xFF) };
            using var ring = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColors.White, StrokeWidth = 3f };
            canvas.DrawRoundRect(rect, 10f, 10f, fill);
            canvas.DrawRoundRect(rect, 10f, 10f, ring);
            using var labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
            using var labelFont = new SKFont(SKTypeface.Default, 16f);
            canvas.DrawText("Custom canvas draw", rect.MidX, rect.MidY + 6f, SKTextAlign.Center, labelFont, labelPaint);
        }
    }
}
