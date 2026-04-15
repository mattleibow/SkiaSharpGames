using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

file sealed class TestSprite : Sprite
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = true };

    public bool DrawCalled { get; private set; }

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        DrawCalled = true;
        FillPaint.Color = SKColors.White;
        canvas.DrawCircle(x, y, 4f, FillPaint);
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

        sprite.Draw(canvas, 20f, 20f);

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
}
