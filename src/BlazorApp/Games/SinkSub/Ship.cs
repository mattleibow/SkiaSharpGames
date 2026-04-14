using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.BlazorApp.Games.SinkSub;

internal sealed class Ship : Entity
{
    public readonly RectCollider Collider = new() { Width = ShipWidth, Height = ShipHeight };

    public readonly RectSprite Hull = new()
    {
        Width = ShipWidth,
        Height = ShipHeight,
        Color = new SKColor(0x4D, 0x5B, 0x6A),
        CornerRadius = 10f,
        ShowShine = true,
    };

    public readonly RectSprite Bridge = new()
    {
        Width = 34f,
        Height = 16f,
        Color = new SKColor(0xD7, 0xDB, 0xE0),
        CornerRadius = 4f,
        ShowShine = false,
    };

    public void Draw(SKCanvas canvas)
    {
        Hull.Draw(canvas, X, Y);
        Bridge.Draw(canvas, X - 10f, Y - 16f);
        DrawHelper.FillRect(canvas, X + 14f, Y - 28f, 4f, 18f, SKColors.Black);
    }
}
