using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;

namespace SkiaSharpGames.UIGallery;

internal sealed class PlayScreen : GameScreen
{
    private const string ThemeBoldId = "theme-bold";
    private const string ThemeRetroId = "theme-retro";
    private const string ThemeSimpleId = "theme-simple";

    private const string PrimaryButtonId = "primary-button";
    private const string OverrideButtonId = "override-button";
    private const string CheckboxId = "checkbox";
    private const string SlideSwitchId = "slide-switch";
    private const string ToggleSwitchId = "toggle-switch";
    private const string SliderId = "slider";
    private const string CustomButtonId = "custom-button";
    private const string JoystickId = "joystick";

    private static readonly UiTheme[] ThemeOptions = [UiThemes.BoldCute, UiThemes.Retro, UiThemes.Simple];

    private readonly UIGalleryState _state;
    private readonly IUiThemeProvider _themes;

    private readonly Entity _themeButtons = new();
    private readonly Entity _controls = new();
    private readonly Entity _pointerProbe = new() { Collider = new CircleCollider { Radius = 2f } };

    private readonly UiControlEntity _themeBoldButton;
    private readonly UiControlEntity _themeRetroButton;
    private readonly UiControlEntity _themeSimpleButton;

    private readonly UiControlEntity _primaryButton;
    private readonly UiControlEntity _overrideButton;
    private readonly UiControlEntity _checkbox;
    private readonly UiControlEntity _slideSwitch;
    private readonly UiControlEntity _toggleSwitch;
    private readonly UiControlEntity _slider;
    private readonly UiControlEntity _customButton;
    private readonly UiControlEntity _joystick;

    private bool _draggingSlider;
    private bool _draggingJoystick;

    public PlayScreen(UIGalleryState state, IUiThemeProvider themes)
    {
        _state = state;
        _themes = themes;

        _themeBoldButton = CreateRectControl(_themeButtons, ThemeBoldId, SKRect.Create(24f, 20f, 160f, 42f));
        _themeRetroButton = CreateRectControl(_themeButtons, ThemeRetroId, SKRect.Create(194f, 20f, 160f, 42f));
        _themeSimpleButton = CreateRectControl(_themeButtons, ThemeSimpleId, SKRect.Create(364f, 20f, 160f, 42f));

        _primaryButton = CreateRectControl(_controls, PrimaryButtonId, SKRect.Create(40f, 110f, 190f, 56f));
        _overrideButton = CreateRectControl(_controls, OverrideButtonId, SKRect.Create(250f, 110f, 190f, 56f));
        _checkbox = CreateRectControl(_controls, CheckboxId, SKRect.Create(40f, 204f, 34f, 34f));
        _slideSwitch = CreateRectControl(_controls, SlideSwitchId, SKRect.Create(40f, 268f, 110f, 42f));
        _toggleSwitch = CreateRectControl(_controls, ToggleSwitchId, SKRect.Create(168f, 268f, 130f, 42f));
        _slider = CreateRectControl(_controls, SliderId, SKRect.Create(40f, 350f, 320f, 26f));
        _customButton = CreateRectControl(_controls, CustomButtonId, SKRect.Create(40f, 416f, 260f, 58f));
        _joystick = CreateCircleControl(_controls, JoystickId, 620f, 360f, 86f);

        ConfigureSprites();
    }

    public override void OnActivated()
    {
        _state.PrimaryPressed = false;
        _state.OverridePressed = false;
        _draggingSlider = false;
        _draggingJoystick = false;
        _state.JoystickDelta = SKPoint.Empty;
        ApplyTheme(_state.ThemeIndex);
    }

    public override void OnPointerDown(float x, float y)
    {
        _pointerProbe.X = x;
        _pointerProbe.Y = y;

        if (_themeButtons.FindChildCollision(_pointerProbe, out _) is UiControlEntity themeButton)
        {
            switch (themeButton.Id)
            {
                case ThemeBoldId: ApplyTheme(0); return;
                case ThemeRetroId: ApplyTheme(1); return;
                case ThemeSimpleId: ApplyTheme(2); return;
            }
        }

        _state.PrimaryPressed = false;
        _state.OverridePressed = false;

        var hit = _controls.FindChildCollision(_pointerProbe, out _) as UiControlEntity;
        if (hit is null)
            return;

        switch (hit.Id)
        {
            case PrimaryButtonId:
                _state.PrimaryPressed = true;
                break;
            case OverrideButtonId:
                _state.OverridePressed = true;
                break;
            case CheckboxId:
                _state.CheckboxChecked = !_state.CheckboxChecked;
                break;
            case SlideSwitchId:
            case ToggleSwitchId:
                _state.SwitchOn = !_state.SwitchOn;
                break;
            case SliderId:
                _draggingSlider = true;
                UpdateSliderValue(x);
                break;
            case JoystickId:
                _draggingJoystick = true;
                UpdateJoystick(x, y);
                break;
        }
    }

    public override void OnPointerMove(float x, float y)
    {
        if (_draggingSlider)
            UpdateSliderValue(x);

        if (_draggingJoystick)
            UpdateJoystick(x, y);
    }

    public override void OnPointerUp(float x, float y)
    {
        _state.PrimaryPressed = false;
        _state.OverridePressed = false;
        _draggingSlider = false;
        _draggingJoystick = false;
        _state.JoystickDelta = SKPoint.Empty;
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

        _themeButtons.Draw(canvas);
        _controls.Draw(canvas);

        DrawText(canvas, textPaint, labelFont, "Checkbox", SKColors.White, 86f, 229f);
        DrawText(canvas, textPaint, labelFont, "Switch (slide + toggle)", SKColors.White, 40f, 332f);
        DrawText(canvas, textPaint, labelFont, $"Slider: {(int)(_state.SliderValue * 100f)}", SKColors.White, 40f, 402f);
        DrawText(canvas, textPaint, labelFont, "Custom draw hook", SKColors.White, 40f, 494f);

        float joyX = _state.JoystickDelta.X / (_joystick.Radius * 0.6f);
        float joyY = _state.JoystickDelta.Y / (_joystick.Radius * 0.6f);
        DrawText(canvas, textPaint, labelFont, $"Joystick X:{joyX:F2} Y:{joyY:F2}", new SKColor(0xD0, 0xDC, 0xEA), 500f, 500f);
    }

