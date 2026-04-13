using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

/// <summary>
/// Factory that builds the self-contained DI container for the Castle Attack game
/// and returns a ready-to-run <see cref="ScreenCoordinator"/>.
/// </summary>
public static class CastleAttackGame
{
    /// <summary>
    /// Creates a fully wired Castle Attack <see cref="ScreenCoordinator"/>, starting on the title screen.
    /// </summary>
    public static ScreenCoordinator Create()
    {
        var services = new ServiceCollection();

        // Shared state
        services.AddSingleton<CastleAttackGameState>();

        // Screens
        services.AddTransient<CastleAttackStartScreen>();
        services.AddTransient<CastleAttackPlayScreen>();
        services.AddTransient<CastleAttackGameOverScreen>();
        services.AddTransient<CastleAttackVictoryScreen>();

        var provider = services.BuildServiceProvider();

        var initialScreen = provider.GetRequiredService<CastleAttackStartScreen>();
        return new ScreenCoordinator(provider, initialScreen);
    }
}
