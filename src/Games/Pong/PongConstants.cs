using SkiaSharp;

namespace SkiaSharpGames.Pong;

internal static class PongConstants
{
    public const float GameWidth = 800f;
    public const float GameHeight = 600f;

    public const float PaddleWidth = 16f;
    public const float PaddleHeight = 96f;
    public const float PaddleMargin = 34f;
    public const float PaddleSpeed = 380f;

    public const float BallRadius = 10f;
    public const float BallSpeed = 330f;
    public const float BallMaxSpeed = 560f;
    public const float BallSpeedGain = 18f;
    public const float BallVerticalSpeedFromHit = 210f;
    public const float EdgeColliderThickness = 12f;

    public const int WinningScore = 7;
    public const float ServeDelay = 0.7f;

    public static readonly SKColor BackgroundColor = new(0x0A, 0x0E, 0x1A);
    public static readonly SKColor AccentColor = new(0x5D, 0xCE, 0xFF);
    public static readonly SKColor DimColor = new(0xA3, 0xAE, 0xC2);
    public static readonly SKColor LeftPaddleColor = new(0x7C, 0xF2, 0xA7);
    public static readonly SKColor RightPaddleColor = new(0xFF, 0x8C, 0x82);
}