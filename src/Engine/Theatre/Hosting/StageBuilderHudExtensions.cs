using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharp.Theatre;

public static class StageBuilderHudExtensions
{
    public static StageBuilder SetHudTheme(this StageBuilder builder, HudTheme theme)
    {
        builder.Services.AddSingleton(theme);
        return builder;
    }
}
