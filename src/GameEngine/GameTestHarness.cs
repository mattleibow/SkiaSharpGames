using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A rendered frame captured by <see cref="GameTestHarness"/>. Provides pixel-level
/// inspection, region assertions, and PNG export for debugging.
/// </summary>
public sealed class FrameSnapshot : IDisposable
{
    private readonly SKBitmap _bitmap;

    internal FrameSnapshot(SKBitmap bitmap, int frameNumber, float elapsedTime)
    {
        _bitmap = bitmap;
        FrameNumber = frameNumber;
        ElapsedTime = elapsedTime;
    }

    /// <summary>The frame number (0-based) in the test run.</summary>
    public int FrameNumber { get; }

    /// <summary>Total elapsed game time at the point this frame was captured.</summary>
    public float ElapsedTime { get; }

    /// <summary>Bitmap width in pixels.</summary>
    public int Width => _bitmap.Width;

    /// <summary>Bitmap height in pixels.</summary>
    public int Height => _bitmap.Height;

    /// <summary>Gets the color of a single pixel.</summary>
    public SKColor GetPixel(int x, int y) => _bitmap.GetPixel(x, y);

    /// <summary>
    /// Returns true if any pixel in the given rectangle is not the specified color.
    /// Useful for asserting "something was drawn in this region".
    /// </summary>
    public bool HasNonBackgroundPixel(SKRectI region, SKColor background)
    {
        int left = Math.Max(0, region.Left);
        int top = Math.Max(0, region.Top);
        int right = Math.Min(Width, region.Right);
        int bottom = Math.Min(Height, region.Bottom);

        for (int y = top; y < bottom; y++)
            for (int x = left; x < right; x++)
                if (_bitmap.GetPixel(x, y) != background)
                    return true;
        return false;
    }

    /// <summary>
    /// Returns true if all pixels in the given rectangle are the specified color.
    /// </summary>
    public bool IsRegionSolidColor(SKRectI region, SKColor color)
    {
        int left = Math.Max(0, region.Left);
        int top = Math.Max(0, region.Top);
        int right = Math.Min(Width, region.Right);
        int bottom = Math.Min(Height, region.Bottom);

        for (int y = top; y < bottom; y++)
            for (int x = left; x < right; x++)
                if (_bitmap.GetPixel(x, y) != color)
                    return false;
        return true;
    }

