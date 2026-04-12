# SkiaSharp Games

A SkiaSharp game gallery built with .NET 10 Blazor WebAssembly. Games are rendered using SkiaSharp and run entirely in the browser via WebAssembly.

🎮 **[Play now on GitHub Pages](https://mattleibow.github.io/SkiaSharpGames/)**

## Games

| Game | Description |
|------|-------------|
| [Breakout](src/BlazorApp/Games/Breakout/) | Classic brick-breaker. Move the paddle to keep the ball in play and clear all the bricks. |

## Project Structure

```
SkiaSharpGames.slnx          # .NET 10 solution

src/
  GameEngine/                # Shared game engine class library (SkiaSharp only)
    GameScreenBase.cs        # Abstract base class for all game screens

  BlazorApp/                 # Blazor WebAssembly host app (net10.0)
    Games/
      Breakout/              # Breakout game implementation
        BreakoutGameEngine.cs
    Pages/
      Home.razor             # Hosts the Breakout game
    Layout/
      GameLayout.razor       # Full-screen layout (no nav sidebar)
    wwwroot/
      index.html             # Entry point
```

## Game Engine

All games share a thin game engine library in `src/GameEngine`. The central type is `GameScreenBase`:

```csharp
public abstract class GameScreenBase
{
    // Called each tick; deltaTime is in seconds
    public abstract void Update(float deltaTime);

    // Called each frame; draw everything onto the provided canvas
    public abstract void Draw(SKCanvas canvas, int width, int height);

    // Optional pointer input handlers
    public virtual void OnPointerMove(float x, float y) { }
    public virtual void OnPointerDown(float x, float y) { }
}
```

To add a new game, subclass `GameScreenBase`, add it to `src/BlazorApp/Games/<Name>/`, and create a Blazor page for it.

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

The GitHub Actions workflow in `.github/workflows/deploy.yml` runs on every push and every PR. It always deploys to GitHub Pages — PRs overwrite the live site, so you can preview changes immediately.

The deployed URL is: **https://mattleibow.github.io/SkiaSharpGames/**

### Workflow steps

1. Setup .NET 10 + install `wasm-tools` workload
2. Restore, build, publish Blazor app
3. Patch `<base href>` to `/SkiaSharpGames/` for GitHub Pages sub-path
4. Copy `index.html` → `404.html` for SPA client-side routing
5. Deploy via `actions/deploy-pages`
