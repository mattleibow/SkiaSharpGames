namespace SkiaSharp.Theatre.Diagnostics;

/// <summary>Immutable snapshot of a single node in the scene tree.</summary>
public record InspectorNodeSnapshot(
    int Id,
    string DisplayName,
    string TypeName,
    bool Active,
    bool? Visible,
    float? X,
    float? Y,
    float? WorldX,
    float? WorldY,
    float? Alpha,
    float? Rotation,
    string? ColliderInfo,
    string? VelocityInfo,
    Dictionary<string, string> ExtraProperties,
    List<InspectorNodeSnapshot> Children
)
{
    /// <summary>
    /// Base64-encoded PNG preview of this node, or null if not available.
    /// Set by the inspector when generating snapshots.
    /// </summary>
    public string? PreviewImageBase64 { get; init; }
};

/// <summary>Full tree snapshot returned by <see cref="IStageInspector.GetSnapshot"/>.</summary>
public record InspectorSnapshot(InspectorNodeSnapshot Root, int TotalNodes);