    /// <summary>
    /// Counts pixels in the region that match the given color (with optional alpha tolerance).
    /// </summary>
    public int CountPixels(SKRectI region, SKColor color, byte tolerance = 0)
    {
        int left = Math.Max(0, region.Left);
        int top = Math.Max(0, region.Top);
        int right = Math.Min(Width, region.Right);
        int bottom = Math.Min(Height, region.Bottom);
        int count = 0;

        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                var px = _bitmap.GetPixel(x, y);
                if (tolerance == 0)
                {
                    if (px == color) count++;
                }
                else
                {
                    if (Math.Abs(px.Red - color.Red) <= tolerance &&
                        Math.Abs(px.Green - color.Green) <= tolerance &&
                        Math.Abs(px.Blue - color.Blue) <= tolerance &&
                        Math.Abs(px.Alpha - color.Alpha) <= tolerance)
                        count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Computes the fraction of pixels that differ between this frame and another.
    /// Returns 0.0 for identical frames, 1.0 if every pixel changed.
    /// </summary>
    public float DiffRatio(FrameSnapshot other)
    {
        if (Width != other.Width || Height != other.Height)
            throw new ArgumentException("Frame dimensions must match for comparison.");

        int diffCount = 0;
        int total = Width * Height;
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (_bitmap.GetPixel(x, y) != other._bitmap.GetPixel(x, y))
                    diffCount++;

        return total == 0 ? 0f : (float)diffCount / total;
    }

    /// <summary>
    /// Saves the frame as a PNG file for visual debugging.
    /// </summary>
    public void SavePng(string path)
    {
        using var image = SKImage.FromBitmap(_bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    /// <inheritdoc />
    public void Dispose() => _bitmap.Dispose();
}

/// <summary>
/// Headless test harness for running a <see cref="Game"/> without a UI host.
/// Simulates frames, input, and captures rendered output for inspection.
/// </summary>
/// <example>
/// <code>
/// using var harness = GameTestHarness.Create(builder => {
///     builder.Screens.Add&lt;MyStartScreen&gt;().Add&lt;MyPlayScreen&gt;();
///     builder.SetInitialScreen&lt;MyStartScreen&gt;();
/// });
///
/// harness.RunFrames(10);               // advance 10 frames
/// harness.PointerDown(400, 300);       // simulate click
/// harness.RunFrames(60);               // play for 1 second
///
/// var frame = harness.CaptureFrame();
/// Assert.True(frame.HasNonBackgroundPixel(region, SKColors.Black));
/// </code>
/// </example>
public sealed class GameTestHarness : IDisposable
{
    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas;
    private int _frameNumber;
    private float _elapsedTime;

    private GameTestHarness(Game game, int width, int height)
    {
        Game = game;
        _bitmap = new SKBitmap(width, height);
        _canvas = new SKCanvas(_bitmap);
    }

    /// <summary>The underlying Game instance. Access services, coordinator, etc.</summary>
    public Game Game { get; }

    /// <summary>Total frames simulated so far.</summary>
    public int FrameNumber => _frameNumber;

    /// <summary>Total elapsed game time in seconds.</summary>
    public float ElapsedTime => _elapsedTime;

    /// <summary>Render width in pixels.</summary>
    public int Width => _bitmap.Width;

    /// <summary>Render height in pixels.</summary>
    public int Height => _bitmap.Height;

    // ── Factory ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a harness by configuring a <see cref="GameBuilder"/>.
    /// </summary>
    /// <param name="configure">Action to configure screens, services, dimensions.</param>
    /// <param name="width">Render surface width (default: 800).</param>
    /// <param name="height">Render surface height (default: 600).</param>
    public static GameTestHarness Create(Action<GameBuilder> configure, int width = 800, int height = 600)
    {
        var builder = GameBuilder.CreateDefault();
        configure(builder);
        var game = builder.Build();
        return new GameTestHarness(game, width, height);
    }

    /// <summary>
    /// Creates a harness from an already-built <see cref="Game"/>.
    /// </summary>
    public static GameTestHarness FromGame(Game game, int width = 800, int height = 600)
        => new(game, width, height);

    // ── Frame simulation ─────────────────────────────────────────────

    /// <summary>
    /// Advances the game by one frame: calls Update then Draw.
    /// </summary>
    /// <param name="deltaTime">Time step in seconds (default: ~60fps).</param>
    public void RunFrame(float deltaTime = 1f / 60f)
    {
        Game.Update(deltaTime);
        Game.Draw(_canvas, _bitmap.Width, _bitmap.Height);
        _frameNumber++;
        _elapsedTime += deltaTime;
    }

    /// <summary>
    /// Advances the game by N frames at the given time step.
    /// </summary>
    /// <param name="count">Number of frames to simulate.</param>
    /// <param name="deltaTime">Time step per frame in seconds.</param>
    public void RunFrames(int count, float deltaTime = 1f / 60f)
    {
        for (int i = 0; i < count; i++)
            RunFrame(deltaTime);
    }

    /// <summary>
    /// Advances the game for the specified duration at the given time step.
    /// </summary>
    /// <param name="seconds">Total time to simulate.</param>
    /// <param name="deltaTime">Time step per frame in seconds.</param>
    public void RunFor(float seconds, float deltaTime = 1f / 60f)
    {
        float t = 0f;
        while (t < seconds)
        {
            float step = MathF.Min(deltaTime, seconds - t);
            RunFrame(step);
            t += step;
        }
    }

    // ── Input simulation ─────────────────────────────────────────────

    /// <summary>Simulate a pointer/mouse move to (x, y) in game-space coordinates.</summary>
    public void PointerMove(float x, float y) => Game.OnPointerMove(x, y);

    /// <summary>Simulate a pointer/mouse press at (x, y) in game-space coordinates.</summary>
    public void PointerDown(float x, float y) => Game.OnPointerDown(x, y);

    /// <summary>Simulate a pointer/mouse release at (x, y) in game-space coordinates.</summary>
    public void PointerUp(float x, float y) => Game.OnPointerUp(x, y);

    /// <summary>Simulate a click: pointer down, optional frames, pointer up.</summary>
    public void Click(float x, float y, int holdFrames = 0, float deltaTime = 1f / 60f)
    {
        PointerDown(x, y);
        RunFrames(holdFrames, deltaTime);
        PointerUp(x, y);
    }

    /// <summary>Simulate a key press.</summary>
    public void KeyDown(string key) => Game.OnKeyDown(key);

    /// <summary>Simulate a key release.</summary>
    public void KeyUp(string key) => Game.OnKeyUp(key);

    /// <summary>Simulate a key tap: down then up.</summary>
    public void KeyTap(string key)
    {
        KeyDown(key);
        KeyUp(key);
    }

    // ── Frame capture & inspection ───────────────────────────────────

    /// <summary>
    /// Captures the current rendered frame as a <see cref="FrameSnapshot"/>
    /// for pixel-level inspection. The snapshot is a copy — subsequent frames
    /// don't affect it.
    /// </summary>
    public FrameSnapshot CaptureFrame()
    {
        // Draw current state to get a fresh frame
        Game.Draw(_canvas, _bitmap.Width, _bitmap.Height);
        var copy = _bitmap.Copy();
        return new FrameSnapshot(copy, _frameNumber, _elapsedTime);
    }

    /// <summary>
    /// Gets the pixel color at the given coordinates from the most recently rendered frame.
    /// </summary>
    public SKColor GetPixel(int x, int y) => _bitmap.GetPixel(x, y);

    /// <summary>
    /// Saves the current frame as a PNG for visual debugging.
    /// </summary>
    public void SaveFrame(string path)
    {
        using var image = SKImage.FromBitmap(_bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    // ── Screen navigation ────────────────────────────────────────────

    /// <summary>
    /// Transitions to a different screen. Shortcut for accessing the coordinator.
    /// </summary>
    public void TransitionTo<TScreen>(IScreenTransition? transition = null) where TScreen : GameScreen
    {
        var coordinator = Game.Services.GetRequiredService<IScreenCoordinator>();
        coordinator.TransitionTo<TScreen>(transition);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _canvas.Dispose();
        _bitmap.Dispose();
    }
}
