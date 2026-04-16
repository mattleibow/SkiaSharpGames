using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class PlayerCannon : Entity
{
    public PlayerCannon()
    {
        Collider = new RectCollider { Width = PlayerWidth, Height = PlayerHeight };
        Sprite = new PlayerCannonSprite();
    }
}
