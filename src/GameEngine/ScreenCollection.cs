using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A typed collection of game screens backed by the game's <see cref="IServiceCollection"/>.
/// Use <see cref="Add{TScreen}"/> to register each screen; the first registration becomes the
/// initial screen shown when the game starts.
/// </summary>
/// <remarks>
/// All screens are registered as transients in the game-scoped DI container so that each
/// call to <see cref="Game.TransitionTo{TScreen}"/> or <see cref="Game.PushOverlay{TOverlay}"/>
/// resolves a fresh instance.
/// </remarks>
public sealed class ScreenCollection
{
    private readonly IServiceCollection _services;
    private Type? _initialScreenType;

    internal ScreenCollection(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers <typeparamref name="TScreen"/> as a transient in the game's DI container.
    /// The first call also designates this screen as the one shown when the game starts.
    /// Returns <see langword="this"/> for fluent chaining.
    /// </summary>
    public ScreenCollection Add<TScreen>() where TScreen : GameScreenBase
    {
        _services.AddTransient<TScreen>();
        _initialScreenType ??= typeof(TScreen);
        return this;
    }

    internal Type InitialScreenType =>
        _initialScreenType
        ?? throw new InvalidOperationException(
            "No screens registered. Call Screens.Add<T>() before calling Build().");
}
