namespace SkiaSharpGames.CastleAttack;

/// <summary>Shared mutable state passed between the Castle Attack play and end screens.</summary>
internal sealed class CastleAttackGameState
{
    public int Score { get; set; }
    public int Level { get; set; } = 1;
}
