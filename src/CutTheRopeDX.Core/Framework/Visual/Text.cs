using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that renders formatted text using either sprite-based or self-drawing fonts.
    /// </summary>
    internal class Text : BaseElement
    {
        /// <summary>
        /// Creates a text element from a font resource name and string.
        /// </summary>
        /// <param name="fontResourceName">Font resource name.</param>
        /// <param name="str">Text to display.</param>
        /// <returns>A new text element initialized with the requested font and string.</returns>
        public static Text CreateWithFontandString(string fontResourceName, string str)
        {
            Text text = new Text().InitWithFont(Application.GetFont(fontResourceName));
            text.SetString(str);
            return text;
        }

        /// <summary>
        /// Initializes the text element with the specified font.
        /// </summary>
        /// <param name="i">Font to use for rendering.</param>
        /// <returns>The initialized text instance.</returns>
        public virtual Text InitWithFont(FontGeneric i)
        {
            font = i;
            formattedStrings = [];
            width = -1;
            height = -1;
            align = 1;
            multiDrawers = [];
            wrapLongWords = false;
            maxHeight = -1f;
            font.NotifyTextCreated(this);
            return this;
        }

        /// <summary>
        /// Sets the display text, auto-wrapping to fit the font's measured width.
        /// </summary>
        /// <param name="newString">Text to display.</param>
        public virtual void SetString(string newString)
        {
            SetStringandWidth(newString, -1f);
        }

        /// <summary>
        /// Sets the display text with an explicit wrap width.
        /// </summary>
        /// <param name="newString">Text to display.</param>
        /// <param name="w">Wrap width in pixels, or -1 to auto-measure.</param>
        public virtual void SetStringandWidth(string newString, float w)
        {
            string_ = newString;
            string_ ??= new string("");
            font.NotifyTextChanged(this);
            if (w == -1f)
            {
                float widthPadding = 0.1f;
                wrapWidth = font.StringWidth(string_) + widthPadding;
            }
            else
            {
                wrapWidth = w;
            }
            if (string_ != null)
            {
                FormatText();

                // Only update drawer values for sprite fonts, not self-drawing fonts
                if (!font.DrawsOwnText)
                {
                    UpdateDrawerValues();
                }
                else
                {
                    // Keep width/height in sync for anchoring and layout when using a self-drawing font
                    if (formattedStrings.Count <= 1)
                    {
                        height = (int)(font.FontHeight() + font.GetTopSpacing());
                        width = (int)wrapWidth;
                    }
                    else
                    {
                        height = (int)(((font.FontHeight() + font.GetLineOffset()) * formattedStrings.Count) - font.GetLineOffset() + font.GetTopSpacing());
                        width = (int)wrapWidth;
                    }

                    if (maxHeight != -1f)
                    {
                        height = (int)MIN(height, maxHeight);
                    }
                }
                return;
            }
            stringLength = 0;
        }

        /// <summary>
        /// Rebuilds the multi-drawer quad data from the formatted strings (sprite font path).
        /// </summary>
        public virtual void UpdateDrawerValues()
        {
            multiDrawers.Clear();
            int totalCharmaps = font.TotalCharmaps();
            int textLength = string_.Length;
            char[] characters = string_.ToCharArray();
            int[] array = new int[totalCharmaps];
            for (int i = 0; i < textLength; i++)
            {
                if (characters[i] is not ' ' and not '*' and not '\n')
                {
                    array[font.GetCharmapIndex(characters[i])]++;
                }
            }
            for (int j = 0; j < totalCharmaps; j++)
            {
                int charCount = array[j];
                if (charCount > 0)
                {
                    ImageMultiDrawer item = new ImageMultiDrawer().InitWithImageandCapacity(font.GetCharmap(j), charCount);
                    multiDrawers.Add(item);
                }
            }
            float lineY = 0f;
            int fontHeight = (int)font.FontHeight();
            int renderedCharCount = 0;
            char[] characters2 = "..".ToCharArray();
            int dotSpacing = (int)font.GetCharOffset(characters2, 0, 2);
            int visibleLineCount = (int)(maxHeight == -1f ? formattedStrings.Count : MIN(formattedStrings.Count, maxHeight / (fontHeight + font.GetLineOffset())));
            bool isTruncated = visibleLineCount != formattedStrings.Count;
            int[] array2 = new int[totalCharmaps];
            for (int k = 0; k < visibleLineCount; k++)
            {
                FormattedString formattedString = formattedStrings[k];
                int lineLength = formattedString.string_.Length;
                char[] characters3 = formattedString.string_.ToCharArray();
                float lineX = align == 1 ? 0f : align != 2 ? wrapWidth - formattedString.width : (wrapWidth - formattedString.width) / 2f;
                for (int l = 0; l < lineLength; l++)
                {
                    if (characters3[l] != '*')
                    {
                        if (characters3[l] == ' ')
                        {
                            lineX += font.GetCharWidth(' ') + font.GetCharOffset(characters3, l, lineLength);
                        }
                        else
                        {
                            int charmapIndex = font.GetCharmapIndex(characters3[l]);
                            int charQuad = font.GetCharQuad(characters3[l]);

                            // Skip rendering if character is not in the font
                            if (charQuad >= 0)
                            {
                                ImageMultiDrawer imageMultiDrawer3 = multiDrawers[charmapIndex];
                                int quadIndex = charQuad;
                                float quadX = lineX;
                                float quadY = lineY;
                                int[] array3 = array2;
                                int mapIndex = charmapIndex;
                                int drawIndex = array3[mapIndex];
                                array3[mapIndex] = drawIndex + 1;
                                imageMultiDrawer3.MapTextureQuadAtXYatIndex(quadIndex, quadX, quadY, drawIndex);
                                renderedCharCount++;
                            }

                            lineX += font.GetCharWidth(characters3[l]) + font.GetCharOffset(characters3, l, lineLength);
                        }
                        if (isTruncated && k == visibleLineCount - 1)
                        {
                            int charmapIndex2 = font.GetCharmapIndex('.');
                            int charQuad2 = font.GetCharQuad('.');

                            // Only render ellipsis if '.' character is available
                            if (charQuad2 >= 0)
                            {
                                ImageMultiDrawer imageMultiDrawer2 = multiDrawers[charmapIndex2];
                                int dotWidth = (int)font.GetCharWidth('.');
                                if (l == lineLength - 1 || (l == lineLength - 2 && lineX + (3 * (dotWidth + dotSpacing)) + font.GetCharWidth(' ') > wrapWidth))
                                {
                                    imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, lineX, lineY, renderedCharCount++);
                                    lineX += dotWidth + dotSpacing;
                                    imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, lineX, lineY, renderedCharCount++);
                                    lineX += dotWidth + dotSpacing;
                                    imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, lineX, lineY, renderedCharCount++);
                                    break;
                                }
                            }
                        }
                    }
                }
                lineY += fontHeight + font.GetLineOffset();
            }
            stringLength = renderedCharCount;
            if (formattedStrings.Count <= 1)
            {
                height = (int)(font.FontHeight() + font.GetTopSpacing());
                width = (int)wrapWidth;
            }
            else
            {
                height = (int)(((font.FontHeight() + font.GetLineOffset()) * formattedStrings.Count) - font.GetLineOffset() + font.GetTopSpacing());
                width = (int)wrapWidth;
            }
            if (maxHeight != -1f)
            {
                height = (int)MIN(height, maxHeight);
            }
        }

        /// <summary>
        /// Returns the current display text.
        /// </summary>
        /// <returns>The currently assigned display text.</returns>
        public virtual string GetString()
        {
            return string_;
        }

        /// <summary>
        /// Sets the text alignment (1 = left, 2 = center, 3 = right).
        /// </summary>
        /// <param name="a">Alignment value.</param>
        public virtual void SetAlignment(int a)
        {
            align = a;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            // Capture inherited color before we apply this element's own modulation in PreDraw
            Color inheritedColor = Renderer.GetCurrentColor();

            PreDraw();

            if (font.DrawsOwnText && !string.IsNullOrEmpty(string_))
            {
                float pingPongOverflow = pingPongEnabled ? GetPingPongOverflow() : 0f;
                bool isPingPonging = pingPongOverflow > 0f;
                float clipLeft = HasParent ? parent.drawX + pingPongPadding : drawX;
                font.DrawText(new TextDrawCall(
                    formattedStrings, drawX, drawY, wrapWidth, align, maxHeight,
                    color, RGBAColor.FromRendererColor(inheritedColor),
                    isPingPonging, pingPongOffset, clipLeft,
                    EffectivePingPongClipWidth, maxHeight > 0f ? maxHeight : height));
            }
            else if (stringLength > 0)
            {
                // Legacy sprite font rendering
                Renderer.Translate(drawX, drawY, 0f);
                int i = 0;
                int count = multiDrawers.Count;
                while (i < count)
                {
                    ImageMultiDrawer imageMultiDrawer = multiDrawers[i];
                    if (imageMultiDrawer != null)
                    {
                        imageMultiDrawer.DrawAllQuads();
                        imageMultiDrawer.Optimize(Renderer.GetLastVertices_PositionNormalTexture());
                    }
                    i++;
                }
                Renderer.Translate(0f - drawX, 0f - drawY, 0f);
            }

            PostDraw();
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);

            if (!pingPongEnabled)
            {
                return;
            }

            float overflow = GetPingPongOverflow();
            if (overflow <= 0f)
            {
                pingPongOffset = 0f;
                return;
            }

            // Wait for the initial delay before starting the scroll
            if (!pingPongStarted)
            {
                pingPongPauseTimer += delta;
                if (pingPongPauseTimer >= pingPongPauseDuration)
                {
                    pingPongStarted = true;
                    pingPongPauseTimer = 0f;
                }
                return;
            }

            if (pingPongPauseTimer > 0f)
            {
                pingPongPauseTimer -= delta;
                return;
            }

            pingPongOffset += pingPongSpeed * delta * pingPongDirection;

            if (pingPongOffset >= overflow)
            {
                pingPongOffset = overflow;
                pingPongDirection = -1;
                pingPongPauseTimer = pingPongPauseDuration;
            }
            else if (pingPongOffset <= 0f)
            {
                pingPongOffset = 0f;
                pingPongDirection = 1;
                pingPongPauseTimer = pingPongPauseDuration;
            }
        }

        /// <summary>
        /// Word-wraps the current string into <see cref="FormattedString"/> lines based on <see cref="wrapWidth"/>.
        /// </summary>
        public virtual void FormatText()
        {
            short[] array = new short[512];
            char[] characters = string_.ToCharArray();
            int textLength = string_.Length;
            int rangesLength = 0;
            int wordStart = 0;
            float wordWidth = 0f;
            int lineStart = 0;
            int lineEnd = 0;
            float lineWidth = 0f;
            int cursor = 0;
            while (cursor < textLength)
            {
                char c = characters[cursor++];
                if (c is ' ' or '\n' or '*')
                {
                    lineWidth += wordWidth;
                    lineEnd = cursor - 1;
                    wordWidth = 0f;
                    wordStart = cursor;
                    if (c == ' ')
                    {
                        wordStart--;
                        wordWidth = font.GetCharWidth(' ') + font.GetCharOffset(characters, cursor - 1, textLength);
                    }
                }
                else
                {
                    wordWidth += font.GetCharWidth(c) + font.GetCharOffset(characters, cursor - 1, textLength);
                }
                bool exceedsWrap = lineWidth + wordWidth > wrapWidth;
                if (wrapLongWords && exceedsWrap && lineEnd == lineStart)
                {
                    lineWidth += wordWidth;
                    lineEnd = cursor;
                    wordWidth = 0f;
                    wordStart = cursor;
                }
                if ((lineWidth + wordWidth > wrapWidth && lineEnd != lineStart) || c == '\n')
                {
                    array[rangesLength++] = (short)lineStart;
                    array[rangesLength++] = (short)lineEnd;
                    while (wordStart < textLength && characters[wordStart] == ' ')
                    {
                        wordStart++;
                        wordWidth -= font.GetCharWidth(' ');
                    }
                    lineStart = wordStart;
                    lineEnd = lineStart;
                    lineWidth = 0f;
                }
            }
            if (wordWidth != 0f)
            {
                array[rangesLength++] = (short)lineStart;
                array[rangesLength++] = (short)cursor;
            }
            int lineCount = rangesLength >> 1;
            formattedStrings.Clear();
            for (int i = 0; i < lineCount; i++)
            {
                int rangeStart = array[i << 1];
                int rangeEnd = array[(i << 1) + 1];
                int length = rangeEnd - rangeStart;
                string str = string_.Substring(rangeStart, length);
                float w = font.StringWidth(str);
                FormattedString item = new FormattedString().InitWithStringAndWidth(str, w);
                formattedStrings.Add(item);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                font?.NotifyTextDeleted(this);
                string_ = null;
                font = null;
                formattedStrings = null;
                multiDrawers?.Clear();
                multiDrawers = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Text alignment (1 = left, 2 = center, 3 = right).
        /// </summary>
        public int align;

        /// <summary>
        /// The raw display string.
        /// </summary>
        public string string_;

        /// <summary>
        /// Number of rendered characters (sprite font path).
        /// </summary>
        public int stringLength;

        /// <summary>
        /// Font used for measuring and rendering.
        /// </summary>
        public FontGeneric font;

        /// <summary>
        /// Width at which text wraps to the next line.
        /// </summary>
        public float wrapWidth;

        /// <summary>
        /// Word-wrapped lines of text.
        /// </summary>
        private List<FormattedString> formattedStrings;

        /// <summary>
        /// Multi-drawers for sprite font character quads.
        /// </summary>
        private List<ImageMultiDrawer> multiDrawers;

        /// <summary>
        /// Maximum rendered height in pixels, or -1 for unlimited.
        /// </summary>
        public float maxHeight;

        /// <summary>
        /// Whether to break long words that exceed the wrap width.
        /// </summary>
        public bool wrapLongWords;

        /// <summary>
        /// Enables the ping-pong scrolling effect for text that overflows <see cref="pingPongClipWidth"/>.
        /// </summary>
        public bool pingPongEnabled;

        /// <summary>
        /// The visible width for ping-pong clipping. Text wider than this scrolls back and forth.
        /// Defaults to -1 (uses parent element's width, or <see cref="wrapWidth"/> if no parent).
        /// </summary>
        public float pingPongClipWidth = -1f;

        /// <summary>
        /// Horizontal padding on each side within the clip area for the ping-pong effect.
        /// </summary>
        public float pingPongPadding = 60f;

        /// <summary>
        /// Scroll speed in virtual pixels per second for the ping-pong effect.
        /// </summary>
        public float pingPongSpeed = 80f;

        /// <summary>
        /// Pause duration in seconds at each end of the ping-pong scroll.
        /// </summary>
        public float pingPongPauseDuration = 2.5f;

        /// <summary>
        /// Current horizontal scroll offset for the ping-pong effect.
        /// </summary>
        private float pingPongOffset;

        /// <summary>
        /// Current scroll direction: 1 = scrolling left (showing more of the right side), -1 = scrolling back.
        /// </summary>
        private int pingPongDirection = 1;

        /// <summary>
        /// Remaining pause time at the current scroll end.
        /// </summary>
        private float pingPongPauseTimer;

        /// <summary>
        /// Whether the initial delay has elapsed.
        /// </summary>
        private bool pingPongStarted;

        /// <summary>
        /// Returns the effective clip width for ping-pong, falling back to the parent's width or <see cref="wrapWidth"/>.
        /// </summary>
        private float EffectivePingPongClipWidth =>
            pingPongClipWidth > 0f ? pingPongClipWidth :
            HasParent ? parent.width - (pingPongPadding * 2f) : wrapWidth;

        /// <summary>
        /// Returns the overflow amount for the widest formatted line, or 0 if no overflow.
        /// </summary>
        private float GetPingPongOverflow()
        {
            float clipW = EffectivePingPongClipWidth;
            float maxW = 0f;
            if (formattedStrings != null)
            {
                foreach (FormattedString fs in formattedStrings)
                {
                    if (fs.width > maxW)
                    {
                        maxW = fs.width;
                    }
                }
            }
            return maxW > clipW ? maxW - clipW : 0f;
        }
    }
}
