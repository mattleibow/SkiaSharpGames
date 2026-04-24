namespace SkiaSharp.Theatre;

/// <summary>
/// Options that control how a <see cref="Stage"/> is created and started.
/// Populated by <see cref="StageBuilder"/> via the <c>Microsoft.Extensions.Options</c>
/// infrastructure before the game's DI container is built.
/// </summary>
/// <remarks>
/// Screens and other game services can inject <see cref="Microsoft.Extensions.Options.IOptions{StageOptions}"/>
/// to read these values at runtime.
/// </remarks>
public sealed class StageOptions
{
    /// <summary>
    /// Logical (virtual) dimensions of the game canvas in game-space units.
    /// Set via <see cref="StageBuilder.SetStageSize(SKSize)"/> (or the two-float overload).
    /// Defaults to 800 × 600 if not set.
    /// </summary>
    public SKSize Dimensions { get; set; } = new(800, 600);

    /// <summary>
    /// The <see cref="Scene"/>-derived type to activate when the game starts.
    /// Set via <see cref="StageBuilder.SetOpeningScene{TScene}"/>.
    /// </summary>
    public Type? OpeningSceneType { get; set; }
}