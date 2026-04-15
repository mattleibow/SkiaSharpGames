using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class InvaderSprite : Sprite
{
    private static readonly SKPaint _paint = new() { IsAntialias = true };

    public bool FrameB { get; set; }
    public int Row { get; set; }

    private SKColor Color =>
        Row switch
        {
            0 => new SKColor(0xFF, 0x6B, 0x6B),
            1 or 2 => new SKColor(0xFF, 0xD1, 0x66),
            _ => new SKColor(0x6F, 0xD4, 0xFF)
        };

    public override void Draw(SKCanvas canvas)
    {
        float left = -InvaderWidth / 2f;
        float top = -InvaderHeight / 2f;
        float bodyH = InvaderHeight - 6f;

        _paint.Color = Color;

        canvas.DrawRect(left, top, InvaderWidth, bodyH, _paint);
        canvas.DrawRect(left + 5f, top - 4f, InvaderWidth - 10f, 4f, _paint);

        if (FrameB)
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
