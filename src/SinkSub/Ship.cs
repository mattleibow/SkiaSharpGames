using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Ship : Entity
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public Ship()
    {
        Collider = new RectCollider { Width = ShipWidth, Height = ShipHeight };
    }

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        byte a = (byte)(255 * Alpha);

        _paint.Color = new SKColor(0x4D, 0x5B, 0x6A).WithAlpha(a);
        canvas.DrawRoundRect(SKRect.Create(0f - ShipWidth / 2f, 0f - ShipHeight / 2f, ShipWidth, ShipHeight), 10f, 10f, _paint);

        _paint.Color = new SKColor(0xD7, 0xDB, 0xE0).WithAlpha(a);
        canvas.DrawRoundRect(SKRect.Create(0f - 27f, 0f - 24f, 34f, 16f), 4f, 4f, _paint);

        _paint.Color = SKColors.Black.WithAlpha(a);
        canvas.DrawRect(SKRect.Create(0f + 14f, 0f - 28f, 4f, 18f), _paint);
    }
}
