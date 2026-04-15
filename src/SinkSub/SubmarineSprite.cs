using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class SubmarineSprite : Sprite
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible || Alpha <= 0f)
            return;

        byte a = (byte)(255 * Alpha);

        _paint.Color = new SKColor(0x3F, 0x6B, 0x7A).WithAlpha(a);
        canvas.DrawRoundRect(SKRect.Create(x - SubWidth / 2f, y - SubHeight / 2f, SubWidth, SubHeight), 12f, 12f, _paint);

        _paint.Color = new SKColor(0x2D, 0x4F, 0x5A).WithAlpha(a);
        canvas.DrawRect(SKRect.Create(x - 10f, y - 18f, 20f, 8f), _paint);

        _paint.Color = SKColors.White.WithAlpha(110).WithAlpha(a);
        canvas.DrawRect(SKRect.Create(x + 24f, y - 2f, 14f, 4f), _paint);
    }
}
