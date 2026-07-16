using Microsoft.Xna.Framework.Content.Pipeline.Processors;

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

            // Non-premultiplied cursors (the first assets the game loads).
            content.Exclude<WildcardRule>("images/cursor.png");
            content.Exclude<WildcardRule>("images/cursor_active.png");
            content.Include(
                "images/cursor.png",
                contentProcessor: new TextureProcessor { PremultiplyAlpha = false });
            content.Include(
                "images/cursor_active.png",
                contentProcessor: new TextureProcessor { PremultiplyAlpha = false });

            // Color-keyed candy sprite (magenta -> transparent).
            content.Exclude<WildcardRule>("images/candies/obj_candy_52.png");
            content.Include(
                "images/candies/obj_candy_52.png",
                contentProcessor: new TextureProcessor
                {
                    ColorKeyEnabled = true,
                    ColorKeyColor = new Microsoft.Xna.Framework.Color(255, 0, 255, 255),
                    PremultiplyAlpha = true,
                });

            content.Include<WildcardRule>("sounds/sfx/*.wav");
            content.Include<WildcardRule>("sounds/*.ogg");

            // Copy (do not build) content the game reads as raw files.
            content.IncludeCopy<WildcardRule>("maps/*.*");
            content.IncludeCopy<WildcardRule>("locales/*.*");
            content.IncludeCopy<WildcardRule>("fonts/*.*");
            content.IncludeCopy<WildcardRule>("video_hd/*.*");
            content.IncludeCopy<WildcardRule>("*.xml");
            content.IncludeCopy<WildcardRule>("*.json");
            content.IncludeCopy<WildcardRule>("*.cur");

            // Exclude legacy pipeline artifacts and prebuilt outputs.
            content.Exclude<WildcardRule>("content.mgcb");
            content.Exclude<WildcardRule>("bin/**/*");
            content.Exclude<WildcardRule>("obj/**/*");
            content.Exclude<WildcardRule>("Builder/**/*");
            content.Exclude<WildcardRule>("sounds/*.xnb");

            return content;
        }
    }
}
