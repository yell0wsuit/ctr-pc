using MonoGame.Framework.Content.Pipeline.Builder;

namespace CutTheRopeDX.Content
{
    /// <summary>
    /// Declares the assets processed by the Cut the Rope DX content build.
    /// </summary>
    public sealed class GameContentBuilder : ContentBuilder
    {
        /// <inheritdoc />
        public override IContentCollection GetContentCollection()
        {
            ContentCollection content = new();
            content.SetContentRoot(string.Empty);

            // Build every asset the default importer/processor understands
            // (textures, sounds, songs). Non-buildable file types are handled
            // in later tasks via IncludeCopy / Exclude.
            content.Include<WildcardRule>("**/*.png");
            content.Include<WildcardRule>("sounds/sfx/*.wav");
            content.Include<WildcardRule>("sounds/*.ogg");

            return content;
        }
    }
}
