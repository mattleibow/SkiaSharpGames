namespace SkiaSharpGames.LunarLander;

/// <summary>Shared mutable state passed between the Lunar Lander screens.</summary>
internal sealed class LunarLanderGameState
{
    public float Fuel { get; set; } = LunarLanderConstants.FuelMax;
    public bool Landed { get; set; }
    public bool Crashed { get; set; }
}
