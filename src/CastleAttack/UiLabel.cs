using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Simple UI/floating text entity backed by <see cref="TextSprite"/>.</summary>
internal sealed class UiLabel : Entity
{
    private string _text = "";
    private SKColor _color = SKColors.White;
    private float _alpha = 1f;

    public UiLabel()
    {
        Sprite = new TextSprite();
        Sprite.Text = _text;
        Sprite.Color = _color;
        Sprite.Alpha = _alpha;
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            Sprite.Text = value;
        }
    }

    public SKColor Color
    {
        get => _color;
        set
        {
            _color = value;
            Sprite.Color = value;
        }
    }

    public float Alpha
    {
        get => _alpha;
        set
        {
            _alpha = Math.Clamp(value, 0f, 1f);
            Sprite.Alpha = _alpha;
        }
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

    protected override void OnUpdate(float deltaTime)
    {
        Sprite.Text = _text;
        Sprite.Color = _color;
        Sprite.Alpha = _alpha;
    }
}
