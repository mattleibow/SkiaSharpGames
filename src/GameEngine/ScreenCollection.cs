using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A typed collection of game screens backed by the game's <see cref="IServiceCollection"/>.
/// Use <see cref="Add{TScreen}"/> to register each screen as a resolvable transient.
/// To designate which screen is shown first, call
/// <see cref="GameBuilder.SetInitialScreen{TScreen}"/> on the builder.
/// </summary>
/// <remarks>
/// All screens are registered as transients in the game-scoped DI container so that each
/// call to <see cref="Game.TransitionTo{TScreen}"/> or <see cref="Game.PushOverlay{TOverlay}"/>
/// resolves a fresh instance.
/// </remarks>
public sealed class ScreenCollection
{
    private readonly IServiceCollection _services;

    internal ScreenCollection(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers <typeparamref name="TScreen"/> as a transient in the game's DI container.
    /// Returns <see langword="this"/> for fluent chaining.
    /// </summary>
    public ScreenCollection Add<TScreen>() where TScreen : GameScreen
    {
        _services.AddTransient<TScreen>();
        return this;
    }
}
