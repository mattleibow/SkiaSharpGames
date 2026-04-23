using SkiaSharp;
using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class HudLabelTests
{
    [Fact]
    public void HudLabel_DrawLeft_DoesNotThrow()
    {
        var label = new HudLabel { Text = "Score: 100", FontSize = 20f, Color = SKColors.White };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void HudLabel_DrawCenter_CentresOnX()
    {
        var label = new HudLabel { Text = "TITLE", FontSize = 32f, Align = TextAlign.Center };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void HudLabel_DrawRight_EndsAtX()
    {
        var label = new HudLabel { Text = "Lives: 3", FontSize = 20f, Align = TextAlign.Right };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void HudLabel_EmptyText_SkipsDrawing()
    {
        var label = new HudLabel { Text = "", FontSize = 16f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void HudLabel_NullText_SkipsDrawing()
    {
        var label = new HudLabel { Text = null!, FontSize = 16f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void HudLabel_Invisible_SkipsDrawing()
    {
        var label = new HudLabel { Text = "Hello", FontSize = 16f, Visible = false };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void HudLabel_ZeroAlpha_SkipsDrawing()
    {
        var label = new HudLabel { Text = "Hello", FontSize = 16f, Alpha = 0f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void HudLabel_MeasureWidth_ReturnsPositive()
    {
        var label = new HudLabel { Text = "Hello", FontSize = 20f };
        float w = label.MeasureWidth();
        Assert.True(w > 0f);
    }

    [Fact]
    public void HudLabel_MeasureWidth_EmptyReturnsZero()
    {
        var label = new HudLabel { Text = "", FontSize = 20f };
        Assert.Equal(0f, label.MeasureWidth());
    }

    [Fact]
    public void HudLabel_AlphaClamps()
    {
        var label = new HudLabel { Text = "Hi", FontSize = 16f, Alpha = 2f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void HudLabel_IsActor()
    {
        var label = new HudLabel { Text = "test", X = 10f, Y = 20f };
        Assert.IsAssignableFrom<Actor>(label);
        Assert.Equal(10f, label.WorldX);
    }

    [Fact]
    public void HudLabel_AsChildActor()
    {
        var parent = new Actor { X = 100f, Y = 200f };
        var label = new HudLabel { Text = "child", X = 10f, Y = 20f };
        parent.Children.Add(label);
        Assert.Equal(110f, label.WorldX);
        Assert.Equal(220f, label.WorldY);
    }
}
