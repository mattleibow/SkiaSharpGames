using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

public static class UiControls
{
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

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        using var textPaint = new SKPaint { IsAntialias = true };
        using var font = new SKFont(SKTypeface.Default, fontSize);

        fillPaint.Color = (pressed ? style.PressedFillColor : style.FillColor).WithAlpha(alpha);
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, fillPaint);

        if (style.BevelSize > 0f)
        {
            strokePaint.StrokeWidth = style.BevelSize;
            strokePaint.Color = (pressed ? style.BevelShadowColor : style.BevelLightColor).WithAlpha(alpha);
            var inset = SKRect.Inflate(rect, -style.BevelSize * 0.5f, -style.BevelSize * 0.5f);
            canvas.DrawLine(inset.Left, inset.Top, inset.Right, inset.Top, strokePaint);
            canvas.DrawLine(inset.Left, inset.Top, inset.Left, inset.Bottom, strokePaint);

            strokePaint.Color = (pressed ? style.BevelLightColor : style.BevelShadowColor).WithAlpha(alpha);
            canvas.DrawLine(inset.Left, inset.Bottom, inset.Right, inset.Bottom, strokePaint);
            canvas.DrawLine(inset.Right, inset.Top, inset.Right, inset.Bottom, strokePaint);
        }

        strokePaint.StrokeWidth = style.BorderWidth;
        strokePaint.Color = style.BorderColor.WithAlpha(alpha);
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, strokePaint);

        textPaint.Color = style.TextColor.WithAlpha(alpha);
        float baselineY = rect.MidY + fontSize * 0.35f;
        canvas.DrawText(label, rect.MidX, baselineY, SKTextAlign.Center, font, textPaint);
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

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        fillPaint.Color = style.FillColor;
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, fillPaint);

        strokePaint.StrokeWidth = style.BorderWidth;
        strokePaint.Color = style.BorderColor;
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, strokePaint);

        if (!isChecked)
            return;

        strokePaint.Color = style.CheckColor;
        strokePaint.StrokeCap = SKStrokeCap.Round;
        strokePaint.StrokeWidth = MathF.Max(2f, rect.Width * 0.12f);
        canvas.DrawLine(rect.Left + rect.Width * 0.2f, rect.MidY, rect.Left + rect.Width * 0.45f, rect.Bottom - rect.Height * 0.2f, strokePaint);
        canvas.DrawLine(rect.Left + rect.Width * 0.45f, rect.Bottom - rect.Height * 0.2f, rect.Right - rect.Width * 0.2f, rect.Top + rect.Height * 0.2f, strokePaint);
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

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        fillPaint.Color = isOn ? style.TrackOnColor : style.TrackOffColor;
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, fillPaint);

        strokePaint.StrokeWidth = style.BorderWidth;
        strokePaint.Color = style.BorderColor;
        canvas.DrawRoundRect(rect, style.CornerRadius, style.CornerRadius, strokePaint);

        float margin = 4f;
        float knobRadius = MathF.Max(6f, rect.Height * 0.5f - margin);
        float knobX = isOn ? rect.Right - margin - knobRadius : rect.Left + margin + knobRadius;

        fillPaint.Color = style.KnobColor;
        canvas.DrawCircle(knobX, rect.MidY, knobRadius, fillPaint);
        strokePaint.Color = style.BorderColor;
        strokePaint.StrokeWidth = MathF.Max(1f, style.BorderWidth * 0.75f);
        canvas.DrawCircle(knobX, rect.MidY, knobRadius, strokePaint);
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

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        strokePaint.StrokeCap = SKStrokeCap.Round;
        strokePaint.StrokeWidth = style.TrackHeight;
        strokePaint.Color = style.TrackColor;
        canvas.DrawLine(left, cy, right, cy, strokePaint);

        strokePaint.Color = style.FillColor;
        canvas.DrawLine(left, cy, knobX, cy, strokePaint);

        fillPaint.Color = style.KnobColor;
        canvas.DrawCircle(knobX, cy, style.KnobRadius, fillPaint);

        strokePaint.StrokeCap = SKStrokeCap.Butt;
        strokePaint.StrokeWidth = style.BorderWidth;
        strokePaint.Color = style.BorderColor;
        canvas.DrawCircle(knobX, cy, style.KnobRadius, strokePaint);
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

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        fillPaint.Color = style.BaseColor;
        canvas.DrawCircle(center.X, center.Y, baseRadius, fillPaint);

        strokePaint.Color = style.BaseBorderColor;
        strokePaint.StrokeWidth = style.BorderWidth;
        canvas.DrawCircle(center.X, center.Y, baseRadius, strokePaint);

        float knobRadius = baseRadius * 0.42f;
        var knobCenter = new SKPoint(center.X + knobDelta.X, center.Y + knobDelta.Y);
        fillPaint.Color = style.KnobColor;
        canvas.DrawCircle(knobCenter.X, knobCenter.Y, knobRadius, fillPaint);

        strokePaint.Color = style.KnobBorderColor;
        canvas.DrawCircle(knobCenter.X, knobCenter.Y, knobRadius, strokePaint);
    }
}
