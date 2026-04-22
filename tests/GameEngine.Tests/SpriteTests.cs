using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class UiLabelTests
{
    [Fact]
    public void UiLabel_DrawLeft_DoesNotThrow()
    {
        var label = new UiLabel { Text = "Score: 100", FontSize = 20f, Color = SKColors.White };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void UiLabel_DrawCenter_CentresOnX()
    {
        var label = new UiLabel { Text = "TITLE", FontSize = 32f, Align = TextAlign.Center };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void UiLabel_DrawRight_EndsAtX()
    {
        var label = new UiLabel { Text = "Lives: 3", FontSize = 20f, Align = TextAlign.Right };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void UiLabel_EmptyText_SkipsDrawing()
    {
        var label = new UiLabel { Text = "", FontSize = 16f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void UiLabel_NullText_SkipsDrawing()
    {
        var label = new UiLabel { Text = null!, FontSize = 16f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void UiLabel_Invisible_SkipsDrawing()
    {
        var label = new UiLabel { Text = "Hello", FontSize = 16f, Visible = false };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void UiLabel_ZeroAlpha_SkipsDrawing()
    {
        var label = new UiLabel { Text = "Hello", FontSize = 16f, Alpha = 0f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void UiLabel_MeasureWidth_ReturnsPositive()
    {
        var label = new UiLabel { Text = "Hello", FontSize = 20f };
        float w = label.MeasureWidth();
        Assert.True(w > 0f);
    }

    [Fact]
    public void UiLabel_MeasureWidth_EmptyReturnsZero()
    {
        var label = new UiLabel { Text = "", FontSize = 20f };
        Assert.Equal(0f, label.MeasureWidth());
    }

    [Fact]
    public void UiLabel_AlphaClamps()
    {
        var label = new UiLabel { Text = "Hi", FontSize = 16f, Alpha = 2f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => label.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void UiLabel_IsEntity()
    {
        var label = new UiLabel { Text = "test", X = 10f, Y = 20f };
        Assert.IsAssignableFrom<Actor>(label);
        Assert.Equal(10f, label.WorldX);
    }

    [Fact]
    public void UiLabel_AsChildEntity()
    {
        var parent = new Actor { X = 100f, Y = 200f };
        var label = new UiLabel { Text = "child", X = 10f, Y = 20f };
        parent.AddChild(label);
        Assert.Equal(110f, label.WorldX);
        Assert.Equal(220f, label.WorldY);
    }
}
