namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal sealed class Wall
{
    public float CenterX    { get; }
    public float LeftX      => CenterX - CastleAttackGame.BlockW / 2f;
    public List<WallBlock> Blocks { get; } = [];
    public bool  HasArcher  { get; set; }
    public int   TotalBlocks { get; }

    public int   ActiveBlocks   => Blocks.Count(b => b.Active);
    public bool  IsDestroyed    => Blocks.All(b => !b.Active);

    // Top Y of the current wall surface (where archer stands)
    public float TopY => IsDestroyed ? CastleAttackGame.GroundY : CastleAttackGame.GroundY - ActiveBlocks * CastleAttackGame.BlockH;
    public float ArcherCenterX => CenterX;
    public float ArcherBaseY   => TopY;   // archer feet

    public Wall(float cx, int blocks, bool hasArcher)
    {
        CenterX     = cx;
        TotalBlocks = blocks;
        HasArcher   = hasArcher;
        for (int i = 0; i < blocks; i++)
            Blocks.Add(new WallBlock());
    }

    // Damage the bottom-most active block
    public void TakeDamage(float dmg)
    {
        var block = Blocks.LastOrDefault(b => b.Active);
        if (block != null) block.HP = Math.Max(0f, block.HP - dmg);
    }

    // Instantly destroy every block (ram hit)
    public void Demolish()
    {
        foreach (var b in Blocks) b.HP = 0f;
        HasArcher = false;
    }
}
