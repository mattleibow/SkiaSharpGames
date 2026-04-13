namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal sealed class WallBlock
{
    public float HP    { get; set; }
    public float MaxHP { get; } = 100f;
    public bool  Active => HP > 0f;
    public WallBlock() { HP = MaxHP; }
}
