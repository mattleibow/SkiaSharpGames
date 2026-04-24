using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Diagnostics;

/// <summary>
/// Default implementation of <see cref="IStageInspector"/>. Walks the scene tree to
/// build snapshots and draws a yellow bounding-box overlay on the selected actor.
/// </summary>
public class StageInspector : IStageInspector
{
    private static readonly SKPaint OutlinePaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f,
        Color = SKColors.Yellow,
    };

    private static readonly SKPaint FillPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        Color = SKColors.Yellow.WithAlpha(40),
    };

    private static readonly SKPaint LabelPaint = new()
    {
        IsAntialias = true,
        Color = SKColors.Yellow,
    };

    private static readonly SKFont LabelFont = new(SKTypeface.Default, 11f)
    {
        Edging = SKFontEdging.Antialias,
    };

    /// <inheritdoc/>
    public bool IsEnabled { get; set; }

    /// <inheritdoc/>
    public SceneNode? SelectedNode { get; set; }

    /// <inheritdoc/>
    public void DrawOverlay(SKCanvas canvas, Scene activeScene)
    {
        if (!IsEnabled || SelectedNode is null)
            return;

        if (SelectedNode is Actor actor && actor.WorldBoundingBox is { } bb)
        {
            canvas.DrawRect(bb, FillPaint);
            canvas.DrawRect(bb, OutlinePaint);

            string name = actor.Name ?? actor.GetType().Name;
            canvas.DrawText(name, bb.Left, bb.Top - 4f, LabelFont, LabelPaint);
        }
    }

    /// <inheritdoc/>
    public InspectorSnapshot GetSnapshot(Scene activeScene)
    {
        int count = 0;
        var root = BuildNode(activeScene, SelectedNode, ref count);
        return new InspectorSnapshot(root, count);
    }

    private static string? BuildPreview(SceneNode node, SceneNode? selectedNode)
    {
        if (node != selectedNode || node is not Actor selectedActor)
            return null;

        try
        {
            using var image = selectedActor.CaptureToImage(120, 90);
            if (image is not null)
            {
                using var data = image.Encode(SKEncodedImageFormat.Png, 80);
                return Convert.ToBase64String(data.ToArray());
            }
        }
        catch
        {
            // ignore capture failures
        }

        return null;
    }

    private static InspectorNodeSnapshot BuildNode(SceneNode node, ref int count)
    {
        return BuildNode(node, null, ref count);
    }

    private static InspectorNodeSnapshot BuildNode(
        SceneNode node,
        SceneNode? selectedNode,
        ref int count
    )
    {
        count++;
        var children = new List<InspectorNodeSnapshot>(node.Children.Count);
        for (int i = 0; i < node.Children.Count; i++)
            children.Add(BuildNode(node.Children[i], selectedNode, ref count));

        string displayName =
            node.Name ?? (node is HudLabel label ? label.Text : null) ?? node.GetType().Name;

        float? x = null,
            y = null,
            worldX = null,
            worldY = null,
            alpha = null,
            rotation = null;
        bool? visible = null;
        string? colliderInfo = null,
            velocityInfo = null;

        if (node is Actor actor)
        {
            x = actor.X;
            y = actor.Y;
            worldX = actor.WorldX;
            worldY = actor.WorldY;
            alpha = actor.Alpha;
            visible = actor.Visible;
            rotation = actor.Rotation != 0f ? actor.Rotation : null;

            colliderInfo = actor.Collider switch
            {
                RectCollider rc => $"Rect {rc.Width:F0}×{rc.Height:F0}",
                CircleCollider cc => $"Circle r={cc.Radius:F0}",
                _ => null,
            };

            if (actor.Rigidbody is { } rb && (rb.VelocityX != 0f || rb.VelocityY != 0f))
                velocityInfo = $"({rb.VelocityX:F1}, {rb.VelocityY:F1})";
        }

        var extra = new Dictionary<string, string>();
        switch (node)
        {
            case HudButton btn:
                extra["Label"] = btn.Label;
                extra["IsPressed"] = btn.IsPressed.ToString();
                extra["IsEnabled"] = btn.IsEnabled.ToString();
                if (btn.IsToggle)
                    extra["IsOn"] = btn.IsOn.ToString();
                break;
            case HudLabel lbl:
                extra["Text"] = lbl.Text;
                extra["FontSize"] = lbl.FontSize.ToString("F0");
                break;
            case HudCheckbox cb:
                extra["IsChecked"] = cb.IsChecked.ToString();
                break;
            case HudSwitch sw:
                extra["IsOn"] = sw.IsOn.ToString();
                break;
            case HudSlider sl:
                extra["Value"] = sl.Value.ToString("F2");
                break;
        }

        return new InspectorNodeSnapshot(
            Id: node.GetHashCode(),
            DisplayName: displayName,
            TypeName: node.GetType().Name,
            Active: node.Active,
            Visible: visible,
            X: x,
            Y: y,
            WorldX: worldX,
            WorldY: worldY,
            Alpha: alpha,
            Rotation: rotation,
            ColliderInfo: colliderInfo,
            VelocityInfo: velocityInfo,
            ExtraProperties: extra,
            Children: children
        )
        {
            PreviewImageBase64 = BuildPreview(node, selectedNode),
        };
    }
}
