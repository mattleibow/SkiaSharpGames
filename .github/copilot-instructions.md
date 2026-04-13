# Copilot Instructions for SkiaSharpGames

## Project overview

This is a SkiaSharp game gallery — a .NET 10 Blazor WebAssembly app that hosts small games rendered with SkiaSharp. There are two projects:

- **`src/GameEngine/`** — Class library referenced by the Blazor app. Contains base classes and utilities shared across all games. References only `SkiaSharp`.
- **`src/BlazorApp/`** — Blazor WebAssembly host. Targets `net10.0`, requires the `wasm-tools` workload.

## Project Structure

```
src/BlazorApp/
  Games/<GameName>/           # Game logic (GameScreenBase subclass)
  Pages/
    Home.razor                # Gallery home page at /
    Games/<GameName>.razor    # Per-game page at /games/<slug>
  Shared/
    GameView.razor            # Reusable SKGLView component
  Layout/
    GameLayout.razor          # Full-screen layout for game pages
    MainLayout.razor          # Gallery layout with header

docs/screenshots/<slug>/      # 4 PNG screenshots per game
```

## Architecture

### Adding a new game

1. Create a class in `src/BlazorApp/Games/<GameName>/` that extends `GameScreenBase`.
2. Override `Update(float deltaTime)` for game logic and `Draw(SKCanvas, int width, int height)` for rendering.
3. Override `GameDimensions` to return the game's logical (virtual) width and height if different from the default 800 × 600.
4. Add a new game page at `src/BlazorApp/Pages/Games/<GameName>.razor`:
   ```razor
   @page "/games/<slug>"
   @layout GameLayout
   @using SkiaSharpGames.BlazorApp.Games.<GameName>

   <PageTitle><GameName> – SkiaSharp Games</PageTitle>
   <GameView Game="_game" />

   @code { private readonly <ClassName> _game = new(); }
   ```
5. Add a game card to the grid in `src/BlazorApp/Pages/Home.razor`.
6. **Update `README.md`** — see Documentation rules below.
7. **Add 4 screenshots** — see Screenshots section below.

### `GameScreenBase`

```csharp
public abstract class GameScreenBase
{
    public virtual (int width, int height) GameDimensions => (800, 600);
    public abstract void Update(float deltaTime);
    public abstract void Draw(SKCanvas canvas, int width, int height);
    public virtual void OnPointerMove(float x, float y) { }
    public virtual void OnPointerDown(float x, float y) { }

    // Transition system — call in Update() and at end of Draw():
    public bool IsTransitioning { get; }
    protected void BeginTransition(IScreenTransition t, float halfDuration, Action midpoint);
    protected void UpdateTransition(float deltaTime);
    protected void DrawTransitionOverlay(SKCanvas canvas);
}
```

### Screen Transitions

Use `BeginTransition` to animate between game states:

```csharp
BeginTransition(new FadeTransition(), 0.35f, StartGame);
BeginTransition(new SlideTransition { Direction = SlideDirection.Up }, 0.3f, StartGame);
```

Call `UpdateTransition(deltaTime)` at the top of `Update()`.
Call `DrawTransitionOverlay(canvas)` at the end of `Draw()` inside the game-space transform.

### Animated Values (`AnimatedFloat`)

Use `AnimatedFloat` to smoothly lerp a value to a target. The game loop calls `Update(dt)` and reads `.Value`.

```csharp
var paddleWidth = new AnimatedFloat(100f);
paddleWidth.AnimateTo(180f, 0.3f, Easing.BackOut);   // spring-expand
paddleWidth.AnimateTo(100f, 0.4f, Easing.EaseIn);    // shrink back
paddleWidth.Update(deltaTime);
float w = paddleWidth.Value;
```

Available easing functions: `Easing.Linear`, `EaseIn`, `EaseOut`, `EaseInOut`, `BounceOut`, `BackOut`, `ElasticOut`.

### Looping Animations (`LoopedAnimation`)

Use `LoopedAnimation` for repeating effects (shimmer, pulse, idle). It fires a `Progress` (0→1) animation on a configurable period.

```csharp
var shimmer = new LoopedAnimation(period: 8f, duration: 0.8f);
shimmer.Start(initialDelay: Random.Shared.NextSingle() * 8f); // stagger
shimmer.Update(deltaTime);
if (shimmer.IsActive) DrawEffect(shimmer.Progress);
```

