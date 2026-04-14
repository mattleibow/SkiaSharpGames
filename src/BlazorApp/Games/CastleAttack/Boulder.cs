namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal sealed class Boulder
{
    public float X, Y;
    public float VX, VY;
    public bool Active = true;
    public int TargetWallIdx;
}
