using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

// ── Test helpers ──────────────────────────────────────────────────────────

file sealed class PolicyScene : Scene
{
    protected override void OnDraw(SKCanvas c) { }
}

file sealed class PolicyDummyScene : Scene
{
    protected override void OnDraw(SKCanvas c) { }
}

file static class PolicyTestFactory
{
    public static Stage Create()
    {
        var builder = StageBuilder.Create();
        builder.Scenes.Add<PolicyScene>().Add<PolicyDummyScene>();
        builder.SetOpeningScene<PolicyScene>();
        return builder.Open();
    }
}

// ── Tests ─────────────────────────────────────────────────────────────────

public class PointerPolicyTests
{
    private static (Stage stage, Scene scene) Setup()
    {
        var stage = PolicyTestFactory.Create();
        var scene = stage.Services.GetRequiredService<IDirector>().ActiveInputScene;
        return (stage, scene);
    }

    private static void DrawFrame(Stage stage)
    {
        using var bmp = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        stage.Draw(canvas, 800, 600);
    }

    // ── AlwaysVisible ────────────────────────────────────────────────────

    [Fact]
    public void AlwaysVisible_PointerHasFullAlpha()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.AlwaysVisible;
        stage.OnPointerMove(100f, 100f);
        stage.Update(0.016f);
        DrawFrame(stage);
        Assert.Equal(1f, scene.Pointer.Alpha);
    }

    [Fact]
    public void AlwaysVisible_IsDefault()
    {
        var (_, scene) = Setup();
        Assert.Equal(PointerPolicy.AlwaysVisible, scene.PointerPolicy);
    }

    // ── AlwaysHidden ─────────────────────────────────────────────────────

    [Fact]
    public void AlwaysHidden_PointerAlphaIsZero()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.AlwaysHidden;
        stage.OnPointerMove(100f, 100f);
        stage.Update(0.016f);
        DrawFrame(stage);
        Assert.Equal(0f, scene.Pointer.Alpha);
    }

    [Fact]
    public void AlwaysHidden_PointerPositionStillTracked()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.AlwaysHidden;
        stage.OnPointerMove(250f, 300f);
        Assert.Equal(250f, scene.Pointer.X);
        Assert.Equal(300f, scene.Pointer.Y);
    }

    // ── HideWhenIdle ─────────────────────────────────────────────────────

    [Fact]
    public void HideWhenIdle_VisibleDuringActivity()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.HideWhenIdle;
        scene.PointerIdleTimeout = 2f;
        stage.OnPointerMove(100f, 100f);
        stage.Update(0.5f);
        DrawFrame(stage);
        Assert.Equal(1f, scene.Pointer.Alpha);
    }

    [Fact]
    public void HideWhenIdle_FadesOutAfterTimeout()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.HideWhenIdle;
        scene.PointerIdleTimeout = 1f;
        scene.PointerFadeDuration = 0.5f;
        stage.OnPointerMove(100f, 100f);

        // Advance past the timeout + full fade duration
        stage.Update(1.6f);
        DrawFrame(stage);
        Assert.Equal(0f, scene.Pointer.Alpha);
    }

    [Fact]
    public void HideWhenIdle_PartialFade()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.HideWhenIdle;
        scene.PointerIdleTimeout = 1f;
        scene.PointerFadeDuration = 1f;
        stage.OnPointerMove(100f, 100f);

        // Advance to halfway through fade
        stage.Update(1.5f);
        DrawFrame(stage);
        Assert.True(scene.Pointer.Alpha > 0f && scene.Pointer.Alpha < 1f);
    }

    [Fact]
    public void HideWhenIdle_MovementResetsTimer()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.HideWhenIdle;
        scene.PointerIdleTimeout = 1f;
        scene.PointerFadeDuration = 0.5f;
        stage.OnPointerMove(100f, 100f);

        // Almost at timeout
        stage.Update(0.9f);
        DrawFrame(stage);
        Assert.Equal(1f, scene.Pointer.Alpha);

        // Move the pointer — resets timer
        stage.OnPointerMove(200f, 200f);
        stage.Update(0.9f);
        DrawFrame(stage);
        Assert.Equal(1f, scene.Pointer.Alpha);
    }

    [Fact]
    public void HideWhenIdle_SamePositionDoesNotResetTimer()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.HideWhenIdle;
        scene.PointerIdleTimeout = 1f;
        scene.PointerFadeDuration = 0.3f;
        stage.OnPointerMove(100f, 100f);

        stage.Update(0.6f);
        // Same position — should NOT reset timer
        stage.OnPointerMove(100f, 100f);
        stage.Update(0.8f);
        DrawFrame(stage);
        // Total idle = 1.4s, past timeout + fade
        Assert.Equal(0f, scene.Pointer.Alpha);
    }

    // ── Pause / Resume ───────────────────────────────────────────────────

    [Fact]
    public void Pause_ForcesPointerVisible()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.AlwaysHidden;
        stage.OnPointerMove(100f, 100f);
        stage.Pause();
        stage.Update(0.016f);
        DrawFrame(stage);
        Assert.Equal(1f, scene.Pointer.Alpha);
    }

    [Fact]
    public void Pause_StopsUpdates()
    {
        var (stage, _) = Setup();
        var tracker = new UpdateTracker();
        var scene = stage.Services.GetRequiredService<IDirector>().ActiveInputScene;
        stage.Pause();
        // Update should be a no-op
        stage.Update(1f);
        Assert.True(stage.IsStagePaused);
    }

    [Fact]
    public void Resume_RestoresPointerPolicy()
    {
        var (stage, scene) = Setup();
        scene.PointerPolicy = PointerPolicy.AlwaysHidden;
        stage.OnPointerMove(100f, 100f);
        stage.Pause();
        DrawFrame(stage);
        Assert.Equal(1f, scene.Pointer.Alpha);

        stage.Resume();
        stage.Update(0.016f);
        DrawFrame(stage);
        Assert.Equal(0f, scene.Pointer.Alpha);
    }

    [Fact]
    public void IsStagePaused_DefaultFalse()
    {
        var (stage, _) = Setup();
        Assert.False(stage.IsStagePaused);
    }

    [Fact]
    public void Pause_ThenResume_FlipsFlag()
    {
        var (stage, _) = Setup();
        stage.Pause();
        Assert.True(stage.IsStagePaused);
        stage.Resume();
        Assert.False(stage.IsStagePaused);
    }

    // ── Pointer auto-created ─────────────────────────────────────────────

    [Fact]
    public void Scene_HasAutoCreatedPointer()
    {
        var (_, scene) = Setup();
        Assert.NotNull(scene.Pointer);
    }

    [Fact]
    public void Pointer_InitiallyInvisible()
    {
        var (_, scene) = Setup();
        Assert.False(scene.Pointer.Visible);
    }

    [Fact]
    public void Pointer_BecomesVisibleOnPointerEvent()
    {
        var (stage, scene) = Setup();
        stage.OnPointerMove(100f, 200f);
        Assert.True(scene.Pointer.Visible);
    }

    [Fact]
    public void PointerDown_SetsIsDown()
    {
        var (stage, scene) = Setup();
        stage.OnPointerDown(100f, 200f);
        Assert.True(scene.Pointer.IsDown);
    }

    [Fact]
    public void PointerUp_ClearsIsDown()
    {
        var (stage, scene) = Setup();
        stage.OnPointerDown(100f, 200f);
        stage.OnPointerUp(100f, 200f);
        Assert.False(scene.Pointer.IsDown);
    }

    // ── Default property values ──────────────────────────────────────────

    [Fact]
    public void PointerIdleTimeout_DefaultIs2Seconds()
    {
        var (_, scene) = Setup();
        Assert.Equal(2f, scene.PointerIdleTimeout);
    }

    [Fact]
    public void PointerFadeDuration_DefaultIs03Seconds()
    {
        var (_, scene) = Setup();
        Assert.Equal(0.3f, scene.PointerFadeDuration);
    }
}

file sealed class UpdateTracker
{
    public int UpdateCount { get; set; }
}
