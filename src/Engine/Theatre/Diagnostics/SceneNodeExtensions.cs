using SkiaSharp;

namespace SkiaSharp.Theatre.Diagnostics;

/// <summary>
/// Diagnostic extension methods for scene nodes.
/// </summary>
public static class SceneNodeExtensions
{
    /// <summary>
    /// Renders this node (and its children) into an offscreen image of the given size.
    /// </summary>
    public static SKImage CaptureToImage(this SceneNode node, int width, int height)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        node.Draw(surface.Canvas);
        return surface.Snapshot();
    }
}
