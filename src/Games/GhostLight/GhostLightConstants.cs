using SkiaSharp;

namespace SkiaSharpGames.GhostLight;

internal static class GhostLightConstants
{
    public const float GameWidth = 800f;
    public const float GameHeight = 600f;

    // Player
    public const float PlayerRadius = 12f;
    public const float PlayerGlowRadius = 80f;
    public const float PlayerSpeed = 120f;
    public static readonly SKColor PlayerCoreColor = new(0xAE, 0xEF, 0xFF);

    // Enemies
    public const float EnemySpeed = 40f;
    public const float EnemyMinRadius = 18f;
    public const float EnemyMaxRadius = 30f;

    // Darkness / fog
    public const float DarknessAlpha = 0.65f;
    public const float PauseOverlayAlpha = 0.7f;

    // Spawning
    public const int MaxEnemies = 12;
    public const float EnemySpawnInterval = 2f;
}
