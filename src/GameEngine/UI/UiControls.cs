using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

public static class UiControls
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private static readonly SKPaint _strokePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private static readonly SKPaint _textPaint = new() { IsAntialias = true };
    private static readonly SKFont _font = new(SKTypeface.Default, 18f);

    public static bool HitTest(SKRect rect, float x, float y) => rect.Contains(x, y);

    public static void DrawButton(
        SKCanvas canvas,
        SKRect rect,
        string label,
        UiButtonStyle style,
        bool pressed = false,
        bool enabled = true,
        float fontSize = 18f,
        Action<SKCanvas, SKRect, UiButtonStyle, bool, bool>? customDraw = null)
    {
        if (customDraw is not null)
        {
            customDraw(canvas, rect, style, pressed, enabled);
            return;
        }

        byte alpha = enabled ? (byte)255 : style.DisabledAlpha;

        _fillPaint.Color = (pressed ? style.PressedFillColor : style.FillColor).WithAlpha(alpha);
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, _fillPaint);

        if (style.BevelSize > 0f)
        {
            _strokePaint.StrokeWidth = style.BevelSize;
            _strokePaint.Color = (pressed ? style.BevelShadowColor : style.BevelLightColor).WithAlpha(alpha);
            var inset = SKRect.Inflate(rect, -style.BevelSize * 0.5f, -style.BevelSize * 0.5f);
            canvas.DrawLine(inset.Left, inset.Top, inset.Right, inset.Top, _strokePaint);
            canvas.DrawLine(inset.Left, inset.Top, inset.Left, inset.Bottom, _strokePaint);

            _strokePaint.Color = (pressed ? style.BevelLightColor : style.BevelShadowColor).WithAlpha(alpha);
            canvas.DrawLine(inset.Left, inset.Bottom, inset.Right, inset.Bottom, _strokePaint);
            canvas.DrawLine(inset.Right, inset.Top, inset.Right, inset.Bottom, _strokePaint);
        }

        _strokePaint.StrokeWidth = style.BorderWidth;
        _strokePaint.Color = style.BorderColor.WithAlpha(alpha);
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, _strokePaint);

        _textPaint.Color = style.TextColor.WithAlpha(alpha);
        _font.Size = fontSize;
        float baselineY = rect.MidY + fontSize * 0.35f;
        canvas.DrawText(label, rect.MidX, baselineY, SKTextAlign.Center, _font, _textPaint);
    }

    public static void DrawCheckbox(
        SKCanvas canvas,
        SKRect rect,
        bool isChecked,
        UiCheckboxStyle style,
        Action<SKCanvas, SKRect, UiCheckboxStyle, bool>? customDraw = null)
    {
        if (customDraw is not null)
        {
            customDraw(canvas, rect, style, isChecked);
            return;
        }

        _fillPaint.Color = style.FillColor;
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, _fillPaint);

        _strokePaint.StrokeWidth = style.BorderWidth;
        _strokePaint.Color = style.BorderColor;
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, _strokePaint);

        if (!isChecked)
            return;

        _strokePaint.Color = style.CheckColor;
        _strokePaint.StrokeCap = SKStrokeCap.Round;
        _strokePaint.StrokeWidth = MathF.Max(2f, rect.Width * 0.12f);
        canvas.DrawLine(rect.Left + rect.Width * 0.2f, rect.MidY, rect.Left + rect.Width * 0.45f, rect.Bottom - rect.Height * 0.2f, _strokePaint);
        canvas.DrawLine(rect.Left + rect.Width * 0.45f, rect.Bottom - rect.Height * 0.2f, rect.Right - rect.Width * 0.2f, rect.Top + rect.Height * 0.2f, _strokePaint);
        _strokePaint.StrokeCap = SKStrokeCap.Butt;
    }

    public static void DrawSwitch(
        SKCanvas canvas,
        SKRect rect,
        bool isOn,
        UiSwitchStyle style,
        UiSwitchVariant variant = UiSwitchVariant.Sliding,
        Action<SKCanvas, SKRect, UiSwitchStyle, bool, UiSwitchVariant>? customDraw = null)
    {
        if (customDraw is not null)
        {
            customDraw(canvas, rect, style, isOn, variant);
            return;
        }

        if (variant == UiSwitchVariant.ToggleButton)
        {
            DrawButton(canvas, rect, isOn ? "ON" : "OFF",
                new UiButtonStyle(
                    isOn ? style.TrackOnColor : style.TrackOffColor,
                    isOn ? style.TrackOnColor : style.TrackOffColor,
                    style.TextColor,
                    style.BorderColor,
                    style.KnobColor,
                    style.BorderColor,
                    style.CornerRadius,
                    style.BorderWidth,
                    0f,
                    100),
                pressed: false,
                enabled: true,
                fontSize: MathF.Min(18f, rect.Height * 0.5f));
            return;
        }

        _fillPaint.Color = isOn ? style.TrackOnColor : style.TrackOffColor;
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, _fillPaint);

        _strokePaint.StrokeWidth = style.BorderWidth;
        _strokePaint.Color = style.BorderColor;
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, _strokePaint);

        float margin = 4f;
        float knobRadius = MathF.Max(6f, rect.Height * 0.5f - margin);
        float knobX = isOn ? rect.Right - margin - knobRadius : rect.Left + margin + knobRadius;

        _fillPaint.Color = style.KnobColor;
        canvas.DrawCircle(knobX, rect.MidY, knobRadius, _fillPaint);
        _strokePaint.Color = style.BorderColor;
        _strokePaint.StrokeWidth = MathF.Max(1f, style.BorderWidth * 0.75f);
        canvas.DrawCircle(knobX, rect.MidY, knobRadius, _strokePaint);
    }

    public static void DrawSlider(
        SKCanvas canvas,
        SKRect rect,
        float value,
        UiSliderStyle style,
        Action<SKCanvas, SKRect, UiSliderStyle, float>? customDraw = null)
    {
        value = Math.Clamp(value, 0f, 1f);

        if (customDraw is not null)
        {
            customDraw(canvas, rect, style, value);
            return;
        }

        float cy = rect.MidY;
        float left = rect.Left;
        float right = rect.Right;
        float knobX = left + (right - left) * value;

        _strokePaint.StrokeCap = SKStrokeCap.Round;
        _strokePaint.StrokeWidth = style.TrackHeight;
        _strokePaint.Color = style.TrackColor;
        canvas.DrawLine(left, cy, right, cy, _strokePaint);

        _strokePaint.Color = style.FillColor;
        canvas.DrawLine(left, cy, knobX, cy, _strokePaint);

        _fillPaint.Color = style.KnobColor;
        canvas.DrawCircle(knobX, cy, style.KnobRadius, _fillPaint);

        _strokePaint.StrokeCap = SKStrokeCap.Butt;
        _strokePaint.StrokeWidth = style.BorderWidth;
        _strokePaint.Color = style.BorderColor;
        canvas.DrawCircle(knobX, cy, style.KnobRadius, _strokePaint);
    }

    public static float SliderValueFromX(SKRect rect, float x) => Math.Clamp((x - rect.Left) / rect.Width, 0f, 1f);

    public static SKPoint ClampJoystick(SKPoint delta, float maxRadius)
    {
        float length = MathF.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        if (length <= maxRadius || length <= 0.0001f)
            return delta;

        float scale = maxRadius / length;
        return new SKPoint(delta.X * scale, delta.Y * scale);
    }

    public static void DrawJoystick(
        SKCanvas canvas,
        SKPoint center,
        float baseRadius,
        SKPoint knobDelta,
        UiJoystickStyle style,
        Action<SKCanvas, SKPoint, float, SKPoint, UiJoystickStyle>? customDraw = null)
    {
        if (customDraw is not null)
        {
            customDraw(canvas, center, baseRadius, knobDelta, style);
            return;
        }

        _fillPaint.Color = style.BaseColor;
        canvas.DrawCircle(center.X, center.Y, baseRadius, _fillPaint);

        _strokePaint.Color = style.BaseBorderColor;
        _strokePaint.StrokeWidth = style.BorderWidth;
        canvas.DrawCircle(center.X, center.Y, baseRadius, _strokePaint);

        float knobRadius = baseRadius * 0.42f;
        var knobCenter = new SKPoint(center.X + knobDelta.X, center.Y + knobDelta.Y);
        _fillPaint.Color = style.KnobColor;
        canvas.DrawCircle(knobCenter.X, knobCenter.Y, knobRadius, _fillPaint);

        _strokePaint.Color = style.KnobBorderColor;
        canvas.DrawCircle(knobCenter.X, knobCenter.Y, knobRadius, _strokePaint);
    }
}
