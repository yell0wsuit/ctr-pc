using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
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
            ApplySnapshot(BlendType.Default);
        }

        /// <summary>
        /// Applies the current blend configuration to the graphics device, skipping the
        /// device write when the same type was already applied and nothing external
        /// invalidated the cache.
        /// </summary>
        public void Apply()
        {
            BlendType snapshot = Snapshot();
            if (snapshot == BlendType.Unknown || snapshot == s_lastApplied)
            {
                return;
            }
            ApplySnapshot(snapshot);
        }

        /// <summary>
        /// Computes the effective blend type from this configuration without touching the device.
        /// </summary>
        /// <returns>The effective blend type; Unknown for factor pairs with no cached state.</returns>
        public BlendType Snapshot()
        {
            return (defaultBlending || !enabled, sfactor, dfactor) switch
            {
                (true, _, _) => BlendType.Default,
                (false, BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA) => BlendType.SourceAlpha_InverseSourceAlpha,
                (false, BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA) => BlendType.One_InverseSourceAlpha,
                (false, BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE) => BlendType.SourceAlpha_One,
                _ => BlendType.Unknown
            };
        }

        /// <summary>
        /// Applies the given blend type to the graphics device unconditionally and records it.
        /// Used by the quad-batch flush, which must not trust the lazy cache.
        /// </summary>
        /// <param name="snapshot">The blend type to apply. Unknown is ignored.</param>
        public static void ApplySnapshot(BlendType snapshot)
        {
            if (snapshot == BlendType.Unknown)
            {
                return;
            }
            Global.GraphicsDevice.BlendState = GetState(snapshot);
            if (snapshot == BlendType.Default)
            {
                Global.GraphicsDevice.BlendFactor = Color.White;
            }
            s_lastApplied = snapshot;
        }

        /// <summary>
        /// Marks the cached device blend state as unknown. Call after any code outside this
        /// class changes GraphicsDevice.BlendState (SpriteBatch.Begin does).
        /// </summary>
        public static void InvalidateDeviceCache()
        {
            s_lastApplied = BlendType.Unknown;
        }

        /// <summary>
        /// Returns the cached BlendState for a blend type, building it on first use.
        /// </summary>
        /// <param name="type">The blend type to look up.</param>
        /// <returns>The cached blend state.</returns>
        private static BlendState GetState(BlendType type)
        {
            int index = (int)type;
            if (states[index] == null)
            {
                states[index] = type switch
                {
                    BlendType.Default => BlendState.Opaque,
                    BlendType.SourceAlpha_InverseSourceAlpha => NewState(Blend.SourceAlpha, Blend.InverseSourceAlpha),
                    BlendType.One_InverseSourceAlpha => NewState(Blend.One, Blend.InverseSourceAlpha),
                    BlendType.SourceAlpha_One => NewState(Blend.SourceAlpha, Blend.One),
                    BlendType.Unknown => throw new InvalidOperationException("Unknown blend snapshots cannot be applied."),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
            }
            return states[index];
        }

        /// <summary>
        /// Builds a blend state with identical color and alpha factors.
        /// </summary>
        /// <param name="source">Source blend factor.</param>
        /// <param name="destination">Destination blend factor.</param>
        /// <returns>The new blend state.</returns>
        private static BlendState NewState(Blend source, Blend destination)
        {
            return new BlendState
            {
                AlphaSourceBlend = source,
                AlphaDestinationBlend = destination,
                ColorSourceBlend = source,
                ColorDestinationBlend = destination
            };
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
        /// The blend type last applied to the graphics device through this class.
        /// Static because it mirrors global device state, not per-instance state.
        /// </summary>
        private static BlendType s_lastApplied = BlendType.Unknown;

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
        internal enum BlendType
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
