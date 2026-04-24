using SkiaSharp;
using SkiaSharp.Theatre;

using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class SlideCurtainTests
{
    private static SKCanvas MakeCanvas() => new(new SKBitmap(800, 600));

    [Theory]
    [InlineData(SlideDirection.Up)]
    [InlineData(SlideDirection.Down)]
    [InlineData(SlideDirection.Left)]
    [InlineData(SlideDirection.Right)]
    public void Draw_AllDirections_DoNotThrow(SlideDirection dir)
    {
        var t = new SlideCurtain { Direction = dir };
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), 0.5f, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_CallsBothScreenCallbacks()
    {
        bool outCalled = false,
            inCalled = false;
        var t = new SlideCurtain();
        t.Draw(MakeCanvas(), 0.5f, _ => outCalled = true, _ => inCalled = true, 800, 600);
        Assert.True(outCalled);
        Assert.True(inCalled);
    }
}