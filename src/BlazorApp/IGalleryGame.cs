using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp;

/// <summary>
/// Describes a game in the gallery. Provides metadata for the home page
/// and a factory to create the <see cref="Game"/> instance.
/// </summary>
public interface IGalleryGame
{
    /// <summary>URL slug used for routing (e.g. "breakout", "space-invaders").</summary>
    string Slug { get; }

    /// <summary>Display title shown on the gallery card and page title.</summary>
    string Title { get; }

    /// <summary>Short description shown on the gallery card.</summary>
    string Description { get; }

    /// <summary>Emoji shown on the gallery card banner.</summary>
    string Emoji { get; }

    /// <summary>Creates a new, ready-to-run Game instance.</summary>
    Game CreateGame();
}
