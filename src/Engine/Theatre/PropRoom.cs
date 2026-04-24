using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharp.Theatre;

/// <summary>
/// A typed collection of game assets backed by the game's <see cref="IServiceCollection"/>.
/// Assets registered here are available for injection into scene constructors.
/// </summary>
/// <remarks>
/// Currently a forward-looking stub. Future additions will include helpers such as
/// <c>AddTypeface</c>, <c>AddBitmap</c>, and similar asset-loading utilities that resolve
/// and cache the underlying SkiaSharp objects via the game-scoped DI container.
/// </remarks>
public sealed class PropRoom(IServiceCollection services)
{
    // Reserved for future asset-registration helpers. The services collection will be used
    // once helpers like AddTypeface, AddBitmap, etc. are implemented.
    private readonly IServiceCollection _services = services;

    // Future asset-registration helpers, e.g.:
    //   public PropRoom AddTypeface(string path) { ... }
    //   public PropRoom AddBitmap(string name, string path) { ... }
}