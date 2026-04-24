using SkiaSharp;

namespace SkiaSharpGames.Starfall;

internal static class StarfallConstants
{
    // Game dimensions
    public const int GameWidth = 800;
    public const int GameHeight = 600;

    // Player
    public const float PlayerSpeed = 8f; // lerp factor per frame
    public const float PlayerBaseFireRate = 0.18f;
    public const int PlayerBaseHP = 3;
    public const int PlayerMaxHP = 8;
    public const float PlayerBulletSpeed = 500f;
    public const float PlayerBulletDamage = 1;
    public const float PlayerShipRadius = 14f;
    public const float PlayerInvincibleDuration = 1.5f;
    public const int PlayerBaseBombs = 1;
    public const float BombFlashDuration = 0.4f;

    // Enemies — general
    public const float EnemySpawnMargin = 40f;
    public const float PowerUpDropChance = 0.15f;
    public const float PowerUpSpeed = 80f;
    public const float PowerUpRadius = 12f;
    public const float PowerUpDuration = 8f;
    public const float ShieldDuration = 3f;

    // Drone
    public const float DroneSpeed = 120f;
    public const float DroneHP = 1;
    public const int DroneScore = 50;
    public const float DroneRadius = 10f;

    // Zigzagger
    public const float ZigzaggerSpeed = 100f;
    public const float ZigzaggerAmplitude = 80f;
    public const float ZigzaggerFrequency = 3f;
    public const float ZigzaggerHP = 2;
    public const int ZigzaggerScore = 75;
    public const float ZigzaggerRadius = 12f;

    // Shooter
    public const float ShooterSpeed = 70f;
    public const float ShooterHP = 3;
    public const int ShooterScore = 100;
    public const float ShooterRadius = 14f;
    public const float ShooterFireRate = 1.5f;
    public const float EnemyBulletSpeed = 250f;
    public const float EnemyBulletRadius = 4f;

    // Bomber
    public const float BomberSpeed = 50f;
    public const float BomberHP = 5;
    public const int BomberScore = 150;
    public const float BomberRadius = 18f;
    public const float BomberShockwaveRate = 2.5f;
    public const float ShockwaveSpeed = 100f;
    public const float ShockwaveMaxRadius = 60f;

    // Charger
    public const float ChargerStalkSpeed = 40f;
    public const float ChargerRushSpeed = 400f;
    public const float ChargerHP = 2;
    public const int ChargerScore = 120;
    public const float ChargerRadius = 11f;
    public const float ChargerLockOnTime = 1.5f;

    // Asteroid
    public const float AsteroidMinSpeed = 60f;
    public const float AsteroidMaxSpeed = 130f;
    public const float AsteroidLargeRadius = 25f;
    public const float AsteroidSmallRadius = 12f;
    public const float AsteroidHP = 2;
    public const int AsteroidScore = 30;

    // Boss general
    public const float BossBarHeight = 8f;
    public const float BossBarY = 20f;
    public const float BossBarWidth = 400f;

    // Boss Stage 1 — The Colossus
    public const float ColossusHP = 40;
    public const float ColossusRadius = 50f;
    public const float ColossusSpeed = 60f;
    public const int ColossusScore = 1000;

    // Boss Stage 2 — The Marauder
    public const float MarauderHP = 60;
    public const float MarauderWidth = 100f;
    public const float MarauderHeight = 60f;
    public const float MarauderSpeed = 80f;
    public const int MarauderScore = 2000;

    // Boss Stage 3 — The Dreadnought
    public const float DreadnoughtHP = 100;
    public const float DreadnoughtWidth = 140f;
    public const float DreadnoughtHeight = 80f;
    public const float DreadnoughtSpeed = 50f;
    public const int DreadnoughtScore = 5000;

    // Particles
    public const float ParticleLifetime = 0.6f;
    public const float ParticleSpeed = 200f;
    public const int ExplosionParticleCount = 12;
    public const int SmallExplosionParticleCount = 6;

    // Floating text
    public const float FloatingTextSpeed = 60f;
    public const float FloatingTextLifetime = 0.8f;

    // Wave timing
    public const float WaveDelay = 1.5f;
    public const float BossDelay = 2.0f;

    // Upgrade screen
    public const int UpgradeChoices = 3;

    // ── Colors ──────────────────────────────────────────
    public static readonly SKColor BackgroundColor = new(0x05, 0x05, 0x18);
    public static readonly SKColor StarDim = new(0xFF, 0xFF, 0xFF, 40);
    public static readonly SKColor StarMedium = new(0xFF, 0xFF, 0xFF, 80);
    public static readonly SKColor StarBright = new(0xFF, 0xFF, 0xFF, 160);

    // Neon accent palette
    public static readonly SKColor CyanAccent = new(0x00, 0xE5, 0xFF);
    public static readonly SKColor MagentaAccent = new(0xFF, 0x00, 0xE5);
    public static readonly SKColor GreenAccent = new(0x00, 0xFF, 0x88);
    public static readonly SKColor YellowAccent = new(0xFF, 0xE5, 0x00);
    public static readonly SKColor RedAccent = new(0xFF, 0x44, 0x44);
    public static readonly SKColor OrangeAccent = new(0xFF, 0x99, 0x22);
    public static readonly SKColor WhiteColor = new(0xEE, 0xEE, 0xFF);
    public static readonly SKColor HudDimColor = new(0x88, 0x99, 0xBB);

    // Entity colors
    public static readonly SKColor PlayerColor = CyanAccent;
    public static readonly SKColor PlayerBulletColor = new(0xAA, 0xFF, 0xFF);
    public static readonly SKColor PlayerShieldColor = new(0x00, 0xE5, 0xFF, 100);
    public static readonly SKColor DroneColor = new(0xFF, 0x77, 0x77);
    public static readonly SKColor ZigzaggerColor = new(0xFF, 0xAA, 0x33);
    public static readonly SKColor ShooterColor = new(0xFF, 0x55, 0xBB);
    public static readonly SKColor BomberColor = new(0xFF, 0x33, 0x33);
    public static readonly SKColor ChargerColor = new(0xCC, 0x33, 0xFF);
    public static readonly SKColor EnemyBulletColor = new(0xFF, 0x66, 0x66);
    public static readonly SKColor AsteroidColor = new(0x88, 0x88, 0x99);
    public static readonly SKColor BossColor = new(0xFF, 0xCC, 0x00);
    public static readonly SKColor ShockwaveColor = new(0xFF, 0x44, 0x00, 120);
    public static readonly SKColor PowerUpHealthColor = GreenAccent;
    public static readonly SKColor PowerUpRapidColor = YellowAccent;
    public static readonly SKColor PowerUpSpreadColor = CyanAccent;
    public static readonly SKColor PowerUpBombColor = MagentaAccent;
    public static readonly SKColor PowerUpShieldColor = new(0x88, 0xCC, 0xFF);
    public static readonly SKColor ExplosionColor = new(0xFF, 0xBB, 0x44);
    public static readonly SKColor BombFlashColor = new(0xFF, 0xFF, 0xFF, 200);
}
