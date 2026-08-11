using System.Collections.Generic;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Everything a self-drawing font needs to render a Text element:
    /// layout, formatted lines, color modulation, and ping-pong clip state.
    /// Field meanings mirror the Text fields they are copied from.
    /// </summary>
    internal readonly struct TextDrawCall(
        IReadOnlyList<FormattedString> lines,
        float drawX, float drawY,
        float wrapWidth, int align, float maxHeight,
        RGBAColor elementColor, RGBAColor inheritedColor,
        bool isPingPonging, float pingPongOffset, float pingPongClipLeft,
        float pingPongClipWidth, float pingPongClipHeight)
    {
        public IReadOnlyList<FormattedString> Lines { get; } = lines;
        public float DrawX { get; } = drawX;
        public float DrawY { get; } = drawY;
        public float WrapWidth { get; } = wrapWidth;
        public int Align { get; } = align;
        public float MaxHeight { get; } = maxHeight;
        public RGBAColor ElementColor { get; } = elementColor;
        public RGBAColor InheritedColor { get; } = inheritedColor;
        public bool IsPingPonging { get; } = isPingPonging;
        public float PingPongOffset { get; } = pingPongOffset;
        public float PingPongClipLeft { get; } = pingPongClipLeft;
        public float PingPongClipWidth { get; } = pingPongClipWidth;
        public float PingPongClipHeight { get; } = pingPongClipHeight;
    }
}
