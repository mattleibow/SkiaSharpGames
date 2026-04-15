using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class ShieldBlock : Entity
{
    private static readonly SKPaint _paint = new() { IsAntialias = true };

    public int HitPoints { get; private set; } = 3;
    public readonly RectCollider Collider = new() { Width = ShieldBlockSize, Height = ShieldBlockSize };

    public void Hit()
    {
        HitPoints--;
        if (HitPoints <= 0)
            Active = false;
    }

    public void Draw(SKCanvas canvas)
    {
        if (!Active)
            return;

        _paint.Color = HitPoints switch
        {
            3 => new SKColor(0x48, 0xD0, 0x67),
            2 => new SKColor(0x6B, 0xBA, 0x6A),
            _ => new SKColor(0x88, 0x9A, 0x72)
        };

        canvas.DrawRect(X - ShieldBlockSize / 2f, Y - ShieldBlockSize / 2f, ShieldBlockSize, ShieldBlockSize, _paint);
    }
}
