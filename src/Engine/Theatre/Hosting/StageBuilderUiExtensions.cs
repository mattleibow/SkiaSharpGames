using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharp.Theatre;

public static class StageBuilderUiExtensions
{
    public static StageBuilder SetUiTheme(this StageBuilder builder, UiTheme theme)
    {
        builder.Services.AddSingleton(theme);
        return builder;
    }
}
