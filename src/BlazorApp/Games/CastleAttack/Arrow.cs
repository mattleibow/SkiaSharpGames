namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal sealed class Arrow
{
    public float X, Y;
    public float VX, VY;
    public bool  Active   = true;
    public bool  IsEnemy  = false;   // crossbowman bolt
    public int   EnemyTargetWall;    // which archer it aims at
}
