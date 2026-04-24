using Microsoft.Extensions.DependencyInjection;

using SkiaSharp;

namespace SkiaSharp.Theatre.Rehearsals;

/// <summary>
/// Headless test harness for running a <see cref="Stage"/> without a UI host.
/// Simulates frames, input, and captures rendered output for inspection.
/// </summary>
/// <example>
/// <code>
/// using var harness = Rehearsal.Create(builder => {
///     builder.Scenes.Add&lt;MyStartScreen&gt;().Add&lt;MyPlayScreen&gt;();
///     builder.SetOpeningScene&lt;MyStartScreen&gt;();
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
public sealed class Rehearsal : IDisposable
{
    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas;
    private int _frameNumber;
    private float _elapsedTime;

    private Rehearsal(Stage stage, int width, int height)
    {
        Stage = stage;
        _bitmap = new SKBitmap(width, height);
        _canvas = new SKCanvas(_bitmap);
    }

    /// <summary>The underlying Stage instance. Access services, director, etc.</summary>
    public Stage Stage { get; }

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
    /// Creates a harness by configuring a <see cref="StageBuilder"/>.
    /// </summary>
    public static Rehearsal Create(
        Action<StageBuilder> configure,
        int width = 800,
        int height = 600
    )
    {
        var builder = StageBuilder.Create();
        configure(builder);
        var stage = builder.Open();
        return new Rehearsal(stage, width, height);
    }

    /// <summary>
    /// Creates a harness from an already-built <see cref="Stage"/>.
    /// </summary>
    public static Rehearsal FromStage(Stage stage, int width = 800, int height = 600) =>
        new(stage, width, height);

    // ── Frame simulation ─────────────────────────────────────────────

    /// <summary>Advances the game by one frame: calls Update then Draw.</summary>
    public void RunFrame(float deltaTime = 1f / 60f)
    {
        Stage.Update(deltaTime);
        _canvas.Clear(SKColors.Transparent);
        Stage.Draw(_canvas, _bitmap.Width, _bitmap.Height);
        _frameNumber++;
        _elapsedTime += deltaTime;
    }

    /// <summary>Advances the game by N frames at the given time step.</summary>
    public void RunFrames(int count, float deltaTime = 1f / 60f)
    {
        for (int i = 0; i < count; i++)
            RunFrame(deltaTime);
    }

    /// <summary>Advances the game for the specified duration at the given time step.</summary>
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
    public void PointerMove(float x, float y) => Stage.OnPointerMove(x, y);

    /// <summary>Simulate a pointer/mouse press at (x, y) in game-space coordinates.</summary>
    public void PointerDown(float x, float y) => Stage.OnPointerDown(x, y);

    /// <summary>Simulate a pointer/mouse release at (x, y) in game-space coordinates.</summary>
    public void PointerUp(float x, float y) => Stage.OnPointerUp(x, y);

    /// <summary>Simulate a click: pointer down, optional frames, pointer up.</summary>
    public void Click(float x, float y, int holdFrames = 0, float deltaTime = 1f / 60f)
    {
        PointerDown(x, y);
        RunFrames(holdFrames, deltaTime);
        PointerUp(x, y);
    }

    /// <summary>Simulate a key press.</summary>
    public void KeyDown(string key) => Stage.OnKeyDown(key);

    /// <summary>Simulate a key release.</summary>
    public void KeyUp(string key) => Stage.OnKeyUp(key);

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
        var copy = _bitmap.Copy();
        return new FrameSnapshot(copy, _frameNumber, _elapsedTime);
    }

    /// <summary>Gets the pixel color at the given coordinates from the last rendered frame.</summary>
    public SKColor GetPixel(int x, int y) => _bitmap.GetPixel(x, y);

    /// <summary>Saves the current frame as a PNG for visual debugging.</summary>
    public void SaveFrame(string path)
    {
        using var image = SKImage.FromBitmap(_bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    // ── Screen navigation ────────────────────────────────────────────

    /// <summary>Transitions to a different scene.</summary>
    public void TransitionTo<TScene>(ICurtain? transition = null)
        where TScene : Scene
    {
        var director = Stage.Services.GetRequiredService<IDirector>();
        director.TransitionTo<TScene>(transition);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _canvas.Dispose();
        _bitmap.Dispose();
    }
}