namespace SkiaSharpGames.Starfall;

internal sealed class StarfallGameState
{
    public int Score { get; set; }
    public float ScoreMultiplier { get; set; } = 1f;
    public int CurrentStage { get; set; } = 1;
    public int HP { get; set; } = StarfallConstants.PlayerBaseHP;
    public int MaxHP { get; set; } = StarfallConstants.PlayerBaseHP;
    public int Bombs { get; set; } = StarfallConstants.PlayerBaseBombs;
    public int MaxBombs { get; set; } = StarfallConstants.PlayerBaseBombs;
    public float FireRateMultiplier { get; set; } = 1f;
    public int BulletDamage { get; set; } = 1;
    public float SpeedMultiplier { get; set; } = 1f;

    // Active power-up timers
    public float RapidFireTimer { get; set; }
    public float SpreadShotTimer { get; set; }
    public float ShieldTimer { get; set; }

    public bool HasRapidFire => RapidFireTimer > 0;
    public bool HasSpreadShot => SpreadShotTimer > 0;
    public bool HasShield => ShieldTimer > 0;

    public float EffectiveFireRate =>
        StarfallConstants.PlayerBaseFireRate / FireRateMultiplier / (HasRapidFire ? 2f : 1f);

    public void Reset()
    {
        Score = 0;
        ScoreMultiplier = 1f;
        CurrentStage = 1;
        HP = StarfallConstants.PlayerBaseHP;
        MaxHP = StarfallConstants.PlayerBaseHP;
        Bombs = StarfallConstants.PlayerBaseBombs;
        MaxBombs = StarfallConstants.PlayerBaseBombs;
        FireRateMultiplier = 1f;
        BulletDamage = 1;
        SpeedMultiplier = 1f;
        RapidFireTimer = 0;
        SpreadShotTimer = 0;
        ShieldTimer = 0;
    }

    public void AddScore(int baseScore)
    {
        Score += (int)(baseScore * ScoreMultiplier);
    }
}
