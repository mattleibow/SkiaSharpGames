namespace SkiaSharpGames.Pong;

internal sealed class PongGameState
{
    public int LeftScore { get; set; }
    public int RightScore { get; set; }
    public string WinnerText { get; set; } = string.Empty;
}
