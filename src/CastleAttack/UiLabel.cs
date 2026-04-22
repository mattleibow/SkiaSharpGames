using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Simple UI/floating text entity backed by <see cref="TextSprite"/>.</summary>
internal sealed class UiLabel : Entity
{
    public UiLabel()
    {
        Sprite = new TextSprite();
    }

    public string Text
    {
        get => Sprite.Text;
        set => Sprite.Text = value;
    }

    public SKColor Color
    {
        get => Sprite.Color;
        set => Sprite.Color = value;
    }

    public float Alpha
    {
        get => Sprite.Alpha;
        set => Sprite.Alpha = Math.Clamp(value, 0f, 1f);
    }

    public float Size
    {
        get => Sprite.Size;
        set => Sprite.Size = value;
    }

    public TextAlign Align
    {
        get => Sprite.Align;
        set => Sprite.Align = value;
    }

    public new TextSprite Sprite { get => (TextSprite)base.Sprite!; init => base.Sprite = value; }
}
