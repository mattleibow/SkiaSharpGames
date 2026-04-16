using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.UIGallery;

internal sealed class UiControlEntity : Entity
{
    public UiControlEntity(string id, float x, float y, float width, float height)
    {
        Id = id;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Collider = new RectCollider { Width = width, Height = height };
    }

    public UiControlEntity(string id, float x, float y, float radius)
    {
        Id = id;
        X = x;
        Y = y;
        Radius = radius;
        Collider = new CircleCollider { Radius = radius };
    }

    public string Id { get; }
    public float Width { get; }
    public float Height { get; }
    public float Radius { get; }

    public bool IsCircle => Radius > 0f;
}

internal sealed class DelegateSprite(Action<SKCanvas> drawAction) : Sprite
{
    public override void Draw(SKCanvas canvas) => drawAction(canvas);
}
