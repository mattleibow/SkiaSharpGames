# SkiaSharp Games

A SkiaSharp game gallery built with .NET 10 Blazor WebAssembly. Games are rendered using SkiaSharp (via WebGL/`SKGLView`) and run entirely in the browser.

🎮 **[Play now on GitHub Pages](https://mattleibow.github.io/SkiaSharpGames/)**

## Games

| Game | URL | Description |
|------|-----|-------------|
| [Breakout](src/BlazorApp/Games/Breakout/) | `/games/breakout` | Classic brick-breaker. Move the paddle with mouse or touch to clear all the bricks. |

## Project Structure

```
SkiaSharpGames.slnx            # .NET 10 solution

src/
  GameEngine/                  # Shared game engine class library (SkiaSharp only)
    GameScreenBase.cs          # Abstract base class for all games

  BlazorApp/                   # Blazor WebAssembly host (net10.0)
    Games/
      Breakout/                # Breakout game logic
        BreakoutGameEngine.cs
    Pages/
      Home.razor               # Gallery home page  (/)
      Games/
        Breakout.razor         # Breakout game page (/games/breakout)
    Shared/
      GameView.razor           # Reusable SKGLView component (game loop + input)
    Layout/
      GameLayout.razor         # Full-screen layout for game pages
      MainLayout.razor         # Gallery layout with sticky header
    wwwroot/
      index.html               # App entry point
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
}
```

### Rendering

The `GameView` component wraps `SKGLView` with `EnableRenderLoop="true"`. The render loop is driven by `requestAnimationFrame` (synced to the display refresh rate). `Update` and `Draw` are called on every frame inside the paint callback — no `PeriodicTimer` or `Invalidate()` needed.

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
3. Implement `Update` and `Draw`. Optionally override `OnPointerMove`/`OnPointerDown`.
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
6. Update the Games table in this README.
7. Add 4 screenshots to `docs/screenshots/<slug>/`:
   - `desktop-loading.png` — desktop start/loading screen
   - `desktop-gameplay.png` — desktop mid-game
   - `mobile-loading.png` — mobile start/loading screen
   - `mobile-gameplay.png` — mobile mid-game

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

