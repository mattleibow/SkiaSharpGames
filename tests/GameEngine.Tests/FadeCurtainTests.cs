using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class FadeCurtainTests
{
    private static SKCanvas MakeCanvas() => new(new SKBitmap(800, 600));

    [Theory]
    [InlineData(0f)]
    [InlineData(0.25f)]
    [InlineData(0.5f)]
    [InlineData(0.75f)]
    [InlineData(1f)]
    public void Draw_DoesNotThrow_AtVariousProgress(float progress)
    {
        var t = new FadeCurtain();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), progress, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_FirstHalf_CallsOutgoingCallback()
    {
        bool outCalled = false;
        var t = new FadeCurtain();
        t.Draw(MakeCanvas(), 0.2f, _ => outCalled = true, _ => { }, 800, 600);
        Assert.True(outCalled);
    }

    [Fact]
    public void Draw_SecondHalf_CallsIncomingCallback()
    {
        bool inCalled = false;
        var t = new FadeCurtain();
        t.Draw(MakeCanvas(), 0.8f, _ => { }, _ => inCalled = true, 800, 600);
        Assert.True(inCalled);
    }
}
