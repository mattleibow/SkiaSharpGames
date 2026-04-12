# Copilot Instructions for SkiaSharpGames

## Project overview

This is a SkiaSharp game gallery — a .NET 10 Blazor WebAssembly app that hosts small games rendered with SkiaSharp. There are two projects:

- **`src/GameEngine/`** — Class library referenced by the Blazor app. Contains base classes and utilities shared across all games. References only `SkiaSharp`.
- **`src/BlazorApp/`** — Blazor WebAssembly host. Targets `net10.0`, requires the `wasm-tools` workload. Each game lives in `src/BlazorApp/Games/<GameName>/`.

## Architecture

### Adding a new game

1. Create a class in `src/BlazorApp/Games/<GameName>/` that extends `GameScreenBase` (from `SkiaSharpGames.GameEngine`).
2. Override `Update(float deltaTime)` for game logic and `Draw(SKCanvas, int width, int height)` for rendering.
3. Add a new Blazor page at `src/BlazorApp/Pages/<GameName>.razor` using `@layout GameLayout` and `SKCanvasView`.
4. Update `README.md` to list the new game in the Games table.

### `GameScreenBase`

```csharp
public abstract class GameScreenBase
{
    public abstract void Update(float deltaTime);
    public abstract void Draw(SKCanvas canvas, int width, int height);
    public virtual void OnPointerMove(float x, float y) { }
    public virtual void OnPointerDown(float x, float y) { }
}
```

All rendering uses virtual/logical coordinates; the Blazor page scales to fill the viewport.

### Game loop

Each game page drives a ~60 FPS loop with `PeriodicTimer` and calls `SKCanvasView.Invalidate()` to redraw:

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16));
while (await timer.WaitForNextTickAsync(ct))
{
    _game.Update(deltaTime);
    _canvasView?.Invalidate();
}
```

## Key conventions

- All game logic belongs in `GameScreenBase` subclasses, not in `.razor` files.
- Use `SKCanvas` drawing APIs (SkiaSharp) for all rendering; avoid HTML/CSS for game visuals.
- Coordinate conversion between CSS pixels and game-space is done via `CanvasToGame(cssPx, cssPy, cssWidth, cssHeight)`.
- The `GameLayout` layout removes the nav sidebar so games are full-screen.

## Documentation

**Always update `README.md` when you change the code.** Specifically:

- If you add a new game, add it to the "Games" table in `README.md`.
- If you change the `GameScreenBase` API, update the "Game Engine" section in `README.md`.
- If you change the CI/CD workflow, update the "CI/CD" section in `README.md`.
- If you add or remove projects, update the "Project Structure" section in `README.md`.

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
