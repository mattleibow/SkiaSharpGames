using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class DrawHelperTests
{
    private static SKCanvas MakeCanvas() =>
        new SKCanvas(new SKBitmap(800, 600));

    // ── FillRect ───────────────────────────────────────────────────────────

    [Fact]
    public void FillRect_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => DrawHelper.FillRect(canvas, 10f, 20f, 100f, 50f, SKColors.Red));
        Assert.Null(ex);
    }

    [Fact]
    public void FillRect_WithAlpha_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => DrawHelper.FillRect(canvas, 0f, 0f, 50f, 50f, SKColors.Blue, 0.5f));
        Assert.Null(ex);
    }

    [Fact]
    public void FillRect_AlphaClamped_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => DrawHelper.FillRect(canvas, 0f, 0f, 50f, 50f, SKColors.Blue, 2f));
        Assert.Null(ex);
    }

    // ── DrawOverlay ────────────────────────────────────────────────────────

    [Fact]
    public void DrawOverlay_DefaultAlpha_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => DrawHelper.DrawOverlay(canvas, 800f, 600f));
        Assert.Null(ex);
    }

    [Fact]
    public void DrawOverlay_WithCustomColor_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => DrawHelper.DrawOverlay(canvas, 800f, 600f, 0.5f, SKColors.DarkBlue));
        Assert.Null(ex);
    }

    [Fact]
    public void DrawOverlay_ZeroAlpha_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => DrawHelper.DrawOverlay(canvas, 800f, 600f, 0f));
        Assert.Null(ex);
    }

    // ── DrawCenteredText ───────────────────────────────────────────────────

    [Fact]
    public void DrawCenteredText_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() =>
            DrawHelper.DrawCenteredText(canvas, "Hello", 20f, SKColors.White, 400f, 300f));
        Assert.Null(ex);
    }

    [Fact]
    public void DrawCenteredText_WithAlpha_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() =>
            DrawHelper.DrawCenteredText(canvas, "Fade", 16f, SKColors.Yellow, 200f, 100f, 0.5f));
        Assert.Null(ex);
    }

    // ── DrawText ───────────────────────────────────────────────────────────

    [Fact]
    public void DrawText_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() =>
            DrawHelper.DrawText(canvas, "Score: 0", 18f, SKColors.White, 20f, 30f));
        Assert.Null(ex);
    }

    [Fact]
    public void DrawText_WithAlpha_DoesNotThrow()
    {
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() =>
            DrawHelper.DrawText(canvas, "Lives: 3", 18f, SKColors.White, 20f, 30f, 0.7f));
        Assert.Null(ex);
    }

    // ── MeasureText ────────────────────────────────────────────────────────

    [Fact]
    public void MeasureText_ReturnsPositiveValue()
    {
        float w = DrawHelper.MeasureText("Hello", 20f);
        Assert.True(w > 0f);
    }

    [Fact]
    public void MeasureText_EmptyString_ReturnsZeroOrClose()
    {
        float w = DrawHelper.MeasureText("", 20f);
        Assert.True(w >= 0f);
    }

    [Fact]
    public void MeasureText_LongerText_IsWider()
    {
        float w1 = DrawHelper.MeasureText("Hi", 20f);
        float w2 = DrawHelper.MeasureText("Hello World", 20f);
        Assert.True(w2 > w1);
    }
}
