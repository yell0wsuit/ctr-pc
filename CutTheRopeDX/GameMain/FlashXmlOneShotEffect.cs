using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Spawns a one-shot Flash XML animation (parsed once, replayed many times) at a world
    /// position and hands it to an <see cref="AnimationsPool" /> for update/draw/cleanup.
    /// Used for transient burst effects such as the chain-cut animation, where the source is a
    /// transform timeline (scale/skew/color) rather than a frame flipbook.
    /// </summary>
    internal sealed class FlashXmlOneShotEffect
    {
        /// <summary>Parsed animation definition, shared across every spawned instance.</summary>
        private readonly FlashXmlAnimationDefinition _definition;

        /// <summary>Resource name of the atlas the effect parts and stage root draw from.</summary>
        private readonly string _textureResourceName;

        /// <summary>Stage-local position that should land on the spawn point.</summary>
        private readonly float _anchorX;

        /// <summary>Stage-local position that should land on the spawn point.</summary>
        private readonly float _anchorY;

        /// <summary>
        /// Parses the effect animation once and resolves its anchor.
        /// </summary>
        /// <param name="animationXmlFileName">Flash XML file name under the animations directory.</param>
        /// <param name="textureResourceName">Atlas resource name shared by the parts and stage root.</param>
        public FlashXmlOneShotEffect(string animationXmlFileName, string textureResourceName)
        {
            _definition = FlashXmlImporter.ParseFile(
                ContentPaths.GetAnimationXmlAbsolutePath(animationXmlFileName));
            _textureResourceName = textureResourceName;

            // Center the effect on the spawn point by anchoring the stage at the first part's
            // first keyframe position, falling back to the stage center when unavailable.
            (_anchorX, _anchorY) = ResolveStageAnchor(_definition);
        }

        /// <summary>
        /// Spawns one instance at a world position, plays its timeline, and adds it to the pool,
        /// which removes it when the root timeline completes.
        /// </summary>
        /// <param name="pool">Animation pool that owns the spawned root's lifecycle.</param>
        /// <param name="x">World-space X position for the effect.</param>
        /// <param name="y">World-space Y position for the effect.</param>
        /// <param name="timelineId">Flash XML timeline ID to play.</param>
        public void SpawnInto(AnimationsPool pool, float x, float y, int timelineId)
        {
            FlashXmlStageRoot root = new();
            _ = root.InitWithTexture(Application.GetTexture(_textureResourceName));
            root.SetDrawQuad(0);
            root.color = RGBAColor.transparentRGBA;
            root.passColorToChilds = false;
            root.useCustomAnchor = true;
            root.customAnchorX = _anchorX;
            root.customAnchorY = _anchorY;
            root.width = (int)System.MathF.Round(_definition.StageWidth);
            root.height = (int)System.MathF.Round(_definition.StageHeight);
            root.x = x;
            root.y = y;

            List<Image> parts = [];
            FlashXmlTargetAnimationBackend.BuildParts(_definition, root, parts, -1, -1);
            FlashXmlTargetAnimationBackend.BuildRootTimelines(_definition, root, -1, -1);

            FlashXmlTargetAnimationBackend.PlayTimeline(parts, timelineId);
            FlashXmlTargetAnimationBackend.PlayRootTimeline(root, timelineId);

            // Route the root timeline's completion to the pool so the whole effect is removed
            // once it finishes (the export ends its root timeline with a self-delete action).
            if (root.GetTimeline(timelineId) is { } rootTimeline)
            {
                rootTimeline.delegateTimelineDelegate = pool;
            }

            _ = pool.AddChild(root);
        }

        /// <summary>
        /// Resolves the stage-local anchor that should coincide with the spawn point.
        /// </summary>
        /// <param name="definition">Parsed animation definition.</param>
        /// <returns>The stage-local anchor X/Y.</returns>
        private static (float X, float Y) ResolveStageAnchor(FlashXmlAnimationDefinition definition)
        {
            if (definition.Parts.Count > 0)
            {
                foreach (KeyValuePair<int, FlashXmlTimelineDefinition> timeline in definition.Parts[0].Timelines)
                {
                    IReadOnlyList<FlashXmlFloat2KeyFrame> positions = timeline.Value.PositionKeyFrames;
                    if (positions.Count > 0)
                    {
                        return (positions[0].X, positions[0].Y);
                    }
                }
            }

            return (definition.StageWidth * 0.5f, definition.StageHeight * 0.5f);
        }
    }
}
