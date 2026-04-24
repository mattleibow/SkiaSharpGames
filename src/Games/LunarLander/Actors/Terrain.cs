using SkiaSharp;

using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>
/// Procedurally generated terrain with one flat landing pad section.
/// Stores terrain as a list of points from left to right.
/// </summary>
internal sealed class Terrain
{
    private readonly SKPaint _terrainPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        Color = TerrainColor,
    };
    private readonly SKPaint _padPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        Color = PadColor,
    };

    public List<SKPoint> Points { get; } = [];

    /// <summary>X centre of the landing pad.</summary>
    public float PadCenterX { get; private set; }

    /// <summary>Y position (top) of the landing pad.</summary>
    public float PadY { get; private set; }

    public void Generate(Random rng)
    {
        Points.Clear();

        float segWidth = (float)GameWidth / TerrainSegments;

        // Choose a random segment index for the landing pad (avoid edges)
        int padSegIndex = rng.Next(4, TerrainSegments - 4);
        int padSegCount = (int)MathF.Ceiling(LandingPadWidth / segWidth);

        for (int i = 0; i <= TerrainSegments; i++)
        {
            float x = i * segWidth;
            float y;

            if (i >= padSegIndex && i <= padSegIndex + padSegCount)
            {
                // Flat landing pad section
                y = TerrainMinY + (TerrainMaxY - TerrainMinY) * 0.6f;
            }
            else
            {
                y = TerrainMinY + (float)rng.NextDouble() * (TerrainMaxY - TerrainMinY);
            }

            Points.Add(new SKPoint(x, y));
        }

        // Record pad position
        float padStartX = padSegIndex * segWidth;
        float padEndX = (padSegIndex + padSegCount) * segWidth;
        PadCenterX = (padStartX + padEndX) / 2f;
        PadY = Points[padSegIndex].Y;
    }

    /// <summary>Returns the terrain height (Y) at a given X position via linear interpolation.</summary>
    public float GetHeightAt(float x)
    {
        if (Points.Count < 2)
            return GameHeight;

        float segWidth = (float)GameWidth / TerrainSegments;
        int idx = Math.Clamp((int)(x / segWidth), 0, Points.Count - 2);
        float t = (x - Points[idx].X) / segWidth;
        return Points[idx].Y + t * (Points[idx + 1].Y - Points[idx].Y);
    }

    /// <summary>Returns true if the given X is over the landing pad.</summary>
    public bool IsOverPad(float x)
    {
        float halfPad = LandingPadWidth / 2f;
        return x >= PadCenterX - halfPad && x <= PadCenterX + halfPad;
    }

    public void Draw(SKCanvas canvas)
    {
        if (Points.Count < 2)
            return;

        // Draw filled terrain
        using var terrainPath = new SKPath();
        terrainPath.MoveTo(0, GameHeight);
        foreach (var pt in Points)
            terrainPath.LineTo(pt.X, pt.Y);
        terrainPath.LineTo(GameWidth, GameHeight);
        terrainPath.Close();
        canvas.DrawPath(terrainPath, _terrainPaint);

        // Draw landing pad on top
        float padLeft = PadCenterX - LandingPadWidth / 2f;
        canvas.DrawRect(
            padLeft,
            PadY - LandingPadHeight,
            LandingPadWidth,
            LandingPadHeight,
            _padPaint
        );
    }
}
