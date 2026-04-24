using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SkiaSharp.Theatre;

/// <summary>
/// Creates and configures a <see cref="Stage"/> instance.
/// Obtain a builder via <see cref="Create"/> then call <see cref="Build"/> to produce
/// a ready-to-run <see cref="Stage"/> that can be assigned to a view.
/// </summary>
/// <remarks>
/// <para>
/// The design mirrors <c>WebApplicationBuilder</c> / <c>MauiAppBuilder</c>:
/// a single entry point (<see cref="Create"/>) sets up sensible defaults, the caller
/// configures scenes, assets, and services via strongly-typed properties, and
/// <see cref="Build"/> seals the configuration and returns the live object.
/// </para>
/// <example>
/// <code>
/// var builder = StageBuilder.Create();
///
/// builder.Services.AddSingleton&lt;BreakoutGameState&gt;();
///
/// builder.Scenes
///        .Add&lt;BreakoutStartScreen&gt;()
///        .Add&lt;BreakoutPlayScreen&gt;()
///        .Add&lt;BreakoutGameOverScreen&gt;()
///        .Add&lt;BreakoutVictoryScreen&gt;();
///
/// builder
///        .SetStageSize(new SKSize(800, 600))
///        .SetOpeningScene&lt;BreakoutStartScreen&gt;();
///
/// return builder.Open();
/// </code>
/// </example>
/// </remarks>
public sealed class StageBuilder
{
    private readonly ServiceCollection _serviceCollection = new();
    private Type? _initialSceneType;
    private SKSize _gameDimensions = new(800, 600);

    private StageBuilder()
    {
        Services = _serviceCollection;
        Scenes = new Repertoire(_serviceCollection);
        Props = new PropRoom(_serviceCollection);
    }

    // ── Public surface ────────────────────────────────────────────────────

    /// <summary>
    /// Stage-scoped service collection, independent of any host DI (e.g. Blazor's container).
    /// Register singletons, transients, and other services here; they will be available for
    /// injection into scene constructors.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Typed collection for registering game scenes.
    /// Internally backed by <see cref="Services"/>.
    /// </summary>
    public Repertoire Scenes { get; }

    /// <summary>
    /// Typed collection for registering game assets (typefaces, bitmaps, etc.).
    /// Internally backed by <see cref="Services"/>.
    /// </summary>
    public PropRoom Props { get; }

    /// <summary>
    /// Configuration builder for the game's <see cref="IConfiguration"/>.
    /// The built configuration is registered in <see cref="Services"/> as
    /// <see cref="IConfiguration"/> so scenes can inject it.
    /// </summary>
    public IConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="StageBuilder"/> with default settings.
    /// This is the recommended entry point, mirroring
    /// <c>WebApplicationBuilder.Create()</c>.
    /// </summary>
    public static StageBuilder Create() => new();

    // ── Stage dimensions ───────────────────────────────────────────────────

    /// <summary>
    /// Sets the logical (virtual) size of the game canvas in game-space units.
    /// This single value is shared across all scenes in the game.
    /// If not called, the default is 800 × 600.
    /// </summary>
    /// <returns>This builder, for method chaining.</returns>
    public StageBuilder SetStageSize(SKSize dimensions)
    {
        if (dimensions.Width <= 0 || dimensions.Height <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(dimensions),
                "Stage dimensions must be positive."
            );
        _gameDimensions = dimensions;
        return this;
    }

    /// <summary>
    /// Sets the logical (virtual) size of the game canvas in game-space units.
    /// This single value is shared across all scenes in the game.
    /// If not called, the default is 800 × 600.
    /// </summary>
    /// <param name="width">Stage-space width in pixels.</param>
    /// <param name="height">Stage-space height in pixels.</param>
    /// <returns>This builder, for method chaining.</returns>
    public StageBuilder SetStageSize(float width, float height) =>
        SetStageSize(new SKSize(width, height));

    // ── Initial scene ────────────────────────────────────────────────────

    /// <summary>
    /// Designates <typeparamref name="TScene"/> as the scene that is activated when the game
    /// starts. The scene must also be registered via <see cref="Repertoire.Add{TScene}"/>.
    /// This must be called exactly once before <see cref="Build"/>.
    /// </summary>
    /// <remarks>
    /// Separating registration (<see cref="Repertoire.Add{TScene}"/>) from initial-scene
    /// selection allows the caller to choose the starting point based on runtime logic —
    /// for example, showing a tutorial for first-time players or jumping straight to the menu
    /// for returning ones.
    /// </remarks>
    /// <returns>This builder, for method chaining.</returns>
    public StageBuilder SetOpeningScene<TScene>()
        where TScene : Scene
    {
        _initialSceneType = typeof(TScene);
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Finalises configuration, builds the game's DI container, activates the initial scene,
    /// and returns a ready-to-run <see cref="Stage"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="SetOpeningScene{TScene}"/> has not been called.
    /// </exception>
    public Stage Open()
    {
        if (_initialSceneType is null)
            throw new InvalidOperationException(
                "No initial scene set. Call SetOpeningScene<T>() before calling Open()."
            );

        // Build the IConfiguration and make it injectable inside scenes.
        var config = Configuration.Build();
        _serviceCollection.AddSingleton<IConfiguration>(config);

        // Bind game options so any service can inject IOptions<StageOptions>.
        _serviceCollection.Configure<StageOptions>(opts =>
        {
            opts.Dimensions = _gameDimensions;
            opts.OpeningSceneType = _initialSceneType;
        });

        // Inspector diagnostic service
        _serviceCollection.AddSingleton<IStageInspector, StageInspector>();

        // Director is the single job that loads and moves between scenes.
        // Register the concrete type and both interfaces so consumers can depend on the
        // narrowest interface they need.
        _serviceCollection.AddSingleton(sp => new Director(
            sp,
            sp.GetRequiredService<IOptions<StageOptions>>()
        ));
        _serviceCollection.AddSingleton<IDirector>(sp => sp.GetRequiredService<Director>());
        _serviceCollection.AddSingleton<IRenderer>(sp => sp.GetRequiredService<Director>());

        // Stage is the public host API; constructed via factory to preserve the internal constructor.
        _serviceCollection.AddSingleton(sp => new Stage(
            sp,
            sp.GetRequiredService<IOptions<StageOptions>>(),
            sp.GetRequiredService<IDirector>(),
            sp.GetRequiredService<IRenderer>()
        ));

        var provider = _serviceCollection.BuildServiceProvider();
        var stage = provider.GetRequiredService<Stage>();
        // Activate the initial scene immediately at build time (not lazily on first frame).
        provider.GetRequiredService<Director>().Initialize(stage);
        return stage;
    }
}