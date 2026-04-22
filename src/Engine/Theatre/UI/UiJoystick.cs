using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// A themed virtual joystick actor with built-in collision, sprite, and readable state.
/// <para>
/// Drag inside the joystick to move the knob. Read <see cref="Delta"/> for the
/// offset, or <see cref="NormalizedDelta"/> for the –1 … 1 range.
/// Call <see cref="UpdateFromPointer"/> during pointer-move, and reset via
/// <see cref="ResetDelta"/> on pointer-up.
/// </para>
/// <example><code>
/// var joystick = new UiJoystick(86f, theme);
/// joystick.X = 620f; joystick.Y = 360f;
/// controls.AddChild(joystick);
///
/// // On pointer move while dragging:
/// joystick.UpdateFromPointer(worldX, worldY);
///
/// // On pointer up:
/// joystick.ResetDelta();
///
/// // Read state:
/// float nx = joystick.NormalizedDelta.X; // -1..1
/// float ny = joystick.NormalizedDelta.Y; // -1..1
/// </code></example>
/// </summary>
public class UiJoystick : UiActor
{
    /// <summary>
    /// Creates a new joystick actor with the given radius and theme.
    /// </summary>
    /// <param name="radius">Base circle radius in game-space units.</param>
    /// <param name="theme">Provides the active UI theme for rendering.</param>
    public UiJoystick(float radius, UiTheme theme) : base(theme)
    {
        Radius = radius;
        Collider = new CircleCollider { Radius = radius };
    }

    /// <summary>Base radius of the joystick in game-space units.</summary>
    public float Radius { get; }

    /// <summary>
    /// Fraction of <see cref="Radius"/> that the knob can travel (0 – 1).
    /// Defaults to 0.6 (60 %).
    /// </summary>
    public float MaxRadiusFraction { get; set; } = 0.6f;

    /// <summary>
    /// Raw knob offset from the center, in game-space units. Clamped to
    /// <see cref="Radius"/> × <see cref="MaxRadiusFraction"/>.
    /// </summary>
    public SKPoint Delta { get; set; }

    /// <summary>
    /// Knob offset normalised to the –1 … 1 range on each axis.
    /// </summary>
    public SKPoint NormalizedDelta
    {
        get
        {
            float maxR = Radius * MaxRadiusFraction;
            return maxR <= 0f ? SKPoint.Empty : new SKPoint(Delta.X / maxR, Delta.Y / maxR);
        }
    }

    /// <summary>
    /// Optional per-joystick appearance override. When null, uses the theme's default.
    /// </summary>
    public UiAppearance<UiJoystick>? Appearance { get; set; }

    /// <summary>
    /// Clamps a joystick delta vector to the given maximum radius.
    /// </summary>
    public static SKPoint ClampJoystick(SKPoint delta, float maxRadius)
    {
        float length = MathF.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        if (length <= maxRadius || length <= 0.0001f)
            return delta;

        float scale = maxRadius / length;
        return new SKPoint(delta.X * scale, delta.Y * scale);
    }

    /// <summary>
    /// Updates <see cref="Delta"/> based on a world-space pointer position.
    /// The delta is clamped to the maximum travel distance.
    /// </summary>
    /// <param name="worldX">The pointer's X position in world space.</param>
    /// <param name="worldY">The pointer's Y position in world space.</param>
    public void UpdateFromPointer(float worldX, float worldY)
    {
        Delta = ClampJoystick(
            new SKPoint(worldX - WorldX, worldY - WorldY),
            Radius * MaxRadiusFraction);
    }

    /// <summary>Resets the knob to the center position.</summary>
    public void ResetDelta() => Delta = SKPoint.Empty;

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme!.Joystick).Draw(canvas, this);
    }
}
