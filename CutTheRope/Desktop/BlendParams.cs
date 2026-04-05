using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    /// <summary>
    /// Encapsulates the desktop renderer's current blend configuration and cached blend states.
    /// </summary>
    internal sealed class BlendParams
    {
        /// <summary>
        /// Initializes a blend configuration that uses the default opaque blend state.
        /// </summary>
        public BlendParams()
        {
            defaultBlending = true;
        }

        /// <summary>
        /// Initializes a blend configuration with explicit source and destination factors.
        /// </summary>
        /// <param name="s">The source blend factor.</param>
        /// <param name="d">The destination blend factor.</param>
        public BlendParams(BlendingFactor s, BlendingFactor d)
        {
            sfactor = s;
            dfactor = d;
            defaultBlending = false;
            enabled = true;
        }

        /// <summary>
        /// Enables this blend configuration for subsequent draw calls.
        /// </summary>
        public void Enable()
        {
            enabled = true;
        }

        /// <summary>
        /// Disables this blend configuration and restores default blending when applied.
        /// </summary>
        public void Disable()
        {
            enabled = false;
        }

        /// <summary>
        /// Applies the default opaque blend state to the graphics device.
        /// </summary>
        public static void ApplyDefault()
        {
            if (states[0] == null)
            {
                states[0] = BlendState.Opaque;
            }
            Global.GraphicsDevice.BlendState = states[0];
            Global.GraphicsDevice.BlendFactor = Color.White;
        }

        /// <summary>
        /// Applies the current blend configuration to the graphics device.
        /// </summary>
        public void Apply()
        {
            if (defaultBlending || !enabled)
            {
                if (lastBlend != BlendType.Default)
                {
                    lastBlend = BlendType.Default;
                    ApplyDefault();
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GLSRCALPHA && dfactor == BlendingFactor.GLONEMINUSSRCALPHA)
            {
                if (lastBlend != BlendType.SourceAlpha_InverseSourceAlpha)
                {
                    lastBlend = BlendType.SourceAlpha_InverseSourceAlpha;
                    if (states[(int)lastBlend] == null)
                    {
                        BlendState blendState = new()
                        {
                            AlphaSourceBlend = Blend.SourceAlpha,
                            AlphaDestinationBlend = Blend.InverseSourceAlpha
                        };
                        blendState.ColorDestinationBlend = blendState.AlphaDestinationBlend;
                        blendState.ColorSourceBlend = blendState.AlphaSourceBlend;
                        states[(int)lastBlend] = blendState;
                    }
                    Global.GraphicsDevice.BlendState = states[(int)lastBlend];
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GLONE && dfactor == BlendingFactor.GLONEMINUSSRCALPHA)
            {
                if (lastBlend != BlendType.One_InverseSourceAlpha)
                {
                    lastBlend = BlendType.One_InverseSourceAlpha;
                    if (states[(int)lastBlend] == null)
                    {
                        BlendState blendState2 = new()
                        {
                            AlphaSourceBlend = Blend.One,
                            AlphaDestinationBlend = Blend.InverseSourceAlpha
                        };
                        blendState2.ColorDestinationBlend = blendState2.AlphaDestinationBlend;
                        blendState2.ColorSourceBlend = blendState2.AlphaSourceBlend;
                        states[(int)lastBlend] = blendState2;
                    }
                    Global.GraphicsDevice.BlendState = states[(int)lastBlend];
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GLSRCALPHA && dfactor == BlendingFactor.GLONE && lastBlend != BlendType.SourceAlpha_One)
            {
                lastBlend = BlendType.SourceAlpha_One;
                if (states[(int)lastBlend] == null)
                {
                    BlendState blendState3 = new()
                    {
                        AlphaSourceBlend = Blend.SourceAlpha,
                        AlphaDestinationBlend = Blend.One
                    };
                    blendState3.ColorDestinationBlend = blendState3.AlphaDestinationBlend;
                    blendState3.ColorSourceBlend = blendState3.AlphaSourceBlend;
                    states[(int)lastBlend] = blendState3;
                }
                Global.GraphicsDevice.BlendState = states[(int)lastBlend];
            }
        }

        /// <summary>
        /// Returns a debug representation of the current blend configuration.
        /// </summary>
        /// <returns>A string describing the blend configuration.</returns>
        public override string ToString()
        {
            return !defaultBlending
                ? string.Concat(new object[] { "BlendParams(src=", sfactor, ", dst=", dfactor, ", enabled=", enabled, ")" })
                : "BlendParams(default)";
        }

        /// <summary>
        /// Caches reusable MonoGame blend states keyed by <see cref="BlendType"/>.
        /// </summary>
        private static readonly BlendState[] states = new BlendState[4];

        /// <summary>
        /// Tracks the last applied blend mode to avoid redundant state changes.
        /// </summary>
        private BlendType lastBlend = BlendType.Unknown;

        /// <summary>
        /// Indicates whether the custom blend parameters are enabled.
        /// </summary>
        private bool enabled;

        /// <summary>
        /// Indicates whether this instance represents the renderer's default blend behavior.
        /// </summary>
        private readonly bool defaultBlending;

        /// <summary>
        /// The source blend factor for custom blending.
        /// </summary>
        private readonly BlendingFactor sfactor;

        /// <summary>
        /// The destination blend factor for custom blending.
        /// </summary>
        private readonly BlendingFactor dfactor;

        /// <summary>
        /// Identifies the cached blend-state variants supported by the desktop renderer.
        /// </summary>
        private enum BlendType
        {
            /// <summary>
            /// No cached blend state has been applied yet.
            /// </summary>
            Unknown = -1,

            /// <summary>
            /// The default opaque blend state.
            /// </summary>
            Default,

            /// <summary>
            /// Source alpha blended against inverse source alpha.
            /// </summary>
            SourceAlpha_InverseSourceAlpha,

            /// <summary>
            /// One blended against inverse source alpha.
            /// </summary>
            One_InverseSourceAlpha,

            /// <summary>
            /// Source alpha blended additively against one.
            /// </summary>
            SourceAlpha_One
        }
    }
}
