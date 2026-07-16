using CutTheRopeDX.Content;

using Microsoft.Xna.Framework.Content.Pipeline;

using MonoGame.Framework.Content.Pipeline.Builder;

GameContentBuilder builder = new();

if (args is { Length: > 0 })
{
    builder.Run(args);
}
else
{
    _ = builder.Run(new ContentBuilderParams
    {
        Mode = ContentBuilderMode.Builder,
        WorkingDirectory = $"{AppContext.BaseDirectory}../../",
        SourceDirectory = "content",
        Platform = TargetPlatform.DesktopVK,
    });
}

return builder.FailedToBuild > 0 ? -1 : 0;
