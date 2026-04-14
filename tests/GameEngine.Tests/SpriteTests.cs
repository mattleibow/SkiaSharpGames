using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class SpriteTests
{
    // ── RectSprite ─────────────────────────────────────────────────────────

    [Fact]
    public void RectSprite_Draw_Visible_DoesNotThrow()
    {
        var s = new RectSprite { Width = 50f, Height = 20f, Color = SKColors.Red };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 10f, 10f));
        Assert.Null(ex);
    }

    [Fact]
    public void RectSprite_Draw_WhenInvisible_DoesNotThrow()
    {
        var s = new RectSprite { Visible = false, Width = 50f, Height = 20f, Color = SKColors.Red };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 0f, 0f));
        Assert.Null(ex);
    }

    [Fact]
    public void RectSprite_Draw_WhenAlphaZero_DoesNotThrow()
    {
        var s = new RectSprite { Alpha = 0f, Width = 50f, Height = 20f, Color = SKColors.Red };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 0f, 0f));
        Assert.Null(ex);
    }

    [Fact]
    public void RectSprite_Draw_WithShowShineTrue_DoesNotThrow()
    {
        var s = new RectSprite { Width = 50f, Height = 20f, Color = SKColors.Blue, ShowShine = true };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 100f, 100f));
        Assert.Null(ex);
    }

    [Fact]
    public void RectSprite_Draw_WithShowShineFalse_DoesNotThrow()
    {
        var s = new RectSprite { Width = 50f, Height = 20f, Color = SKColors.Blue, ShowShine = false };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 100f, 100f));
        Assert.Null(ex);
    }

    [Fact]
    public void RectSprite_Draw_SmallSize_ShowShineSkipped_DoesNotThrow()
    {
        // Width/Height <= 4 → shine is skipped
        var s = new RectSprite { Width = 3f, Height = 3f, Color = SKColors.Blue, ShowShine = true };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 100f, 100f));
        Assert.Null(ex);
    }

    [Fact]
    public void RectSprite_Update_AdvancesShimmer()
    {
        var s = new RectSprite { Width = 50f, Height = 20f, Color = SKColors.Blue };
        s.Shimmer.Start(initialDelay: s.Shimmer.Period); // fires immediately next period

        // advance until the shimmer becomes active
        for (int i = 0; i < 100; i++)
        {
            s.Update(0.1f);
            if (s.Shimmer.IsActive) break;
        }

        Assert.True(s.Shimmer.IsActive, "Shimmer should have become active after enough ticks");
    }

    [Fact]
    public void RectSprite_Draw_WithActiveShimmer_DoesNotThrow()
    {
        var s = new RectSprite { Width = 50f, Height = 20f, Color = SKColors.Blue };
        s.Shimmer.Start(initialDelay: s.Shimmer.Period);

        // advance until shimmer fires
        for (int i = 0; i < 100; i++)
        {
            s.Update(0.1f);
            if (s.Shimmer.IsActive) break;
        }

        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 100f, 100f));
        Assert.Null(ex);
    }

    // ── CircleSprite ───────────────────────────────────────────────────────

    [Fact]
    public void CircleSprite_Draw_Visible_DoesNotThrow()
    {
        var s = new CircleSprite { Radius = 10f, Color = SKColors.White };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 50f, 50f));
        Assert.Null(ex);
    }

    [Fact]
    public void CircleSprite_Draw_WhenInvisible_DoesNotThrow()
    {
        var s = new CircleSprite { Visible = false, Radius = 10f, Color = SKColors.White };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 0f, 0f));
        Assert.Null(ex);
    }

    [Fact]
    public void CircleSprite_Draw_WhenAlphaZero_DoesNotThrow()
    {
        var s = new CircleSprite { Alpha = 0f, Radius = 10f, Color = SKColors.White };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 0f, 0f));
        Assert.Null(ex);
    }

    [Fact]
    public void CircleSprite_Draw_WithGlow_DoesNotThrow()
    {
        var s = new CircleSprite { Radius = 10f, Color = SKColors.Orange, GlowRadius = 5f, GlowColor = SKColors.Orange };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 50f, 50f));
        Assert.Null(ex);
    }

    [Fact]
    public void CircleSprite_Draw_WithNoGlow_DoesNotThrow()
    {
        var s = new CircleSprite { Radius = 10f, Color = SKColors.Orange, GlowRadius = 0f };
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => s.Draw(canvas, 50f, 50f));
        Assert.Null(ex);
    }

    // ── Sprite.Update default ─────────────────────────────────────────────

    [Fact]
    public void CircleSprite_Update_DoesNotThrow()
    {
        // CircleSprite inherits the default Update (no-op) from Sprite
        var s = new CircleSprite { Radius = 5f, Color = SKColors.White };
        var ex = Record.Exception(() => s.Update(1f));
        Assert.Null(ex);
    }
}
