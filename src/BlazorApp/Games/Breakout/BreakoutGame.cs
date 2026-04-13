using Microsoft.Extensions.DependencyInjection;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>
/// Factory that builds the self-contained DI container for the Breakout game
/// and returns a ready-to-run <see cref="ScreenCoordinator"/>.
/// </summary>
public static class BreakoutGame
{
    /// <summary>
    /// Creates a fully wired Breakout <see cref="ScreenCoordinator"/>, starting on the title screen.
    /// </summary>
    public static ScreenCoordinator Create()
    {
        var services = new ServiceCollection();

        // Shared state threaded through all screens via DI
        services.AddSingleton<BreakoutGameState>();

        // Screens
        services.AddTransient<BreakoutStartScreen>();
        services.AddTransient<BreakoutPlayScreen>();
        services.AddTransient<BreakoutGameOverScreen>();
        services.AddTransient<BreakoutVictoryScreen>();

        var provider = services.BuildServiceProvider();

        var initialScreen = provider.GetRequiredService<BreakoutStartScreen>();
        return new ScreenCoordinator(provider, initialScreen);
    }
}
