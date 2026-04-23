using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharp.Theatre;
using SkiaSharp.Theatre.Rehearsals;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

// ── Test scenes for harness tests ────────────────────────────────────────

file sealed class HarnessState
{
    public int UpdateCount { get; set; }
    public (float x, float y)? LastClick { get; set; }
    public string? LastKey { get; set; }
    public bool GameStarted { get; set; }
}

file sealed class HarnessStartScreen(HarnessState state, IDirector director) : Scene
{
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(SKColors.Black);
        // Draw a red rectangle in the center as a "Start" button
        using var paint = new SKPaint { Color = SKColors.Red };
        canvas.DrawRect(SKRect.Create(350, 250, 100, 50), paint);
    }

    public override void OnPointerDown(float x, float y)
    {
        state.LastClick = (x, y);
        // If clicked in the "button" area, start the game
        if (x >= 350 && x <= 450 && y >= 250 && y <= 300)
        {
            state.GameStarted = true;
            director.TransitionTo<HarnessPlayScreen>();
        }
    }
}

file sealed class HarnessPlayScreen(HarnessState state) : Scene
{
    private readonly Actor _ball = new()
    {
        X = 400f, Y = 300f,
        Collider = new CircleCollider { Radius = 10f },
        Rigidbody = new Rigidbody2D { VelocityX = 100f, VelocityY = 50f }
    };

    public override void Update(float deltaTime)
    {
        state.UpdateCount++;
        _ball.Update(deltaTime);
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(new SKColor(0x1A, 0x1A, 0x2E));

        // Draw ball
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawCircle(_ball.X, _ball.Y, 10f, paint);

        // Draw score label
        var label = new HudLabel { Text = $"Frame: {state.UpdateCount}", FontSize = 20f, Color = SKColors.White };
        label.X = 10f; label.Y = 25f;
        label.Draw(canvas);
    }

    public override void OnKeyDown(string key)
    {
        state.LastKey = key;
    }

    public Actor Ball => _ball;
}

// ── Harness tests ─────────────────────────────────────────────────────────

public class RehearsalTests
{
    private static Rehearsal CreateHarness()
    {
        return Rehearsal.Create(builder =>
        {
            builder.Services.AddSingleton<HarnessState>();
            builder.Scenes
                .Add<HarnessStartScreen>()
                .Add<HarnessPlayScreen>();
            builder.SetOpeningScene<HarnessStartScreen>();
        });
    }

    [Fact]
    public void Harness_Creates_WithDefaults()
    {
        using var h = CreateHarness();
        Assert.Equal(800, h.Width);
        Assert.Equal(600, h.Height);
        Assert.Equal(0, h.FrameNumber);
        Assert.Equal(0f, h.ElapsedTime);
    }

    [Fact]
    public void RunFrame_AdvancesFrameCounter()
    {
        using var h = CreateHarness();
        h.RunFrame();
        Assert.Equal(1, h.FrameNumber);
        Assert.True(h.ElapsedTime > 0f);
    }

    [Fact]
    public void RunFrames_AdvancesMultiple()
    {
        using var h = CreateHarness();
        h.RunFrames(60);
        Assert.Equal(60, h.FrameNumber);
        Assert.InRange(h.ElapsedTime, 0.99f, 1.01f); // ~1 second at 60fps
    }

    [Fact]
    public void RunFor_SimulatesDuration()
    {
        using var h = CreateHarness();
        h.RunFor(2.0f);
        Assert.InRange(h.ElapsedTime, 1.99f, 2.01f);
    }

    // ── Rendering ─────────────────────────────────────────────────────

    [Fact]
    public void StartScreen_DrawsRedButton()
    {
        using var h = CreateHarness();
        h.RunFrame();

        using var frame = h.CaptureFrame();
        // The start scene draws a red rect at (350,250)-(450,300)
        var buttonRegion = new SKRectI(360, 260, 440, 290);
        Assert.True(frame.HasNonBackgroundPixel(buttonRegion, SKColors.Black));

        // Count red pixels in the button area
        int redPixels = frame.CountPixels(buttonRegion, SKColors.Red);
        Assert.True(redPixels > 100, $"Expected red pixels in button, found {redPixels}");
    }

