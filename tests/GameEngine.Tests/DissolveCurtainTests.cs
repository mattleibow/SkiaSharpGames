using SkiaSharp;
using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class DissolveCurtainTests
{
    private static SKCanvas MakeCanvas() => new(new SKBitmap(800, 600));

    [Fact]
    public void Draw_AtProgress0_DoesNotThrow()
    {
        var t = new DissolveCurtain();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), 0f, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_AtProgress1_DoesNotThrow()
    {
        var t = new DissolveCurtain();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), 1f, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_AtProgress05_DoesNotThrow()
    {
        var t = new DissolveCurtain();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), 0.5f, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultDuration_Is04()
        => Assert.Equal(0.4f, new DissolveCurtain().Duration);
}
