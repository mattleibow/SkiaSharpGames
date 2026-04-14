using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

/// <summary>Tests covering GameBounds, RectCollider.BoundingBox, GameScreen virtual defaults,
/// Game input forwarding, and ScreenCoordinator.ActiveInputScreen during a transition.</summary>
public class MiscCoverageTests
{
    private sealed class TestEntity : Entity { }

    // ── GameBounds ─────────────────────────────────────────────────────────

    [Fact]
    public void GameBounds_Width_ReturnsRightMinusLeft()
    {
        var b = new GameBounds(10f, 20f, 110f, 220f);
        Assert.Equal(100f, b.Width, precision: 4);
    }

    [Fact]
    public void GameBounds_Height_ReturnsBottomMinusTop()
    {
        var b = new GameBounds(10f, 20f, 110f, 220f);
        Assert.Equal(200f, b.Height, precision: 4);
    }

    [Fact]
    public void GameBounds_FromSize_LeftTopAreZero()
    {
        var b = GameBounds.FromSize(800f, 600f);
        Assert.Equal(0f, b.Left, precision: 4);
        Assert.Equal(0f, b.Top, precision: 4);
        Assert.Equal(800f, b.Right, precision: 4);
        Assert.Equal(600f, b.Bottom, precision: 4);
    }

    // ── RectCollider.BoundingBox ──────────────────────────────────────────

    [Fact]
    public void RectCollider_BoundingBox_MatchesWorldRect()
    {
        var entity = new TestEntity { X = 100f, Y = 50f };
        var collider = new RectCollider { Width = 40f, Height = 20f };

        var worldRect = collider.WorldRect(entity);
        var boundingBox = collider.BoundingBox(entity);

        Assert.Equal(worldRect.Left,   boundingBox.Left,   precision: 4);
        Assert.Equal(worldRect.Top,    boundingBox.Top,    precision: 4);
        Assert.Equal(worldRect.Right,  boundingBox.Right,  precision: 4);
        Assert.Equal(worldRect.Bottom, boundingBox.Bottom, precision: 4);
    }

    [Fact]
    public void RectCollider_BoundingBox_WithOffset_IsShifted()
    {
        var entity = new TestEntity { X = 100f, Y = 100f };
        var collider = new RectCollider { Width = 20f, Height = 10f, OffsetX = 5f, OffsetY = -5f };

        var box = collider.BoundingBox(entity);

        // left  = owner.X + OffsetX - Width/2  = 100 + 5 - 10 = 95
        // top   = owner.Y + OffsetY - Height/2 = 100 - 5 - 5  = 90
        // right = left + Width                 = 95 + 20       = 115
        // bottom= top  + Height                = 90 + 10       = 100
        Assert.Equal(95f,  box.Left,   precision: 4);
        Assert.Equal(90f,  box.Top,    precision: 4);
        Assert.Equal(115f, box.Right,  precision: 4);
        Assert.Equal(100f, box.Bottom, precision: 4);
    }

    // ── GameScreen default virtual methods ────────────────────────────────

    private sealed class DefaultGameScreen : GameScreen
    {
        public override void Draw(SKCanvas c, int w, int h) { }
    }

