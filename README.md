# SkiaSharp Games

A SkiaSharp game gallery built with .NET 10 Blazor WebAssembly. Games are rendered using SkiaSharp (via WebGL/`SKGLView`) and run entirely in the browser.

🎮 **[Play now on GitHub Pages](https://mattleibow.github.io/SkiaSharpGames/)**

## Games

| Game | URL | Description |
|------|-----|-------------|
| [Breakout](src/BlazorApp/Games/Breakout/) | `/games/breakout` | Classic brick-breaker with powerups. Move the paddle with mouse or touch to clear all the bricks. |
| [Castle Attack](src/BlazorApp/Games/CastleAttack/) | `/games/castle-attack` | Tower-defence shooter. Command archers and workers to defend three walls while building the keep. |

### Breakout

| Desktop | Mobile |
|---------|--------|
| ![Desktop – Start screen](https://github.com/user-attachments/assets/7386849c-f6fc-4e18-8647-12d33c7239e9) | ![Mobile – Start screen](https://github.com/user-attachments/assets/d9ba932b-6200-421a-881a-0891d2392c5c) |
| ![Desktop – Gameplay](https://github.com/user-attachments/assets/f2f22d8c-1fd1-4b35-a5fc-10e34e15e127) | ![Mobile – Gameplay](https://github.com/user-attachments/assets/bd8134f6-911c-471c-a361-c3c5bb4c5489) |

**Features:** 5×10 colour-coded bricks (top rows = more points), 3 lives, mouse & touch input, powerup drops:
- **STRONG BALL** (15% chance) — ball pierces through all bricks for 5 s (ball turns orange)
- **BIG PADDLE** (15% chance) — paddle springs to 1.8× wider for 8 s (animated via `AnimatedFloat`)
- Bricks have a periodic shimmer sweep powered by `LoopedAnimation`

### Castle Attack

| Desktop | Mobile |
|---------|--------|
| ![Desktop – Start screen](docs/screenshots/castle-attack/desktop-loading.png) | ![Mobile – Start screen](docs/screenshots/castle-attack/mobile-loading.png) |
| ![Desktop – Gameplay](docs/screenshots/castle-attack/desktop-gameplay.png) | ![Mobile – Gameplay](docs/screenshots/castle-attack/mobile-gameplay.png) |

**Features:** Tower-defence shooter set in a medieval siege. Defend three concentric walls (2, 4, 6 blocks tall) while workers build the keep:
- **Three walls** — outer (2 blocks), middle (4 blocks), inner (6 blocks), each taking independent damage
- **Archers + Workers** — start with 3 of each; convert freely with `↑/↓` or on-screen buttons (always keep ≥ 1 worker)
- **Parabolic arrow fire** — `SPACE` or FIRE button; dotted arc preview shows trajectory and landing point
- **Aim sweep** — hold `← →` or hold aim buttons; step on tap
- **Seven enemy types** — Spearman, Swordsman, Berserker, Crossbowman, Catapult, Ram, and the secret Cow 🐄
- **Special weapons** (one-use each) — Boiling Oil (near-wall area clear), Mangonel (long-range salvo), Flaming Logs (full-screen wipe)
- **Lord defender** — golden champion activates and fights back when all walls fall
- **Victory** — keep reaches 100% construction; **Defeat** — Lord's HP drops to zero
- **Accuracy multiplier** — consecutive hits with a single archer build a ×8 scoring streak
- **Full mobile support** — on-screen button bar + tap-to-aim-and-fire anywhere in the battlefield

## Project Structure

```
SkiaSharpGames.slnx            # .NET 10 solution

src/
  GameEngine/                  # Shared game engine class library (SkiaSharp only)
    GameScreenBase.cs          # Abstract base class + transition system
    IScreenTransition.cs       # Swappable transition interface
    FadeTransition.cs          # Fade-to-colour transition
    SlideTransition.cs         # Wipe/slide transition
    Sprite.cs                  # Abstract sprite base with Update(dt)
    RectSprite.cs              # Rounded-rect sprite + shimmer LoopedAnimation
    CircleSprite.cs            # Circle sprite with glow
    DrawHelper.cs              # Static drawing utilities
    Easing.cs                  # Easing functions (Linear, EaseIn/Out, BounceOut, BackOut, ElasticOut)
    AnimatedFloat.cs           # One-shot lerped float with easing + duration
    LoopedAnimation.cs         # Periodic repeating animation (period, duration, progress 0→1)
    PhysicsBody.cs             # Rect/circle body with Step, Overlaps, Reflect, ReflectOff

  BlazorApp/                   # Blazor WebAssembly host (net10.0)
    Games/
      Breakout/                # Breakout game logic
        BreakoutGameEngine.cs
      CastleAttack/            # Castle Attack game logic
        CastleAttackGame.cs
    Pages/
      Home.razor               # Gallery home page  (/)
      Games/
        Breakout.razor         # Breakout game page (/games/breakout)
        CastleAttack.razor     # Castle Attack page (/games/castle-attack)
    Shared/
      GameView.razor           # Reusable SKGLView component (game loop + input)
    Layout/
      GameLayout.razor         # Full-screen layout for game pages
      MainLayout.razor         # Gallery layout with sticky header
    wwwroot/
      index.html               # App entry point

tests/
  GameEngine.Tests/            # xUnit tests for the game engine (111 tests)
    EasingTests.cs             # All 6 easing functions, all branches including BounceOut segments
    AnimatedFloatTests.cs      # Initial value, AnimateTo, SetImmediate, Update lifecycle
    LoopedAnimationTests.cs    # Start/Stop, period/active/complete phases, RepeatCount
    PhysicsBodyTests.cs        # 100% branch coverage — Step, BoundingBox, Overlaps (all 4 pairs),
                               #   Reflect (guards + minV/minH axis), ReflectOff (all branches incl. len=0)
    TransitionTests.cs         # FadeTransition, SlideTransition (all 4 dirs), GameScreenBase transitions
    SpriteTests.cs             # RectSprite (visible/invisible/alpha/shine/shimmer), CircleSprite (glow)

docs/screenshots/              # Per-game screenshot assets
  breakout/
    desktop-loading.png
    desktop-gameplay.png
    mobile-loading.png
    mobile-gameplay.png
  castle-attack/
    desktop-loading.png
    desktop-gameplay.png
    mobile-loading.png
    mobile-gameplay.png
```

## Game Engine

All games share a thin game engine library in `src/GameEngine`. The central type is `GameScreenBase`:

```csharp
public abstract class GameScreenBase
{
    // Logical (virtual) canvas size. Default: 800 × 600.
    public virtual (int width, int height) GameDimensions => (800, 600);

    // Called every frame; deltaTime is in seconds.
    public abstract void Update(float deltaTime);

    // Called every frame; draw everything onto the canvas (physical pixels).
    public abstract void Draw(SKCanvas canvas, int width, int height);

    // Optional pointer input — coordinates are in game-space units.
    public virtual void OnPointerMove(float x, float y) { }
    public virtual void OnPointerDown(float x, float y) { }
    public virtual void OnPointerUp(float x, float y) { }

    // Optional keyboard input — key is the browser key value string (e.g. "ArrowLeft", " ", "z").
    public virtual void OnKeyDown(string key) { }
    public virtual void OnKeyUp(string key) { }

    // ── Transition system ──────────────────────────────────────────
    public bool IsTransitioning { get; }
    protected void BeginTransition(IScreenTransition transition, float halfDuration, Action midpointAction);
    protected void UpdateTransition(float deltaTime);   // call at top of Update()
    protected void DrawTransitionOverlay(SKCanvas canvas); // call at end of Draw()
}
```

### Rendering

The `GameView` component wraps `SKGLView` with `EnableRenderLoop="true"`. The render loop is driven by `requestAnimationFrame` (synced to the display refresh rate). `Update` and `Draw` are called on every frame inside the paint callback — no `PeriodicTimer` or `Invalidate()` needed.

### Screen Transitions

Use `BeginTransition` to animate between game states (e.g. from start screen to gameplay):

```csharp
// Fade to black, call StartGame(), then fade back in
BeginTransition(new FadeTransition(), halfDuration: 0.35f, StartGame);

// Or use a directional wipe:
BeginTransition(new SlideTransition { Direction = SlideDirection.Up }, 0.3f, StartGame);
```

### Animated Values

`AnimatedFloat` lerps a float from its current value to a target over a duration using a configurable easing function. Call `Update(dt)` every tick.

```csharp
var paddleWidth = new AnimatedFloat(100f);

// Spring-expand the paddle when powerup activates
paddleWidth.AnimateTo(180f, duration: 0.3f, Easing.BackOut);

// Smoothly shrink it back when the powerup expires
paddleWidth.AnimateTo(100f, duration: 0.4f, Easing.EaseIn);

// In Update():
paddleWidth.Update(deltaTime);
float w = paddleWidth.Value;
```

Available easing functions in `Easing`:

| Name | Effect |
|------|--------|
| `Linear` | Constant rate |
| `EaseIn` | Accelerates from rest |
| `EaseOut` | Decelerates to rest |
| `EaseInOut` | Smooth-step (accelerate then decelerate) |
| `BounceOut` | Bounces at the end |
| `BackOut` | Overshoots slightly then settles |
| `ElasticOut` | Elastic snap past the target |

### Looping Animations

`LoopedAnimation` fires a normalised `Progress` (0 → 1) animation on a configurable period. Use it for repeating effects like shimmer, pulse, or idle animations.

```csharp
// 0.8 s shimmer every 8 s, staggered across instances
var shimmer = new LoopedAnimation(period: 8f, duration: 0.8f);
shimmer.Start(initialDelay: Random.Shared.NextSingle() * 8f);

// In Update():
shimmer.Update(deltaTime);

// In Draw():
if (shimmer.IsActive)
    DrawShimmerStripe(shimmer.Progress);
```

`RectSprite` already has a built-in `Shimmer` property (a `LoopedAnimation` with period=8s, duration=0.8s). Call `Sprite.Update(dt)` to advance it.

### Physics

`PhysicsBody` is a simple circle-or-rect body. Use `Step(dt)` to advance position, `Overlaps` to test collision, and `Reflect`/`ReflectOff` to bounce.

```csharp
var ball   = new PhysicsBody(PhysicsShape.Circle) { Radius = 8f };
var brick  = new PhysicsBody(PhysicsShape.Rect)   { Width = 72f, Height = 22f, IsStatic = true };

ball.Step(deltaTime);

if (!piercing && ball.Overlaps(brick))
{
    ball.Reflect(brick);  // reflect velocity off brick; already know they overlap
    brick.Active = false;
}

// Or use the combined check + reflect:
ball.ReflectOff(paddle);  // returns true if overlap was detected
```

### Sprites

Engine sprites encapsulate their own draw logic. Update their properties and call `Draw(canvas)`. Call `Update(dt)` to advance animations (e.g. shimmer on `RectSprite`):

| Type | Properties | Use for |
|------|-----------|---------|
| `RectSprite` | X, Y, Width, Height, Color, CornerRadius, ShowShine, Alpha, Shimmer | Bricks, paddle, panels |
| `CircleSprite` | X, Y, Radius, Color, GlowRadius, GlowColor, Alpha | Ball, particles |

```csharp
// Enable staggered shimmer on a brick
brick.Sprite.Shimmer.Start(Random.Shared.NextSingle() * brick.Sprite.Shimmer.Period);

// Advance in Update():
brick.Sprite.Update(deltaTime);
```

### Drawing Utilities

`DrawHelper` provides stateless convenience methods:

```csharp
DrawHelper.FillRect(canvas, x, y, w, h, color);
DrawHelper.DrawOverlay(canvas, width, height, alpha: 0.73f);
DrawHelper.DrawCenteredText(canvas, text, size, color, cx, y);
DrawHelper.DrawText(canvas, text, size, color, x, y);
DrawHelper.MeasureText(text, size);
```

### Input

`GameView` handles both **mouse** (desktop) and **touch** (mobile) events. CSS-pixel coordinates are automatically converted to game-space units using `GameDimensions`.

### Routing

| URL | Page | Layout |
|-----|------|--------|
| `/` | `Pages/Home.razor` | `MainLayout` — sticky header, scrollable gallery |
| `/games/<slug>` | `Pages/Games/<Name>.razor` | `GameLayout` — full-screen, no chrome |

## Adding a New Game

1. Create `src/BlazorApp/Games/<Name>/<ClassName>.cs` extending `GameScreenBase`.
2. Override `GameDimensions` if your game uses dimensions other than 800 × 600.
3. Implement `Update` and `Draw`. Optionally override `OnPointerMove`/`OnPointerDown`/`OnPointerUp` and `OnKeyDown`/`OnKeyUp`.
4. Create `src/BlazorApp/Pages/Games/<Name>.razor`:
   ```razor
   @page "/games/<slug>"
   @layout GameLayout
   @using SkiaSharpGames.BlazorApp.Games.<Name>

   <PageTitle><Name> – SkiaSharp Games</PageTitle>
   <GameView Game="_game" />

   @code { private readonly <ClassName> _game = new(); }
   ```
5. Add a game card to the grid in `src/BlazorApp/Pages/Home.razor`.
6. **Update `README.md`** — add the game to the Games table and add a Screenshots section with all 4 images.
7. Add 4 screenshots to `docs/screenshots/<slug>/`:
   | File | Description |
   |------|-------------|
   | `desktop-loading.png` | Desktop (≥ 1280 px wide) — start/loading screen |
   | `desktop-gameplay.png` | Desktop — mid-game |
   | `mobile-loading.png` | Mobile (≤ 430 px wide) — start/loading screen |
   | `mobile-gameplay.png` | Mobile — mid-game |

## Testing

The game engine is tested with xUnit (111 tests). Coverage is measured with coverlet — the thresholds enforced in CI are **≥ 80% line** and **≥ 80% branch**; `PhysicsBody` achieves **100% line and branch coverage**.

```bash
dotnet test tests/GameEngine.Tests/
```

To run with full coverage report:

```bash
dotnet test tests/GameEngine.Tests/ \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=coverage/coverage.cobertura.xml
```

| Area | Tests | Coverage focus |
|------|-------|----------------|
| `Easing` | 22 | All 6 functions; BounceOut all 4 segments; ElasticOut boundaries |
| `AnimatedFloat` | 13 | AnimateTo (positive/zero/negative duration), SetImmediate, Update lifecycle |
| `LoopedAnimation` | 14 | Start/Stop, period/active/complete phases, RepeatCount (finite/infinite/zero) |
| `PhysicsBody` | 30 | **100% branch coverage** — Step, BoundingBox, Overlaps (all 4 shape pairs), Reflect (guards + minV≤minH + minH<minV), ReflectOff (no overlap, circle-circle len=0, circle-circle len>0, circle-rect, rect-circle, rect-rect) |
| Transitions | 19 | FadeTransition (0/0.5/1 coverage), SlideTransition (all 4 directions), GameScreenBase Out/In phases |
| Sprites | 13 | RectSprite (visible/invisible/alpha-zero/shine/shimmer), CircleSprite (glow/no-glow) |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `wasm-tools` workload:

```bash
dotnet workload install wasm-tools
```

### Running locally

```bash
cd src/BlazorApp
dotnet watch run
```

Then open http://localhost:5000.

### Building for production

```bash
dotnet publish src/BlazorApp/SkiaSharpGames.BlazorApp.csproj -c Release -o dist
```

## CI/CD

The GitHub Actions workflow in `.github/workflows/deploy.yml` runs on every push **and** every PR. It always deploys to GitHub Pages — PRs overwrite the live site, so changes are instantly previewable.

The deployed URL is: **https://mattleibow.github.io/SkiaSharpGames/**

### Workflow steps

1. Setup .NET 10 + install `wasm-tools` workload
2. Restore → build → publish Blazor app
3. Patch `<base href>` to `/SkiaSharpGames/` for GitHub Pages sub-path
4. Copy `index.html` → `404.html` for SPA client-side routing
5. Deploy via `actions/deploy-pages`

