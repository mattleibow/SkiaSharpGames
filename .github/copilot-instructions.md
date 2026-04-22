# Copilot Instructions for SkiaSharpGames

## Project overview

This is a SkiaSharp game gallery — a .NET 10 Blazor WebAssembly app that hosts small games rendered with SkiaSharp. There are two projects:

- **`src/Engine/Theatre/`** — Class library referenced by the Blazor app. Contains base classes and utilities shared across all games. References only `SkiaSharp`.
- **`src/Apps/BlazorApp/`** — Blazor WebAssembly host. Targets `net10.0`, requires the `wasm-tools` workload.

## Project Structure

```
src/Apps/BlazorApp/
  Pages/
    Home.razor                # Gallery home page at /
    GamePage.razor            # Dynamic game page at /games/{slug}
  Shared/
    GameView.razor            # Reusable SKGLView component
  Layout/
    GameLayout.razor          # Full-screen layout for game pages
    MainLayout.razor          # Gallery layout with header

docs/screenshots/<slug>/      # 4 PNG screenshots per game
```

## Architecture

### Core Type Hierarchy (Theatre Terminology)

| Old Name | New Name | Description |
|----------|----------|-------------|
| `Entity` | `Actor` | Visible game object with position, transform, collision |
| `GameScreen` | `Scene` | Abstract game screen (start, play, game-over) |
| `Game` | `Stage` | Root of a running game, owns DI container |
| `GameBuilder` | `Theatre` | Builder that configures and creates a Stage |
| `GameOptions` | `StageOptions` | Options for Stage creation |
| `IScreenCoordinator` | `IDirector` | Interface for screen transitions/overlays |
| `IScreenDrawable` | `IStageRenderer` | Interface for update/draw pipeline |
| `ScreenCoordinator` | `Director` | Implementation of IDirector + IStageRenderer |
| `IScreenTransition` | `ICurtain` | Transition effect between scenes |
| `FadeToColorTransition` | `FadeCurtain` | Fade-to-colour curtain |
| `DissolveTransition` | `DissolveCurtain` | Cross-dissolve curtain |
| `SlideTransition` | `SlideCurtain` | Slide-in curtain |
| `ScreenCollection` | `SceneCollection` | Register scenes in Theatre |
| `AssetCollection` | `PropRoom` | Register assets in Theatre |
| `UiPointer` | `Spotlight` | Visible pointer/cursor |
| `UiEntity` | `UiActor` | Base UI element (extends Actor) |
| `GameTestHarness` | `Rehearsal` | Headless test harness |
| `GameBuilderUiExtensions` | `TheatreUiExtensions` | UI theme builder extensions |

### `SceneNode` (NEW base class)

Both `Actor` and `Scene` inherit from `SceneNode`, which provides:
- Parent/child hierarchy with `AddChild`, `RemoveChild`, `InsertChild`
- Child reordering: `MoveChildToFront`, `MoveChildToBack`, `MoveChildUp`, `MoveChildDown`
- `Name` property for debugging
- Recursive `Update(float dt)` and `Draw(SKCanvas canvas)`
- `Dump()` for tree inspection

### Adding a new game

1. Create game screens in `src/Games/<GameName>/` that extend `Scene`.
2. Override `Draw(SKCanvas canvas, int width, int height)` for rendering.
3. Create a static factory method:
   ```csharp
   public static Stage Create()
   {
       var builder = Theatre.Create();
       builder.Services.AddSingleton<MyGameState>();
       builder.Scenes.Add<StartScreen>().Add<PlayScreen>();
       builder.SetStageSize(800, 600);
       builder.SetOpeningScene<StartScreen>();
       return builder.Open();
   }
   ```
4. Register in `src/Apps/BlazorApp/GalleryGameRegistration.cs`.
5. **Update `README.md`** — see Documentation rules below.
6. **Add 4 screenshots** — see Screenshots section below.

### `Actor` (was Entity)

```csharp
public class Actor : SceneNode
{
    public float X, Y, Rotation, Alpha;
    public bool Visible;
    public SKMatrix44 LocalMatrix, WorldMatrix;
    public float WorldX, WorldY;
    public Collider2D? Collider;
    public Rigidbody2D? Rigidbody;
    public bool Overlaps(Actor other);
    public bool TryGetHit(Actor other, out CollisionHit hit);
    public bool BounceOff(Actor other);
    public Actor? FindChildCollision(Actor other, out CollisionHit hit);
    public SKImage CaptureToImage(int width, int height);
}
```

