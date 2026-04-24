using SkiaSharp.Theatre;

namespace SkiaSharpGames.BlazorApp;

/// <summary>
/// Simple <see cref="IGalleryGame"/> implementation backed by a factory delegate.
/// </summary>
internal sealed class GalleryGame(
    string slug,
    string title,
    string description,
    string emoji,
    Func<Stage> factory
) : IGalleryGame
{
    public string Slug => slug;
    public string Title => title;
    public string Description => description;
    public string Emoji => emoji;

    public Stage CreateStage() => factory();
}

/// <summary>
/// Registers all gallery games into the DI container.
/// </summary>
public static class GalleryGameRegistration
{
    public static IServiceCollection AddGalleryGames(this IServiceCollection services)
    {
        Register(
            services,
            "breakout",
            "Breakout",
            "🧱",
            "Classic brick-breaker. Move the paddle with mouse or touch to keep the ball in play and clear all the bricks.",
            Breakout.BreakoutGame.Create
        );

        Register(
            services,
            "catch",
            "Catch",
            "🟡",
            "Catch falling circles with a moving bar. Each catch raises your score and speeds up the next drop.",
            Catch.CatchGame.Create
        );

        Register(
            services,
            "castle-attack",
            "Castle Attack",
            "🏰",
            "Defend the castle walls from waves of enemies using archers, workers, and special weapons while the keep is being built.",
            CastleAttack.CastleAttackGame.Create
        );

        Register(
            services,
            "sink-sub",
            "Sink Sub",
            "🚢",
            "Arcade naval survival. Patrol the surface, drop depth charges, and sink submarines before their floating mines reach your ship.",
            SinkSub.SinkSubGame.Create
        );

        Register(
            services,
            "pong",
            "Pong",
            "🏓",
            "Classic 2-player Pong. Use W/S and arrow keys or touch to control paddles. First to 7 points wins!",
            Pong.PongGame.Create
        );

        Register(
            services,
            "2048",
            "2048",
            "🔢",
            "Slide numbered tiles on a grid. Combine matching tiles to reach 2048!",
            TwoZeroFourEight.TwoZeroFourEightGame.Create
        );

        Register(
            services,
            "space-invaders",
            "Space Invaders",
            "👾",
            "Classic alien invasion. Move your cannon and fire to destroy waves of descending invaders before they reach the ground.",
            SpaceInvaders.SpaceInvadersGame.Create
        );

        Register(
            services,
            "snake",
            "Snake",
            "🐍",
            "Guide the snake to eat food and grow longer. Don't crash into the walls or your own tail!",
            Snake.SnakeGame.Create
        );

        Register(
            services,
            "lunar-lander",
            "Lunar Lander",
            "🚀",
            "Guide your lander safely to the pad. Use thrust and rotation to control descent — land gently or crash!",
            LunarLander.LunarLanderGame.Create
        );

        Register(
            services,
            "asteroids",
            "Asteroids",
            "☄️",
            "Pilot your ship through an asteroid field. Rotate, thrust, and shoot to survive wave after wave of space rocks.",
            Asteroids.AsteroidsGame.Create
        );

        Register(
            services,
            "ui-gallery",
            "UI Gallery",
            "🧩",
            "Try shared game UI controls with global themes, per-control overrides, and custom canvas drawing hooks.",
            UIGallery.UIGalleryGame.Create
        );

        return services;
    }

    private static void Register(
        IServiceCollection services,
        string slug,
        string title,
        string emoji,
        string description,
        Func<Stage> factory
    )
    {
        var game = new GalleryGame(slug, title, description, emoji, factory);
        services.AddSingleton<IGalleryGame>(game);
    }
}
