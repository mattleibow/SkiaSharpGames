using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharp.Theatre;

/// <summary>
/// A typed collection of game scenes backed by the game's <see cref="IServiceCollection"/>.
/// Use <see cref="Add{TScene}"/> to register each scene as a resolvable transient.
/// To designate which scene is shown first, call
/// <see cref="StageBuilder.SetOpeningScene{TScene}"/> on the builder.
/// </summary>
/// <remarks>
/// All scenes are registered as transients in the game-scoped DI container so that each
/// call to <see cref="IDirector.TransitionTo{TScene}"/> or
/// <see cref="IDirector.PushScene{TOverlay}"/> resolves a fresh instance.
/// </remarks>
public sealed class Repertoire(IServiceCollection services)
{
    /// <summary>
    /// Registers <typeparamref name="TScene"/> as a transient in the game's DI container.
    /// Returns <see langword="this"/> for fluent chaining.
    /// </summary>
    public Repertoire Add<TScene>()
        where TScene : Scene
    {
        services.AddTransient<TScene>();
        return this;
    }
}
