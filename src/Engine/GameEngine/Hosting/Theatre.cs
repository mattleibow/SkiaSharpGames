using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SkiaSharp;
using SkiaSharpGames.GameEngine.UI;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Creates and configures a <see cref="Stage"/> instance.
/// Obtain a builder via <see cref="Create"/> then call <see cref="Build"/> to produce
/// a ready-to-run <see cref="Stage"/> that can be assigned to a view.
/// </summary>
/// <remarks>
/// <para>
/// The design mirrors <c>WebApplicationBuilder</c> / <c>MauiAppBuilder</c>:
/// a single entry point (<see cref="Create"/>) sets up sensible defaults, the caller
/// configures screens, assets, and services via strongly-typed properties, and
/// <see cref="Build"/> seals the configuration and returns the live object.
/// </para>
/// <example>
/// <code>
/// var builder = Theatre.Create();
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
public sealed class Theatre
{
    private readonly ServiceCollection _serviceCollection = new();
    private Type? _initialScreenType;
    private SKSize _gameDimensions = new(800, 600);

    private Theatre()
    {
        Services = _serviceCollection;
        Scenes = new SceneCollection(_serviceCollection);
        Props = new PropRoom(_serviceCollection);
    }

    // ── Public surface ────────────────────────────────────────────────────

    /// <summary>
    /// Stage-scoped service collection, independent of any host DI (e.g. Blazor's container).
    /// Register singletons, transients, and other services here; they will be available for
    /// injection into screen constructors.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Typed collection for registering game screens.
    /// Internally backed by <see cref="Services"/>.
    /// </summary>
    public SceneCollection Scenes { get; }

    /// <summary>
    /// Typed collection for registering game assets (typefaces, bitmaps, etc.).
    /// Internally backed by <see cref="Services"/>.
    /// </summary>
    public PropRoom Props { get; }

    /// <summary>
    /// Configuration builder for the game's <see cref="IConfiguration"/>.
    /// The built configuration is registered in <see cref="Services"/> as
    /// <see cref="IConfiguration"/> so screens can inject it.
    /// </summary>
    public IConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Theatre"/> with default settings.
    /// This is the recommended entry point, mirroring
    /// <c>WebApplicationBuilder.Create()</c>.
    /// </summary>
    public static Theatre Create() => new();

    // ── Stage dimensions ───────────────────────────────────────────────────

    /// <summary>
    /// Sets the logical (virtual) size of the game canvas in game-space units.
    /// This single value is shared across all screens in the game.
    /// If not called, the default is 800 × 600.
    /// </summary>
    /// <returns>This builder, for method chaining.</returns>
    public Theatre SetStageSize(SKSize dimensions)
    {
        if (dimensions.Width <= 0 || dimensions.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(dimensions), "Stage dimensions must be positive.");
        _gameDimensions = dimensions;
        return this;
    }

    /// <summary>
    /// Sets the logical (virtual) size of the game canvas in game-space units.
    /// This single value is shared across all screens in the game.
    /// If not called, the default is 800 × 600.
    /// </summary>
    /// <param name="width">Stage-space width in pixels.</param>
    /// <param name="height">Stage-space height in pixels.</param>
    /// <returns>This builder, for method chaining.</returns>
    public Theatre SetStageSize(float width, float height)
        => SetStageSize(new SKSize(width, height));

    // ── Initial screen ────────────────────────────────────────────────────

    /// <summary>
    /// Designates <typeparamref name="TScreen"/> as the screen that is activated when the game
    /// starts. The screen must also be registered via <see cref="SceneCollection.Add{TScreen}"/>.
    /// This must be called exactly once before <see cref="Build"/>.
    /// </summary>
    /// <remarks>
    /// Separating registration (<see cref="SceneCollection.Add{TScreen}"/>) from initial-screen
    /// selection allows the caller to choose the starting point based on runtime logic —
    /// for example, showing a tutorial for first-time players or jumping straight to the menu
    /// for returning ones.
    /// </remarks>
    /// <returns>This builder, for method chaining.</returns>
    public Theatre SetOpeningScene<TScreen>() where TScreen : Scene
    {
        _initialScreenType = typeof(TScreen);
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Finalises configuration, builds the game's DI container, activates the initial screen,
    /// and returns a ready-to-run <see cref="Stage"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="SetOpeningScene{TScreen}"/> has not been called.
    /// </exception>
    public Stage Open()
    {
        if (_initialScreenType is null)
            throw new InvalidOperationException(
                "No initial screen set. Call SetOpeningScene<T>() before calling Open().");

        // Build the IConfiguration and make it injectable inside screens.
        var config = Configuration.Build();
        _serviceCollection.AddSingleton<IConfiguration>(config);
        _serviceCollection.TryAddSingleton(_ => new UiTheme());

        // Bind game options so any service can inject IOptions<StageOptions>.
        _serviceCollection.Configure<StageOptions>(opts =>
        {
            opts.Dimensions = _gameDimensions;
            opts.OpeningSceneType = _initialScreenType;
        });

        // Director is the single job that loads and moves between screens.
        // Register the concrete type and both interfaces so consumers can depend on the
        // narrowest interface they need.
        _serviceCollection.AddSingleton<Director>(
            sp => new Director(sp, sp.GetRequiredService<IOptions<StageOptions>>()));
        _serviceCollection.AddSingleton<IDirector>(
            sp => sp.GetRequiredService<Director>());
        _serviceCollection.AddSingleton<IStageRenderer>(
            sp => sp.GetRequiredService<Director>());

        // Stage is the public host API; constructed via factory to preserve the internal constructor.
        _serviceCollection.AddSingleton<Stage>(
            sp => new Stage(sp,
                           sp.GetRequiredService<IOptions<StageOptions>>(),
                           sp.GetRequiredService<IStageRenderer>()));

        var provider = _serviceCollection.BuildServiceProvider();
        var game = provider.GetRequiredService<Stage>();
        // Activate the initial screen immediately at build time (not lazily on first frame).
        provider.GetRequiredService<Director>().Initialize();
        return game;
    }
}
