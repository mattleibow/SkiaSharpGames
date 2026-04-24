using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Base class for all enemies. Tracks HP and provides hit/destroy interface.
/// </summary>
internal abstract class EnemyBase : Actor
{
    public float HP { get; set; }
    public float MaxHP { get; set; }
    public int ScoreValue { get; set; }
    public float Radius { get; protected set; }

    protected EnemyBase(float hp, int score, float radius)
    {
        HP = hp;
        MaxHP = hp;
        ScoreValue = score;
        Radius = radius;
        Collider = new CircleCollider(radius);
    }

    /// <summary>Apply damage. Returns true if the enemy was destroyed.</summary>
    public bool TakeDamage(float damage)
    {
        HP -= damage;
        if (HP <= 0f)
        {
            HP = 0f;
            Active = false;
            return true;
        }
        // Flash white briefly on hit
        _hitFlash = 0.08f;
        return false;
    }

    private float _hitFlash;

    protected override void OnUpdate(float deltaTime)
    {
        if (_hitFlash > 0f)
            _hitFlash -= deltaTime;

        // Remove if off-screen (below game area)
        if (Y > GameHeight + Radius * 2 + 50f)
            Active = false;
    }

    protected bool IsHitFlashing => _hitFlash > 0f;

    protected SKColor FlashColor(SKColor baseColor) =>
        IsHitFlashing ? SKColors.White : baseColor;
}
