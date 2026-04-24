using SkiaSharp;

namespace SkiaSharpGames.Asteroids;

internal static class AsteroidsConstants
{
    public const int GameWidth = 800;
    public const int GameHeight = 600;

    // Ship
    public const float ShipRadius = 14f;
    public const float ShipThrustAccel = 280f;
    public const float ShipMaxSpeed = 360f;
    public const float ShipDrag = 0.98f;
    public const float ShipRotationSpeed = 5.0f;
    public const float ShipInvincibleDuration = 2.5f;

    // Bullets
    public const float BulletSpeed = 450f;
    public const float BulletLifetime = 1.2f;
    public const float BulletRadius = 2.5f;
    public const float FireCooldown = 0.15f;
    public const int MaxBullets = 6;

    // Asteroids
    public const float AsteroidLargeRadius = 40f;
    public const float AsteroidMediumRadius = 22f;
    public const float AsteroidSmallRadius = 11f;
    public const float AsteroidLargeMinSpeed = 40f;
    public const float AsteroidLargeMaxSpeed = 80f;
    public const float AsteroidMediumMinSpeed = 60f;
    public const float AsteroidMediumMaxSpeed = 120f;
    public const float AsteroidSmallMinSpeed = 90f;
    public const float AsteroidSmallMaxSpeed = 180f;
    public const int AsteroidLargeScore = 20;
    public const int AsteroidMediumScore = 50;
    public const int AsteroidSmallScore = 100;
    public const int InitialAsteroidCount = 4;
    public const int AsteroidVertices = 10;

    // Debris
    public const float DebrisLifetime = 0.8f;
    public const float DebrisSpeed = 120f;
    public const int DebrisCount = 6;

    // Lives
    public const int InitialLives = 3;
    public const int ExtraLifeScore = 10000;

    // Colors
    public static readonly SKColor BackgroundColor = new(0x02, 0x02, 0x0A);
    public static readonly SKColor ShipColor = new(0xE0, 0xE8, 0xFF);
    public static readonly SKColor ThrustColor = new(0xFF, 0x88, 0x22);
    public static readonly SKColor AsteroidColor = new(0x99, 0xA0, 0xAA);
    public static readonly SKColor BulletColor = new(0xFF, 0xFF, 0xFF);
    public static readonly SKColor AccentColor = new(0x55, 0xCC, 0xFF);
    public static readonly SKColor HudDimColor = new(0x88, 0x99, 0xAA);
    public static readonly SKColor DebrisColor = new(0xCC, 0xCC, 0xCC);
}