using SkiaSharp;

namespace SkiaSharpGames.GameEngine.Testing;

/// <summary>
/// A rendered frame captured by <see cref="Rehearsal"/>. Provides pixel-level
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
    /// Counts pixels in the region that match the given color (with optional tolerance).
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
