using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

/// <summary>
/// The Castle Attack game. Register as a transient service and inject into the game page:
/// <code>builder.Services.AddTransient&lt;CastleAttackGame&gt;();</code>
/// </summary>
public sealed class CastleAttackGame : Game
{
    protected override void Configure(GameBuilder builder)
    {
        builder.Services.AddSingleton<CastleAttackGameState>();

        builder.AddScreen<CastleAttackStartScreen>()
               .AddScreen<CastleAttackPlayScreen>()
               .AddScreen<CastleAttackGameOverScreen>()
               .AddScreen<CastleAttackVictoryScreen>();
    }
}
