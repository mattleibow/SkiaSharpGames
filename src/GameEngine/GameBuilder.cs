using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Fluent builder used inside <see cref="Game.Configure"/> to register screens and
/// game-scoped services before the game starts.
/// </summary>
/// <remarks>
/// Instances are created internally by <see cref="Game"/>. The first screen registered
/// via <see cref="AddScreen{TScreen}"/> becomes the initial (start) screen.
/// </remarks>
public sealed class GameBuilder
{
    private readonly ServiceCollection _services = new();
    private Type? _initialScreenType;

    internal GameBuilder() { }

    /// <summary>
    /// The game-scoped service collection.
    /// Register singletons, transients, and other services here.
    /// This container is fully independent of any host DI (e.g. Blazor's DI).
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// Registers a screen type with the game's DI container.
    /// The first call also marks the screen as the initial screen shown when the game starts.
    /// Returns <see langword="this"/> for fluent chaining.
    /// </summary>
    public GameBuilder AddScreen<TScreen>() where TScreen : GameScreenBase
    {
        _services.AddTransient<TScreen>();
        _initialScreenType ??= typeof(TScreen);
        return this;
    }

    // ── Internal use by Game ──────────────────────────────────────────────

    internal Type InitialScreenType =>
        _initialScreenType
        ?? throw new InvalidOperationException(
            "No screens registered. Call AddScreen<T>() inside Configure().");

    internal IServiceProvider BuildServiceProvider() => _services.BuildServiceProvider();
}