`RectSprite` has a built-in `Shimmer` property. Call `sprite.Update(dt)` to advance it.

### Physics (`PhysicsBody`)

`PhysicsBody` supports rect and circle shapes.

```csharp
var ball  = new PhysicsBody(PhysicsShape.Circle) { Radius = 8f };
var brick = new PhysicsBody(PhysicsShape.Rect)   { Width = 72f, Height = 22f, IsStatic = true };

ball.Step(deltaTime);

// Test overlap and reflect velocity:
if (ball.Overlaps(brick)) ball.Reflect(brick);

// Or combined check + reflect:
ball.ReflectOff(paddle);
```

### Sprites

Use engine sprites to reduce drawing boilerplate. Call `Update(dt)` to advance sprite animations.

| Class | Use for |
|-------|---------|
| `RectSprite` | Bricks, paddle, panels. Properties: X, Y, Width, Height, Color, CornerRadius, ShowShine, Alpha, Shimmer |
| `CircleSprite` | Ball, particles. Properties: X, Y, Radius, Color, GlowRadius, GlowColor, Alpha |

```csharp
brick.Sprite.Shimmer.Start(Random.Shared.NextSingle() * brick.Sprite.Shimmer.Period);
brick.Sprite.Update(deltaTime);  // advances shimmer
brick.Sprite.Draw(canvas);
```

### Drawing utilities

```csharp
DrawHelper.FillRect(canvas, x, y, w, h, color);
DrawHelper.DrawOverlay(canvas, gameWidth, gameHeight, alpha: 0.73f);
DrawHelper.DrawCenteredText(canvas, text, size, color, cx, y);
DrawHelper.DrawText(canvas, text, size, color, x, y);
float w = DrawHelper.MeasureText(text, size);
```

### `GameView` component

`src/BlazorApp/Shared/GameView.razor` is the standard host for any game. It:

- Renders via `SKGLView` with `EnableRenderLoop="true"` — driven by `requestAnimationFrame`.
- Computes `deltaTime` in the paint callback.
- Handles **mouse** (`OffsetX/OffsetY`) and **touch** (`ClientX/ClientY` minus element offset) events.
- Converts CSS-pixel coordinates to game-space via `Game.GameDimensions`.

Usage: `<GameView Game="_game" />`

### Routing

| URL | Page | Layout |
|-----|------|--------|
| `/` | `Pages/Home.razor` | `MainLayout` (gallery + header) |
| `/games/<slug>` | `Pages/Games/<Name>.razor` | `GameLayout` (full-screen) |

**Always use relative hrefs** (no leading `/`) in game cards and links to ensure GitHub Pages compatibility:
```html
<a href="games/breakout">  ✅
<a href="/games/breakout"> ✗ breaks on GitHub Pages
```

## Screenshots requirement

**Every game must include 4 screenshots** in `docs/screenshots/<game-slug>/`:

| File | Description |
|------|-------------|
| `desktop-loading.png` | Desktop (≥ 1280 px wide) — loading/start screen |
| `desktop-gameplay.png` | Desktop — mid-game |
| `mobile-loading.png` | Mobile (≤ 430 px wide) — loading/start screen |
| `mobile-gameplay.png` | Mobile — mid-game |

These screenshots must be linked from the game's entry in `README.md`.

## Documentation

**Always update `README.md` when you change the code.** Specifically:

- If you add a new game, add it to the "Games" table and add a Screenshots section with all 4 images.
- If you change the `GameScreenBase` API or any engine primitive (AnimatedFloat, LoopedAnimation, PhysicsBody, Easing), update the relevant "Game Engine" section.
- If you change the CI/CD workflow, update the "CI/CD" section.
- If you add or remove projects, update the "Project Structure" section.
- **README.md must always include up-to-date screenshots for every game.**

## Building & running

```bash
# Install required workload (once per machine)
dotnet workload install wasm-tools

# Build the whole solution
dotnet build

# Run locally (hot-reload)
cd src/BlazorApp
dotnet watch run

# Publish for GitHub Pages
dotnet publish src/BlazorApp/SkiaSharpGames.BlazorApp.csproj -c Release -o dist
```

## CI/CD

The GitHub Actions workflow (`.github/workflows/deploy.yml`) runs on every push and every PR. It always deploys to GitHub Pages — PRs overwrite whatever is currently deployed.
