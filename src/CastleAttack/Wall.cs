using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

internal sealed class Wall
{
    public float CenterX { get; }
    public float LeftX => CenterX - BlockW / 2f;
    public List<WallBlock> Blocks { get; } = [];
    public bool HasArcher { get; set; }
    public int TotalBlocks { get; }

    public int ActiveBlocks => Blocks.Count(b => b.Active);
    public bool IsDestroyed => Blocks.All(b => !b.Active);

    public float TopY => IsDestroyed ? GroundY : GroundY - ActiveBlocks * BlockH;
    public float ArcherCenterX => CenterX;
    public float ArcherBaseY => TopY;

    public Wall(float cx, int blocks, bool hasArcher)
    {
        CenterX = cx;
        TotalBlocks = blocks;
        HasArcher = hasArcher;
        for (int i = 0; i < blocks; i++)
            Blocks.Add(new WallBlock());
    }

    public void TakeDamage(float dmg)
    {
        var block = Blocks.LastOrDefault(b => b.Active);
        if (block != null) block.HP = Math.Max(0f, block.HP - dmg);
    }

    public void Demolish()
    {
        foreach (var b in Blocks) b.HP = 0f;
        HasArcher = false;
    }
}