    private void ConfigureSprites()
    {
        _themeBoldButton.Sprite = new DelegateSprite(canvas => DrawThemeButton(canvas, _themeBoldButton, "Bold/Cute", UiThemes.BoldCute.Button, _themes.Theme.Name == UiThemes.BoldCute.Name));
        _themeRetroButton.Sprite = new DelegateSprite(canvas => DrawThemeButton(canvas, _themeRetroButton, "Retro", UiThemes.Retro.Button, _themes.Theme.Name == UiThemes.Retro.Name));
        _themeSimpleButton.Sprite = new DelegateSprite(canvas => DrawThemeButton(canvas, _themeSimpleButton, "Simple", UiThemes.Simple.Button, _themes.Theme.Name == UiThemes.Simple.Name));

        _primaryButton.Sprite = new DelegateSprite(canvas =>
            UiControls.DrawButton(canvas, RectFor(_primaryButton), "Button", _themes.Theme.Button, _state.PrimaryPressed));

        _overrideButton.Sprite = new DelegateSprite(canvas =>
        {
            var style = _themes.Theme.Button with
            {
                FillColor = new SKColor(0x39, 0xC6, 0x91),
                PressedFillColor = new SKColor(0x1F, 0x8A, 0x64),
                BevelLightColor = new SKColor(0x9A, 0xFF, 0xD8),
                BevelShadowColor = new SKColor(0x0F, 0x4F, 0x3A),
                CornerRadius = 22f,
                BorderColor = SKColors.White,
            };
            UiControls.DrawButton(canvas, RectFor(_overrideButton), "Override", style, _state.OverridePressed);
        });

        _checkbox.Sprite = new DelegateSprite(canvas =>
            UiControls.DrawCheckbox(canvas, RectFor(_checkbox), _state.CheckboxChecked, _themes.Theme.Checkbox));

        _slideSwitch.Sprite = new DelegateSprite(canvas =>
            UiControls.DrawSwitch(canvas, RectFor(_slideSwitch), _state.SwitchOn, _themes.Theme.Switch, UiSwitchVariant.Sliding));

        _toggleSwitch.Sprite = new DelegateSprite(canvas =>
            UiControls.DrawSwitch(canvas, RectFor(_toggleSwitch), _state.SwitchOn, _themes.Theme.Switch, UiSwitchVariant.ToggleButton));

        _slider.Sprite = new DelegateSprite(canvas =>
            UiControls.DrawSlider(canvas, RectFor(_slider), _state.SliderValue, _themes.Theme.Slider));

        _customButton.Sprite = new DelegateSprite(canvas =>
            UiControls.DrawButton(
                canvas,
                RectFor(_customButton),
                "",
                _themes.Theme.Button,
                false,
                customDraw: static (c, rect, style, _, _) =>
                {
                    using var fill = new SKPaint { IsAntialias = true, Color = new SKColor(0x4A, 0x2E, 0xFF) };
                    using var ring = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColors.White, StrokeWidth = 3f };
                    c.DrawRoundRect(rect, 10f, 10f, fill);
                    c.DrawRoundRect(rect, 10f, 10f, ring);
                    using var labelPaint = new SKPaint { IsAntialias = true, Color = style.TextColor };
                    using var labelFont = new SKFont(SKTypeface.Default, 16f);
                    c.DrawText("Custom canvas draw", rect.MidX, rect.MidY + 6f, SKTextAlign.Center, labelFont, labelPaint);
                }));

        _joystick.Sprite = new DelegateSprite(canvas =>
            UiControls.DrawJoystick(canvas, SKPoint.Empty, _joystick.Radius, _state.JoystickDelta, _themes.Theme.Joystick));
    }

    private static void DrawThemeButton(SKCanvas canvas, UiControlEntity entity, string label, UiButtonStyle style, bool selected)
    {
        var themedStyle = selected
            ? style with { BorderColor = SKColors.White, BorderWidth = style.BorderWidth + 1f }
            : style;

        UiControls.DrawButton(canvas, RectFor(entity), label, themedStyle, pressed: selected, fontSize: 14f);
    }

    private static UiControlEntity CreateRectControl(Entity parent, string id, SKRect rect)
    {
        var entity = new UiControlEntity(id, rect.MidX, rect.MidY, rect.Width, rect.Height);
        parent.AddChild(entity);
        return entity;
    }

    private static UiControlEntity CreateCircleControl(Entity parent, string id, float x, float y, float radius)
    {
        var entity = new UiControlEntity(id, x, y, radius);
        parent.AddChild(entity);
        return entity;
    }

    private static SKRect RectFor(UiControlEntity entity) =>
        SKRect.Create(-entity.Width / 2f, -entity.Height / 2f, entity.Width, entity.Height);

    private void UpdateSliderValue(float x)
    {
        float left = _slider.WorldX - _slider.Width / 2f;
        _state.SliderValue = Math.Clamp((x - left) / _slider.Width, 0f, 1f);
    }

    private void UpdateJoystick(float x, float y)
    {
        _state.JoystickDelta = UiControls.ClampJoystick(
            new SKPoint(x - _joystick.WorldX, y - _joystick.WorldY),
            _joystick.Radius * 0.6f);
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
