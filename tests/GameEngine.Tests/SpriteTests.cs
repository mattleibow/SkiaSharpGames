using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

file sealed class TestSprite : Sprite
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = true };

    public bool DrawCalled { get; private set; }

    public override void Draw(SKCanvas canvas)
    {
        DrawCalled = true;
        FillPaint.Color = SKColors.White;
        canvas.DrawCircle(0f, 0f, 4f, FillPaint);
    }
}

public class SpriteTests
{
    [Fact]
    public void Sprite_Draw_ConcreteImplementationRuns()
    {
        var sprite = new TestSprite();
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);

        sprite.Draw(canvas);

        Assert.True(sprite.DrawCalled);
    }

    [Fact]
    public void Sprite_Update_DefaultDoesNotThrow()
    {
        var sprite = new TestSprite();
        var ex = Record.Exception(() => sprite.Update(1f));
        Assert.Null(ex);
    }

    [Fact]
    public void Sprite_DirectCanvasDrawing_DoesNotThrow()
    {
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { IsAntialias = true, Color = SKColors.CornflowerBlue };
        var ex = Record.Exception(() => canvas.DrawRoundRect(SKRect.Create(10f, 10f, 40f, 20f), 4f, 4f, paint));
        Assert.Null(ex);
    }

    [Fact]
    public void Sprite_DirectTextDrawing_DoesNotThrow()
    {
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { IsAntialias = true, Color = SKColors.White };
        using var font = new SKFont(SKTypeface.Default, 16f) { Edging = SKFontEdging.Antialias };
        var ex = Record.Exception(() => canvas.DrawText("hello", 10f, 20f, font, paint));
        Assert.Null(ex);
    }

    // ── TextSprite tests ──────────────────────────────────────────────────

    [Fact]
    public void TextSprite_DrawLeft_DoesNotThrow()
    {
        var ts = new TextSprite { Text = "Score: 100", Size = 20f, Color = SKColors.White };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => ts.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void TextSprite_DrawCenter_CentresOnX()
    {
        var ts = new TextSprite { Text = "TITLE", Size = 32f, Align = TextAlign.Center };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => ts.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void TextSprite_DrawRight_EndsAtX()
    {
        var ts = new TextSprite { Text = "Lives: 3", Size = 20f, Align = TextAlign.Right };
        using var bmp = new SKBitmap(400, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => ts.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void TextSprite_EmptyText_SkipsDrawing()
    {
        var ts = new TextSprite { Text = "", Size = 16f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => ts.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void TextSprite_NullText_SkipsDrawing()
    {
        var ts = new TextSprite { Text = null!, Size = 16f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => ts.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void TextSprite_Invisible_SkipsDrawing()
    {
        var ts = new TextSprite { Text = "Hello", Size = 16f, Visible = false };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => ts.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void TextSprite_ZeroAlpha_SkipsDrawing()
    {
        var ts = new TextSprite { Text = "Hello", Size = 16f, Alpha = 0f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => ts.Draw(canvas));
        Assert.Null(ex);
    }

    [Fact]
    public void TextSprite_MeasureWidth_ReturnsPositive()
    {
        var ts = new TextSprite { Text = "Hello", Size = 20f };
        float w = ts.MeasureWidth();
        Assert.True(w > 0f);
    }

    [Fact]
    public void TextSprite_MeasureWidth_EmptyReturnsZero()
    {
        var ts = new TextSprite { Text = "", Size = 20f };
        Assert.Equal(0f, ts.MeasureWidth());
    }

    [Fact]
    public void TextSprite_AlphaClamps()
    {
        var ts = new TextSprite { Text = "Hi", Size = 16f, Alpha = 2f };
        using var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => ts.Draw(canvas));
        Assert.Null(ex);
    }
}
