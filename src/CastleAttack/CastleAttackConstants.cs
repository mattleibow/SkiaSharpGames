using SkiaSharp;

namespace SkiaSharpGames.CastleAttack;

/// <summary>All layout, physics, and colour constants for the Castle Attack game.</summary>
internal static class CastleAttackConstants
{
    // ── Game dimensions ───────────────────────────────────────────────────
    public const int GameWidth = 1200;
    public const int GameHeight = 600;

    // ── Layout ────────────────────────────────────────────────────────────
    public const float GroundY = 548f;
    public const float BlockW = 40f;
    public const float BlockH = 28f;

    public const float OuterWallX = 430f;
    public const float MiddleWallX = 330f;
    public const float InnerWallX = 230f;

    public const float KeepLeft = 10f;
    public const float KeepRight = 200f;
    public const float KeepFullH = 200f;
    public const float KeepBaseY = GroundY;

    public const float LordStandX = KeepRight + 8f;
    public const float LordW = 14f;
    public const float LordH = 28f;

    public const float SpawnX = 1185f;

    // ── Physics ───────────────────────────────────────────────────────────
    public const float ArrowSpeed = 580f;
    public const float ArrowGravity = 480f;

    public const float WorkerBuildRate = 1f / 110f;

    public const float LordMeleeRange = 30f;
    public const float LordAttackDamage = 2f;
    public const float LordAttackInterval = 0.6f;

    // ── Units ─────────────────────────────────────────────────────────────
    public const float ArcherH = 26f;
    public const float ArcherW = 12f;
    public const float WorkerH = 22f;
    public const float WorkerW = 10f;

    // ── Aim ───────────────────────────────────────────────────────────────
    public const float AimDefault = 18f;
    public const float AimMin = -8f;
    public const float AimMax = 62f;
    public const float AimSpeed = 50f;
    public const float AimStep = 10f;

    // ── Arrows ────────────────────────────────────────────────────────────
    public const float ArrowCooldownTime = 0.75f;

    // ── Buttons ───────────────────────────────────────────────────────────
    public const float BtnY = 560f;
    public const float BtnH = 36f;
    public const float BtnR = 6f;

    public static readonly SKRect BtnAimLeft = SKRect.Create(8f, BtnY, 98f, BtnH);
    public static readonly SKRect BtnAimRight = SKRect.Create(112f, BtnY, 98f, BtnH);
    public static readonly SKRect BtnA2W = SKRect.Create(216f, BtnY, 116f, BtnH);
    public static readonly SKRect BtnW2A = SKRect.Create(338f, BtnY, 116f, BtnH);
    public static readonly SKRect BtnFire = SKRect.Create(470f, BtnY, 260f, BtnH);
    public static readonly SKRect BtnOil = SKRect.Create(750f, BtnY, 140f, BtnH);
    public static readonly SKRect BtnCannon = SKRect.Create(898f, BtnY, 148f, BtnH);
    public static readonly SKRect BtnLogs = SKRect.Create(1054f, BtnY, 140f, BtnH);

    // ── Colours ───────────────────────────────────────────────────────────
    public static readonly SKColor ColSky = new(0x1A, 0x1A, 0x30);
    public static readonly SKColor ColHorizon = new(0x3A, 0x2E, 0x20);
    public static readonly SKColor ColGround = new(0x55, 0x44, 0x22);
    public static readonly SKColor ColGroundEdge = new(0x44, 0x34, 0x16);
    public static readonly SKColor ColStone = new(0x88, 0x88, 0x88);
    public static readonly SKColor ColStoneDmg = new(0xBB, 0x66, 0x44);
    public static readonly SKColor ColStoneLow = new(0xCC, 0x55, 0x33);
    public static readonly SKColor ColArcher = new(0x2E, 0x7D, 0x32);
    public static readonly SKColor ColWorker = new(0xF5, 0xA5, 0x23);
    public static readonly SKColor ColLord = new(0xFF, 0xD7, 0x00);
    public static readonly SKColor ColArrow = new(0x8B, 0x4B, 0x13);
    public static readonly SKColor ColKeepBase = new(0x4A, 0x35, 0x28);
    public static readonly SKColor ColKeepDone = new(0x6B, 0x55, 0x33);
    public static readonly SKColor ColKeepProg = new(0x22, 0x88, 0x44);
    public static readonly SKColor ColHud = SKColors.White;
    public static readonly SKColor ColDim = new(0xAA, 0xAA, 0xAA);
    public static readonly SKColor ColOil = new(0x6B, 0x4A, 0x10);
    public static readonly SKColor ColFire = new(0xFF, 0x6B, 0x00);
    public static readonly SKColor ColBoulder = new(0x77, 0x77, 0x77);
    public static readonly SKColor ColAccent = new(0x00, 0xD4, 0xFF);
    public static readonly SKColor ColGold = new(0xFF, 0xD7, 0x00);
    public static readonly SKColor ColRed = new(0xFF, 0x2D, 0x55);

    public static SKColor EnemyCol(EnemyType t) => t switch
    {
        EnemyType.Spearman => new(0xE7, 0x4C, 0x3C),
        EnemyType.Swordsman => new(0x8E, 0x44, 0xAD),
        EnemyType.Berserker => new(0xE6, 0x7E, 0x22),
        EnemyType.Crossbowman => new(0x27, 0xAE, 0x60),
        EnemyType.Catapult => new(0x95, 0x7C, 0x55),
        EnemyType.Ram => new(0x6D, 0x5A, 0x3C),
        EnemyType.Cow => new(0xF5, 0xF5, 0xDC),
        _ => SKColors.Gray
    };
}
