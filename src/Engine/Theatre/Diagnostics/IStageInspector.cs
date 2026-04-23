namespace SkiaSharp.Theatre;

/// <summary>
/// Diagnostic service that provides a live view of the scene tree and an optional
/// overlay highlight on the selected node. Registered as a singleton in the game's
/// DI container by <see cref="StageBuilder"/>.
/// </summary>
public interface IStageInspector
{
    /// <summary>When true, the inspector overlay is drawn and snapshots are produced.</summary>
    bool IsEnabled { get; set; }

    /// <summary>The currently selected node (highlighted in the overlay).</summary>
    SceneNode? SelectedNode { get; set; }

    /// <summary>Draws the selection overlay on top of the scene.</summary>
    void DrawOverlay(SKCanvas canvas, Scene activeScene);

    /// <summary>Builds a serialisable snapshot of the active scene tree.</summary>
    InspectorSnapshot GetSnapshot(Scene activeScene);
}
