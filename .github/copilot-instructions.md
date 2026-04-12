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
6. **Update `README.md`** — add the game to the Games table.
7. **Add screenshots** — see the Screenshots section below.

### `GameScreenBase`

```csharp
public abstract class GameScreenBase
{
    // Logical (virtual) canvas size. Default: 800 × 600.
    public virtual (int width, int height) GameDimensions => (800, 600);

    public abstract void Update(float deltaTime);
    public abstract void Draw(SKCanvas canvas, int width, int height);
    public virtual void OnPointerMove(float x, float y) { }
    public virtual void OnPointerDown(float x, float y) { }
}
```

- `Update` and `Draw` are called on every frame (tied to display refresh rate via `SKGLView`).
- `OnPointerMove`/`OnPointerDown` receive **game-space** coordinates (scaled from CSS pixels by `GameView`).

### `GameView` component

`src/BlazorApp/Shared/GameView.razor` is the standard host for any game. It:

- Renders via `SKGLView` with `EnableRenderLoop="true"` — the render loop is driven by `requestAnimationFrame`, synced to the display refresh rate. No `PeriodicTimer` is needed.
- Computes `deltaTime` in the paint callback.
- Handles **mouse** (`OffsetX/OffsetY`) and **touch** (`ClientX/ClientY` minus element offset) events on both desktop and mobile.
- Converts CSS-pixel coordinates to game-space via `Game.GameDimensions`.

Usage:

```razor
<GameView Game="_game" />
```

### Routing

| URL | Page | Layout |
|-----|------|--------|
| `/` | `Pages/Home.razor` | `MainLayout` (gallery + header) |
| `/games/<slug>` | `Pages/Games/<Name>.razor` | `GameLayout` (full-screen) |

## Screenshots requirement

**Every game must include 4 screenshots** checked into the repository at `docs/screenshots/<game-slug>/`:

| File | Description |
|------|-------------|
| `desktop-loading.png` | Desktop (≥1280 px wide) — loading/start screen |
| `desktop-gameplay.png` | Desktop — mid-game |
| `mobile-loading.png` | Mobile (≤430 px wide) — loading/start screen |
| `mobile-gameplay.png` | Mobile — mid-game |

These screenshots must be linked from the game's entry in `README.md`.

## Documentation

**Always update `README.md` when you change the code.** Specifically:

- If you add a new game, add it to the "Games" table in `README.md` with screenshot links.
- If you change the `GameScreenBase` API, update the "Game Engine" section in `README.md`.
- If you change the CI/CD workflow, update the "CI/CD" section in `README.md`.
- If you add or remove projects, update the "Project Structure" section in `README.md`.
- If you change routing, update the "Routing" section in `README.md`.

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