    [Fact]
    public void CaptureFrame_IsIndependentCopy()
    {
        using var h = CreateHarness();
        h.RunFrame();

        using var frame1 = h.CaptureFrame();
        h.RunFrames(10); // more frames change the internal bitmap
        using var frame2 = h.CaptureFrame();

        // frame1 should still reflect the original state
        Assert.Equal(1, frame1.FrameNumber);
        Assert.Equal(11, frame2.FrameNumber);
    }

    // ── Input simulation ──────────────────────────────────────────────

    [Fact]
    public void Click_TransitionsScreen()
    {
        using var h = CreateHarness();
        h.RunFrame();

        var state = h.Stage.Services.GetRequiredService<HarnessState>();
        Assert.False(state.GameStarted);

        // Click the "Start" button at center of red rect
        h.Click(400, 275);
        Assert.True(state.GameStarted);

        // Advance some frames on the play scene
        h.RunFrames(30);
        Assert.True(state.UpdateCount > 0);
    }

    [Fact]
    public void KeyTap_ForwardedToScreen()
    {
        using var h = CreateHarness();
        h.RunFrame();

        // Transition to play scene
        h.Click(400, 275);
        h.RunFrame();

        var state = h.Stage.Services.GetRequiredService<HarnessState>();
        h.KeyTap("Space");
        Assert.Equal("Space", state.LastKey);
    }

    // ── Frame comparison ──────────────────────────────────────────────

    [Fact]
    public void DiffRatio_IdenticalFrames_ReturnsZero()
    {
        using var h = CreateHarness();
        h.RunFrame();

        using var frame1 = h.CaptureFrame();
        using var frame2 = h.CaptureFrame();
        Assert.Equal(0f, frame1.DiffRatio(frame2));
    }

    [Fact]
    public void DiffRatio_DifferentScreens_ReturnsNonZero()
    {
        using var h = CreateHarness();
        h.RunFrame();
        using var startFrame = h.CaptureFrame();

        h.Click(400, 275); // transition to play scene
        h.RunFrames(5);
        using var playFrame = h.CaptureFrame();

        float diff = startFrame.DiffRatio(playFrame);
        Assert.True(diff > 0.01f, $"Expected visible difference, got {diff:P2}");
    }

    // ── Actor dump ───────────────────────────────────────────────────

    [Fact]
    public void Actor_Dump_ShowsHierarchy()
    {
        var parent = new Actor { X = 100f, Y = 200f };
        var child = new Actor { X = 10f, Y = 20f, Alpha = 0.5f };
        child.Collider = new CircleCollider { Radius = 5f };
        child.Rigidbody = new Rigidbody2D { VelocityX = 50f };
        parent.AddChild(child);

        string dump = parent.Dump();

        Assert.Contains("Actor @ (100.0, 200.0)", dump);
        Assert.Contains("Actor @ (10.0, 20.0)", dump);
        Assert.Contains("world=(110.0, 220.0)", dump);
        Assert.Contains("a=0.50", dump);
        Assert.Contains("collider: Circle r=5", dump);
        Assert.Contains("velocity: (50.0, 0.0)", dump);
    }

    [Fact]
    public void Actor_Dump_ShowsInactiveAndHidden()
    {
        var actor = new Actor { Active = false, Visible = false };
        string dump = actor.Dump();
        Assert.Contains("INACTIVE", dump);
        Assert.Contains("HIDDEN", dump);
    }

    [Fact]
    public void Actor_Dump_ShowsConcreteTypeName()
    {
        var button = new HudButton(100f, 40f);
        string dump = button.Dump();
        Assert.Contains("HudButton", dump);
        Assert.Contains("collider: Rect 100x40", dump);
    }

    // ── Pixel assertions ──────────────────────────────────────────────

    [Fact]
    public void FrameSnapshot_IsRegionSolidColor()
    {
        using var h = CreateHarness();
        h.RunFrame(); // Start scene: black background with red button

        using var frame = h.CaptureFrame();
        // Top-left corner should be solid black (no UI there)
        Assert.True(frame.IsRegionSolidColor(new SKRectI(0, 0, 10, 10), SKColors.Black));
    }
}
