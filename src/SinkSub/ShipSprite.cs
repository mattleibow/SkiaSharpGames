using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class ShipSprite : Sprite
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible || Alpha <= 0f)
            return;

        byte a = (byte)(255 * Alpha);

        _paint.Color = new SKColor(0x4D, 0x5B, 0x6A).WithAlpha(a);
        canvas.DrawRoundRect(SKRect.Create(x - ShipWidth / 2f, y - ShipHeight / 2f, ShipWidth, ShipHeight), 10f, 10f, _paint);

        _paint.Color = new SKColor(0xD7, 0xDB, 0xE0).WithAlpha(a);
        canvas.DrawRoundRect(SKRect.Create(x - 27f, y - 24f, 34f, 16f), 4f, 4f, _paint);

        _paint.Color = SKColors.Black.WithAlpha(a);
        canvas.DrawRect(SKRect.Create(x + 14f, y - 28f, 4f, 18f), _paint);
    }
}
