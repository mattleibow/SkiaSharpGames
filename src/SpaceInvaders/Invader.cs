using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class Invader(int row, int col) : Entity
{
    private static readonly SKPaint _paint = new() { IsAntialias = true };

    public int Row { get; } = row;
    public int Col { get; } = col;

    public readonly RectCollider Collider = new() { Width = InvaderWidth, Height = InvaderHeight };

    public int ScoreValue =>
        Row switch
        {
            0 => 30,
            1 or 2 => 20,
            _ => 10
        };

    private SKColor Color =>
        Row switch
        {
            0 => new SKColor(0xFF, 0x6B, 0x6B),
            1 or 2 => new SKColor(0xFF, 0xD1, 0x66),
            _ => new SKColor(0x6F, 0xD4, 0xFF)
        };

    public void Draw(SKCanvas canvas, bool frameB)
    {
        float left = X - InvaderWidth / 2f;
        float top = Y - InvaderHeight / 2f;
        float bodyH = InvaderHeight - 6f;

        _paint.Color = Color;

        canvas.DrawRect(left, top, InvaderWidth, bodyH, _paint);
        canvas.DrawRect(left + 5f, top - 4f, InvaderWidth - 10f, 4f, _paint);

        if (frameB)
        {
            canvas.DrawRect(left + 4f, top + bodyH, 5f, 6f, _paint);
            canvas.DrawRect(left + InvaderWidth - 9f, top + bodyH, 5f, 6f, _paint);
        }
        else
        {
            canvas.DrawRect(left + 9f, top + bodyH, 5f, 6f, _paint);
            canvas.DrawRect(left + InvaderWidth - 14f, top + bodyH, 5f, 6f, _paint);
        }
    }
}
