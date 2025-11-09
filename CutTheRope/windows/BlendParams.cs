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
            if (BlendParams.states[0] == null)
            {
                BlendParams.states[0] = BlendState.Opaque;
            }
            Global.GraphicsDevice.BlendState = BlendParams.states[0];
            Global.GraphicsDevice.BlendFactor = Color.White;
        }

        public void apply()
        {
            if (defaultBlending || !enabled)
            {
                if (lastBlend != BlendParams.BlendType.Default)
                {
                    lastBlend = BlendParams.BlendType.Default;
                    BlendParams.applyDefault();
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GL_SRC_ALPHA && dfactor == BlendingFactor.GL_ONE_MINUS_SRC_ALPHA)
            {
                if (lastBlend != BlendParams.BlendType.SourceAlpha_InverseSourceAlpha)
                {
                    lastBlend = BlendParams.BlendType.SourceAlpha_InverseSourceAlpha;
                    if (BlendParams.states[(int)lastBlend] == null)
                    {
                        BlendState blendState = new();
                        blendState.AlphaSourceBlend = Blend.SourceAlpha;
                        blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                        blendState.ColorDestinationBlend = blendState.AlphaDestinationBlend;
                        blendState.ColorSourceBlend = blendState.AlphaSourceBlend;
                        BlendParams.states[(int)lastBlend] = blendState;
                    }
                    Global.GraphicsDevice.BlendState = BlendParams.states[(int)lastBlend];
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GL_ONE && dfactor == BlendingFactor.GL_ONE_MINUS_SRC_ALPHA)
            {
                if (lastBlend != BlendParams.BlendType.One_InverseSourceAlpha)
                {
                    lastBlend = BlendParams.BlendType.One_InverseSourceAlpha;
                    if (BlendParams.states[(int)lastBlend] == null)
                    {
                        BlendState blendState2 = new();
                        blendState2.AlphaSourceBlend = Blend.One;
                        blendState2.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                        blendState2.ColorDestinationBlend = blendState2.AlphaDestinationBlend;
                        blendState2.ColorSourceBlend = blendState2.AlphaSourceBlend;
                        BlendParams.states[(int)lastBlend] = blendState2;
                    }
                    Global.GraphicsDevice.BlendState = BlendParams.states[(int)lastBlend];
                    return;
                }
            }
            else if (sfactor == BlendingFactor.GL_SRC_ALPHA && dfactor == BlendingFactor.GL_ONE && lastBlend != BlendParams.BlendType.SourceAlpha_One)
            {
                lastBlend = BlendParams.BlendType.SourceAlpha_One;
                if (BlendParams.states[(int)lastBlend] == null)
                {
                    BlendState blendState3 = new();
                    blendState3.AlphaSourceBlend = Blend.SourceAlpha;
                    blendState3.AlphaDestinationBlend = Blend.One;
                    blendState3.ColorDestinationBlend = blendState3.AlphaDestinationBlend;
                    blendState3.ColorSourceBlend = blendState3.AlphaSourceBlend;
                    BlendParams.states[(int)lastBlend] = blendState3;
                }
                Global.GraphicsDevice.BlendState = BlendParams.states[(int)lastBlend];
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

        private BlendParams.BlendType lastBlend = BlendParams.BlendType.Unknown;

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
