using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SkiaSharp;
using SkiaSharpGames.GameEngine.UI;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Creates and configures a <see cref="Game"/> instance.
/// Obtain a builder via <see cref="CreateDefault"/> then call <see cref="Build"/> to produce
/// a ready-to-run <see cref="Game"/> that can be assigned to a view.
/// </summary>
/// <remarks>
/// <para>
/// The design mirrors <c>WebApplicationBuilder</c> / <c>MauiAppBuilder</c>:
/// a single entry point (<see cref="CreateDefault"/>) sets up sensible defaults, the caller
/// configures screens, assets, and services via strongly-typed properties, and
/// <see cref="Build"/> seals the configuration and returns the live object.
/// </para>
/// <example>
/// <code>
/// var builder = GameBuilder.CreateDefault();
///
/// builder.Services.AddSingleton&lt;BreakoutGameState&gt;();
///
/// builder.Screens
///        .Add&lt;BreakoutStartScreen&gt;()
///        .Add&lt;BreakoutPlayScreen&gt;()
///        .Add&lt;BreakoutGameOverScreen&gt;()
///        .Add&lt;BreakoutVictoryScreen&gt;();
///
/// builder
///        .SetGameDimensions(new SKSize(800, 600))
///        .SetInitialScreen&lt;BreakoutStartScreen&gt;();
///
/// return builder.Build();
/// </code>
/// </example>
/// </remarks>
public sealed class GameBuilder
{
    private readonly ServiceCollection _serviceCollection = new();
    private Type? _initialScreenType;
    private SKSize _gameDimensions = new(800, 600);

    private GameBuilder()
    {
        Services = _serviceCollection;
        Screens = new ScreenCollection(_serviceCollection);
        Assets = new AssetCollection(_serviceCollection);
    }

    // ── Public surface ────────────────────────────────────────────────────

    /// <summary>
    /// Game-scoped service collection, independent of any host DI (e.g. Blazor's container).
    /// Register singletons, transients, and other services here; they will be available for
    /// injection into screen constructors.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Typed collection for registering game screens.
    /// Internally backed by <see cref="Services"/>.
    /// </summary>
    public ScreenCollection Screens { get; }

    /// <summary>
    /// Typed collection for registering game assets (typefaces, bitmaps, etc.).
    /// Internally backed by <see cref="Services"/>.
    /// </summary>
    public AssetCollection Assets { get; }

    /// <summary>
    /// Configuration builder for the game's <see cref="IConfiguration"/>.
    /// The built configuration is registered in <see cref="Services"/> as
    /// <see cref="IConfiguration"/> so screens can inject it.
    /// </summary>
    public IConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="GameBuilder"/> with default settings.
    /// This is the recommended entry point, mirroring
    /// <c>WebApplicationBuilder.CreateDefault()</c>.
    /// </summary>
    public static GameBuilder CreateDefault() => new();

    // ── Game dimensions ───────────────────────────────────────────────────

    /// <summary>
    /// Sets the logical (virtual) size of the game canvas in game-space units.
    /// This single value is shared across all screens in the game.
    /// If not called, the default is 800 × 600.
    /// </summary>
    /// <returns>This builder, for method chaining.</returns>
    public GameBuilder SetGameDimensions(SKSize dimensions)
    {
        _gameDimensions = dimensions;
        return this;
    }

    /// <summary>
    /// Sets the logical (virtual) size of the game canvas in game-space units.
    /// This single value is shared across all screens in the game.
    /// If not called, the default is 800 × 600.
    /// </summary>
    /// <param name="width">Game-space width in pixels.</param>
    /// <param name="height">Game-space height in pixels.</param>
    /// <returns>This builder, for method chaining.</returns>
    public GameBuilder SetGameDimensions(float width, float height)
        => SetGameDimensions(new SKSize(width, height));

    // ── Initial screen ────────────────────────────────────────────────────

    /// <summary>
    /// Designates <typeparamref name="TScreen"/> as the screen that is activated when the game
    /// starts. The screen must also be registered via <see cref="ScreenCollection.Add{TScreen}"/>.
    /// This must be called exactly once before <see cref="Build"/>.
    /// </summary>
    /// <remarks>
    /// Separating registration (<see cref="ScreenCollection.Add{TScreen}"/>) from initial-screen
    /// selection allows the caller to choose the starting point based on runtime logic —
    /// for example, showing a tutorial for first-time players or jumping straight to the menu
    /// for returning ones.
    /// </remarks>
    /// <returns>This builder, for method chaining.</returns>
    public GameBuilder SetInitialScreen<TScreen>() where TScreen : GameScreen
    {
        _initialScreenType = typeof(TScreen);
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Finalises configuration, builds the game's DI container, activates the initial screen,
    /// and returns a ready-to-run <see cref="Game"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="SetInitialScreen{TScreen}"/> has not been called.
    /// </exception>
    public Game Build()
    {
        if (_initialScreenType is null)
            throw new InvalidOperationException(
                "No initial screen set. Call SetInitialScreen<T>() before calling Build().");

        // Build the IConfiguration and make it injectable inside screens.
        var config = Configuration.Build();
        _serviceCollection.AddSingleton<IConfiguration>(config);
        _serviceCollection.TryAddSingleton<IUiThemeProvider>(_ => new UiThemeProvider(UiThemes.Simple));

        // Bind game options so any service can inject IOptions<GameOptions>.
        _serviceCollection.Configure<GameOptions>(opts =>
        {
            opts.Dimensions = _gameDimensions;
            opts.InitialScreenType = _initialScreenType;
        });

        // ScreenCoordinator is the single job that loads and moves between screens.
        // Register the concrete type and both interfaces so consumers can depend on the
        // narrowest interface they need.
        _serviceCollection.AddSingleton<ScreenCoordinator>(
            sp => new ScreenCoordinator(sp, sp.GetRequiredService<IOptions<GameOptions>>()));
        _serviceCollection.AddSingleton<IScreenCoordinator>(
            sp => sp.GetRequiredService<ScreenCoordinator>());
        _serviceCollection.AddSingleton<IScreenDrawable>(
            sp => sp.GetRequiredService<ScreenCoordinator>());

        // Game is the public host API; constructed via factory to preserve the internal constructor.
        _serviceCollection.AddSingleton<Game>(
            sp => new Game(sp,
                           sp.GetRequiredService<IOptions<GameOptions>>(),
                           sp.GetRequiredService<IScreenDrawable>()));

        var provider = _serviceCollection.BuildServiceProvider();
        var game = provider.GetRequiredService<Game>();
        // Activate the initial screen immediately at build time (not lazily on first frame).
        provider.GetRequiredService<ScreenCoordinator>().Initialize();
        return game;
    }
}
