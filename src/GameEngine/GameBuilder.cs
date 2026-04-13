using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
///        .Add&lt;BreakoutStartScreen&gt;()   // first = initial screen
///        .Add&lt;BreakoutPlayScreen&gt;()
///        .Add&lt;BreakoutGameOverScreen&gt;()
///        .Add&lt;BreakoutVictoryScreen&gt;();
///
/// return builder.Build();
/// </code>
/// </example>
/// </remarks>
public sealed class GameBuilder
{
    private readonly ServiceCollection _serviceCollection = new();

    private GameBuilder()
    {
        Services = _serviceCollection;
        Screens  = new ScreenCollection(_serviceCollection);
        Assets   = new AssetCollection(_serviceCollection);
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
    /// The first screen added via <see cref="ScreenCollection.Add{TScreen}"/> becomes the
    /// initial screen shown when the game starts.
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

    // ── Build ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Finalises configuration, builds the game's DI container, activates the initial screen,
    /// and returns a ready-to-run <see cref="Game"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no screens have been registered via <see cref="Screens"/>.
    /// </exception>
    public Game Build()
    {
        // Build the IConfiguration and make it injectable inside screens.
        var config = Configuration.Build();
        _serviceCollection.AddSingleton<IConfiguration>(config);

        var provider = _serviceCollection.BuildServiceProvider();
        return new Game(provider, Screens.InitialScreenType);
    }
}
