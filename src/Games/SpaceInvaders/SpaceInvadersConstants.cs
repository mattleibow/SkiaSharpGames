using SkiaSharp;

namespace SkiaSharpGames.SpaceInvaders;

internal static class SpaceInvadersConstants
{
    public const int GameWidth = 800;
    public const int GameHeight = 600;

    public const float PlayerWidth = 54f;
    public const float PlayerHeight = 20f;
    public const float PlayerY = 560f;
    public const float PlayerSpeed = 360f;

    public const int InvaderRows = 5;
    public const int InvaderCols = 11;
    public const float InvaderWidth = 30f;
    public const float InvaderHeight = 22f;
    public const float InvaderSpacingX = 14f;
    public const float InvaderSpacingY = 14f;
    public const float InvaderStartY = 110f;
    public const float InvaderStepX = 12f;
    public const float InvaderStepDown = 20f;
    public const float InvaderSideMargin = 26f;
    public const float InvaderMoveStartInterval = 0.62f;
    public const float InvaderMoveMinInterval = 0.08f;

    public const float PlayerBulletSpeed = 520f;
    public const float EnemyBulletSpeed = 260f;
    public const float PlayerFireCooldown = 0.26f;
    public const float EnemyFireMinInterval = 0.42f;
    public const float EnemyFireMaxInterval = 1.08f;
    public const float BulletWidth = 4f;
    public const float BulletHeight = 14f;

    public const int ShieldCount = 4;
    public const int ShieldRows = 4;
    public const int ShieldCols = 8;
    public const float ShieldBlockSize = 8f;
    public const float ShieldBlockGap = 2f;
    public const float ShieldY = 470f;

    public static readonly SKColor BackgroundColor = new(0x05, 0x07, 0x13);
    public static readonly SKColor AccentColor = new(0x3A, 0xF8, 0x5F);
    public static readonly SKColor HudDimColor = new(0x93, 0x9E, 0xAA);
    public static readonly SKColor PlayerColor = new(0x45, 0xF4, 0x6D);
    public static readonly SKColor EnemyBulletColor = new(0xFF, 0x71, 0x4A);
    public static readonly SKColor PlayerBulletColor = new(0xF0, 0xF0, 0xF0);
}
