using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CutTheRope.windows
{
    internal class BlendParams
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

        public void enable()
        {
            enabled = true;
        }

        public void disable()
        {
            enabled = false;
        }

        public static void applyDefault()
        {
            if (states[0] == null)
            {
                states[0] = BlendState.Opaque;
            }
            Global.GraphicsDevice.BlendState = states[0];
            Global.GraphicsDevice.BlendFactor = Color.White;
        }

        public void apply()
        {
            if (defaultBlending || !enabled)
            {
                if (lastBlend != BlendType.Default)
                {
                    lastBlend = BlendType.Default;
                    applyDefault();
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GL_SRC_ALPHA && dfactor == BlendingFactor.GL_ONE_MINUS_SRC_ALPHA)
            {
                if (lastBlend != BlendType.SourceAlpha_InverseSourceAlpha)
                {
                    lastBlend = BlendType.SourceAlpha_InverseSourceAlpha;
                    if (states[(int)lastBlend] == null)
                    {
                        BlendState blendState = new();
                        blendState.AlphaSourceBlend = Blend.SourceAlpha;
                        blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                        blendState.ColorDestinationBlend = blendState.AlphaDestinationBlend;
                        blendState.ColorSourceBlend = blendState.AlphaSourceBlend;
                        states[(int)lastBlend] = blendState;
                    }
                    Global.GraphicsDevice.BlendState = states[(int)lastBlend];
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GL_ONE && dfactor == BlendingFactor.GL_ONE_MINUS_SRC_ALPHA)
            {
                if (lastBlend != BlendType.One_InverseSourceAlpha)
                {
                    lastBlend = BlendType.One_InverseSourceAlpha;
                    if (states[(int)lastBlend] == null)
                    {
                        BlendState blendState2 = new();
                        blendState2.AlphaSourceBlend = Blend.One;
                        blendState2.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                        blendState2.ColorDestinationBlend = blendState2.AlphaDestinationBlend;
                        blendState2.ColorSourceBlend = blendState2.AlphaSourceBlend;
                        states[(int)lastBlend] = blendState2;
                    }
                    Global.GraphicsDevice.BlendState = states[(int)lastBlend];
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GL_SRC_ALPHA && dfactor == BlendingFactor.GL_ONE && lastBlend != BlendType.SourceAlpha_One)
            {
                lastBlend = BlendType.SourceAlpha_One;
                if (states[(int)lastBlend] == null)
                {
                    BlendState blendState3 = new();
                    blendState3.AlphaSourceBlend = Blend.SourceAlpha;
                    blendState3.AlphaDestinationBlend = Blend.One;
                    blendState3.ColorDestinationBlend = blendState3.AlphaDestinationBlend;
                    blendState3.ColorSourceBlend = blendState3.AlphaSourceBlend;
                    states[(int)lastBlend] = blendState3;
                }
                Global.GraphicsDevice.BlendState = states[(int)lastBlend];
            }
        }

        public override string ToString()
        {
            if (!defaultBlending)
            {
                return string.Concat(new object[] { "BlendParams(src=", sfactor, ", dst=", dfactor, ", enabled=", enabled, ")" });
            }
            return "BlendParams(default)";
        }

        private static BlendState[] states = new BlendState[4];

        private BlendType lastBlend = BlendType.Unknown;

        private bool enabled;

        private bool defaultBlending;

        private BlendingFactor sfactor;

        private BlendingFactor dfactor;

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