    [Fact]
    public void GameScreen_DefaultUpdate_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.Update(0.016f));
        Assert.Null(ex);
    }

    [Fact]
    public void GameScreen_DefaultOnPointerMove_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnPointerMove(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void GameScreen_DefaultOnPointerDown_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnPointerDown(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void GameScreen_DefaultOnPointerUp_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnPointerUp(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void GameScreen_DefaultOnKeyDown_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnKeyDown("ArrowLeft"));
        Assert.Null(ex);
    }

    [Fact]
    public void GameScreen_DefaultOnKeyUp_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnKeyUp("ArrowLeft"));
        Assert.Null(ex);
    }

    [Fact]
    public void GameScreen_DefaultOnPaused_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnPaused());
        Assert.Null(ex);
    }

    [Fact]
    public void GameScreen_DefaultOnResumed_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnResumed());
        Assert.Null(ex);
    }

    // ── Game input forwarding ─────────────────────────────────────────────

    private static (Game game, InputTracker tracker) BuildTestGame()
    {
        var tracker = new InputTracker();
        var builder = GameBuilder.CreateDefault();
        builder.Services.AddSingleton(tracker);
        builder.Screens.Add<InputCapturingScreen>();
        builder.SetInitialScreen<InputCapturingScreen>();
        return (builder.Build(), tracker);
    }

    [Fact]
    public void Game_OnPointerMove_ForwardedToActiveScreen()
    {
        var (game, tracker) = BuildTestGame();
        game.OnPointerMove(100f, 200f);
        Assert.Equal((100f, 200f), tracker.LastPointerMove);
    }

    [Fact]
    public void Game_OnPointerDown_ForwardedToActiveScreen()
    {
        var (game, tracker) = BuildTestGame();
        game.OnPointerDown(50f, 60f);
        Assert.Equal((50f, 60f), tracker.LastPointerDown);
    }

    [Fact]
    public void Game_OnPointerUp_ForwardedToActiveScreen()
    {
        var (game, tracker) = BuildTestGame();
        game.OnPointerUp(70f, 80f);
        Assert.Equal((70f, 80f), tracker.LastPointerUp);
    }

    [Fact]
    public void Game_OnKeyDown_ForwardedToActiveScreen()
    {
        var (game, tracker) = BuildTestGame();
        game.OnKeyDown("Space");
        Assert.Equal("Space", tracker.LastKeyDown);
    }

    [Fact]
    public void Game_OnKeyUp_ForwardedToActiveScreen()
    {
        var (game, tracker) = BuildTestGame();
        game.OnKeyUp("Enter");
        Assert.Equal("Enter", tracker.LastKeyUp);
    }

    // ── ScreenCoordinator.ActiveInputScreen during transition ─────────────

    [Fact]
    public void ActiveInputScreen_DuringTransition_DoesNotThrow()
    {
        // Build a game with two screens
        var tracker = new InputTracker();
        var builder = GameBuilder.CreateDefault();
        builder.Services.AddSingleton(tracker);
        builder.Screens
               .Add<InputCapturingScreen>()
               .Add<SecondScreen>();
        builder.SetInitialScreen<InputCapturingScreen>();
        var game = builder.Build();

        var coordinator = game.Services.GetRequiredService<IScreenCoordinator>();

        // Start a dissolve transition to the second screen
        coordinator.TransitionTo<SecondScreen>(new DissolveTransition { Duration = 1f });

        // During transition, ActiveInputScreen is the incoming screen (SecondScreen).
        // Forwarded input must not throw.
        var ex = Record.Exception(() => game.OnPointerMove(0f, 0f));
        Assert.Null(ex);
    }

    // ── Collider2D offset coverage ────────────────────────────────────────

    [Fact]
    public void CircleCollider_WithOffset_WorldCenterIsShifted()
    {
        var entity = new TestEntity { X = 100f, Y = 100f };
        var collider = new CircleCollider { Radius = 5f, OffsetX = 10f, OffsetY = -5f };

        var (cx, cy) = collider.WorldCenter(entity);

        Assert.Equal(110f, cx, precision: 4);
        Assert.Equal(95f,  cy, precision: 4);
    }

    // ── Rigidbody2D BounceX/Y with restitution ────────────────────────────

    [Fact]
    public void Rigidbody2D_BounceX_WithRestitution_ScalesVelocity()
    {
        var body = new Rigidbody2D { VelocityX = 100f };
        body.BounceX(0.5f); // restitution = 0.5 → new speed = 50
        Assert.Equal(-50f, body.VelocityX, precision: 3);
    }

    [Fact]
    public void Rigidbody2D_BounceY_WithRestitution_ScalesVelocity()
    {
        var body = new Rigidbody2D { VelocityY = 200f };
        body.BounceY(0.8f);
        Assert.Equal(-160f, body.VelocityY, precision: 3);
    }

    [Fact]
    public void Rigidbody2D_BounceX_WithZeroRestitution_StopsVelocity()
    {
        var body = new Rigidbody2D { VelocityX = 100f };
        body.BounceX(0f); // restitution=0 → new VelocityX = -100 * max(0,0) = 0
        Assert.Equal(0f, body.VelocityX, precision: 3);
    }

    [Fact]
    public void Rigidbody2D_Bounce_WithAngleDiagonal_ReflectsCorrectly()
    {
        // Velocity going down-right (45°), normal pointing up-right (45°)
        var body = new Rigidbody2D { VelocityX = 1f, VelocityY = 1f };
        body.Bounce(0f, -1f); // horizontal surface, normal pointing up
        Assert.Equal(1f,  body.VelocityX, precision: 3);
        Assert.Equal(-1f, body.VelocityY, precision: 3);
    }

    [Fact]
    public void CollisionHit_IsHorizontal_ReturnsTrueWhenNormalXDominates()
    {
        var hit = new CollisionHit(1f, 0.5f, 5f);
        Assert.True(hit.IsHorizontal);
        Assert.False(hit.IsVertical);
    }

    [Fact]
    public void CollisionHit_IsVertical_ReturnsTrueWhenNormalYDominatesOrEqual()
    {
        var hitEqual = new CollisionHit(0.5f, 0.5f, 5f);
        Assert.True(hitEqual.IsVertical); // equal → IsVertical is true

        var hitY = new CollisionHit(0f, 1f, 3f);
        Assert.True(hitY.IsVertical);
        Assert.False(hitY.IsHorizontal);
    }
}

// ── Helper types ──────────────────────────────────────────────────────────

internal sealed class InputTracker
{
    public (float x, float y)? LastPointerMove { get; set; }
    public (float x, float y)? LastPointerDown { get; set; }
    public (float x, float y)? LastPointerUp   { get; set; }
    public string? LastKeyDown { get; set; }
    public string? LastKeyUp   { get; set; }
}

internal sealed class InputCapturingScreen(InputTracker tracker) : GameScreen
{
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnPointerMove(float x, float y) => tracker.LastPointerMove = (x, y);
    public override void OnPointerDown(float x, float y) => tracker.LastPointerDown = (x, y);
    public override void OnPointerUp(float x, float y)   => tracker.LastPointerUp   = (x, y);
    public override void OnKeyDown(string key)            => tracker.LastKeyDown     = key;
    public override void OnKeyUp(string key)              => tracker.LastKeyUp       = key;
}

internal sealed class SecondScreen : GameScreen
{
    public override void Draw(SKCanvas c, int w, int h) { }
}
