using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharp.Theatre;

/// <summary>
/// A typed collection of game screens backed by the game's <see cref="IServiceCollection"/>.
/// Use <see cref="Add{TScreen}"/> to register each screen as a resolvable transient.
/// To designate which screen is shown first, call
/// <see cref="StageBuilder.SetOpeningScene{TScreen}"/> on the builder.
/// </summary>
/// <remarks>
/// All screens are registered as transients in the game-scoped DI container so that each
/// call to <see cref="IDirector.TransitionTo{TScreen}"/> or
/// <see cref="IDirector.PushOverlay{TOverlay}"/> resolves a fresh instance.
/// </remarks>
public sealed class SceneCollection(IServiceCollection services)
{
    /// <summary>
    /// Registers <typeparamref name="TScreen"/> as a transient in the game's DI container.
    /// Returns <see langword="this"/> for fluent chaining.
    /// </summary>
    public SceneCollection Add<TScreen>() where TScreen : Scene
    {
        services.AddTransient<TScreen>();
        return this;
    }
}
