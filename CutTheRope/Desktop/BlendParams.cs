using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    internal sealed class BlendParams
    {
        public BlendParams()
        {
            defaultBlending = true;
        }

        public BlendParams(BlendingFactor s, BlendingFactor d)
        {
            sfactor = s;
            dfactor = d;
            defaultBlending = false;
            enabled = true;
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }

        public static void ApplyDefault()
        {
            if (states[0] == null)
            {
                states[0] = BlendState.Opaque;
            }
            Global.GraphicsDevice.BlendState = states[0];
            Global.GraphicsDevice.BlendFactor = Color.White;
        }

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

        public override string ToString()
        {
            return !defaultBlending
                ? string.Concat(new object[] { "BlendParams(src=", sfactor, ", dst=", dfactor, ", enabled=", enabled, ")" })
                : "BlendParams(default)";
        }

        private static readonly BlendState[] states = new BlendState[4];

        private BlendType lastBlend = BlendType.Unknown;

        private bool enabled;

        private readonly bool defaultBlending;

        private readonly BlendingFactor sfactor;

        private readonly BlendingFactor dfactor;

        private enum BlendType
        {
            Unknown = -1,
            Default,
            SourceAlpha_InverseSourceAlpha,
            One_InverseSourceAlpha,
            SourceAlpha_One
        }
    }
}