### `Scene` (was GameScreen)

```csharp
public abstract class Scene
{
    public bool IsPaused { get; internal set; }
    public Spotlight? Spotlight { get; protected set; }
    public virtual void Update(float deltaTime) { }
    public abstract void Draw(SKCanvas canvas, int width, int height);
    // Input
    public virtual void OnPointerMove/Down/Up(float x, float y) { }
    public virtual void OnKeyDown/Up(string key) { }
    // Lifecycle
    public virtual void OnActivating/Activated/Deactivating/Deactivated() { }
    public virtual void OnPaused/OnResumed() { }
}
```

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

### Actor Rendering (`OnDraw`)

Actors render themselves by overriding `OnDraw(SKCanvas canvas)`. The canvas is already translated to the actor's position. Actors also carry an `Alpha` property (0–1) for opacity.

```csharp
public class Brick : Actor
{
    public SKColor Color { get; set; }
    public LoopedAnimation Shimmer { get; } = new(period: 8f, duration: 0.8f);

    protected override void OnUpdate(float deltaTime) => Shimmer.Update(deltaTime);

    protected override void OnDraw(SKCanvas canvas)
    {
        // Draw at local origin — Actor.Draw() handles the transform
    }
}
```

### `UiLabel`

A shared text entity in `GameEngine.UI`. Used for HUD text, titles, and labels in all screens.

```csharp
var label = new UiLabel { Text = "Score: 0", FontSize = 24f, Color = SKColors.White, Align = TextAlign.Center };
label.X = 400f; label.Y = 30f;
label.Draw(canvas);
float w = label.MeasureWidth();
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

`src/Apps/BlazorApp/Shared/GameView.razor` is the standard host for any game. It:

- Renders via `SKGLView` with `EnableRenderLoop="true"` — driven by `requestAnimationFrame`.
- Computes `deltaTime` in the paint callback.
- Handles **mouse** (`OffsetX/OffsetY`) and **touch** (`ClientX/ClientY` minus element offset) events.
- Converts CSS-pixel coordinates to game-space via `Stage.StageSize`.

Usage: `<GameView Stage="_game" />`

### Routing

| URL | Page | Layout |
|-----|------|--------|
| `/` | `Pages/Home.razor` | `MainLayout` (gallery + header) |
| `/games/<slug>` | `Pages/GamePage.razor` | `GameLayout` (full-screen) |

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

### Screenshot capture policy (required)

- **Do not use placeholders, empty, or synthetic 1x1 files.** Screenshots must be real captures of the running app.
- **Use Playwright to capture screenshots** from the actual game page (`games/<slug>`) after starting the Blazor app.
- For every code change that affects behavior or visuals, **recapture and update screenshots** for impacted games before finishing.
- Missing or invalid screenshots are considered a **failed change**.

Required capture workflow:

1. Start the app locally (for example: `dotnet run --project src/Apps/BlazorApp/SkiaSharpGames.BlazorApp.csproj`).
2. Open `games/<slug>` (ensure the correct game is selected).
3. Capture:
   - `desktop-loading.png` at desktop size (>=1280 wide) on start/loading state
   - `desktop-gameplay.png` at desktop size during active gameplay/interaction
   - `mobile-loading.png` at mobile size (<=430 wide) on start/loading state
   - `mobile-gameplay.png` at mobile size during active gameplay/interaction
4. Verify files are valid PNG images and visually match the intended game state.

## Documentation

**Always update `README.md` when you change the code.** Specifically:

- If you add a new game, add it to the "Games" table and add a Screenshots section with all 4 images.
- If you change the `Scene` API or any engine primitive (AnimatedFloat, LoopedAnimation, Rigidbody2D, Easing), update the relevant "Game Engine" section.
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
cd src/Apps/BlazorApp
dotnet watch run

# Publish for GitHub Pages
dotnet publish src/Apps/BlazorApp/SkiaSharpGames.BlazorApp.csproj -c Release -o dist
```

## CI/CD

The GitHub Actions workflow (`.github/workflows/deploy.yml`) runs on every push and every PR. It always deploys to GitHub Pages — PRs overwrite whatever is currently deployed.
