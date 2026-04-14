namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal sealed class Enemy
{
    public EnemyType Type;
    public float X;
    public float HP, MaxHP;
    public float Speed;
    public EnemyState State = EnemyState.Walking;
    public float AttackTimer;
    public float AttackInterval;
    public float AttackDamage;
    public float AttackRange;  // how far from wall to stop and attack
    public bool Active = true;
    public float W, H;         // visual size
    public int TargetWallIdx = -1; // cached target wall index
    // Crossbowman
    public float FireCooldown;
    public float FireInterval = 2.5f;
}
