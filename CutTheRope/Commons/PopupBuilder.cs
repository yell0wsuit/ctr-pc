using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;
using CutTheRope.GameMain;

namespace CutTheRope.Commons
{
    /// <summary>
    /// Builds and displays popups from reusable templates.
    /// </summary>
    internal sealed class PopupBuilder(MenuController controller)
    {
        internal const float LargeScale = 1.2f;
        internal const float XLargeScale = 1.5f;
        internal const float DefaultScrollableWidth = 700f;
        internal const float DefaultScrollableHeight = 300f;
        internal const float DefaultButtonSpacing = 0f;

        private readonly MenuController menuController = controller;

        /// <summary>
        /// Builds and shows a popup from the provided template definition.
        /// </summary>
        /// <param name="template">Template describing the popup's content and layout.</param>
        /// <returns>The created popup instance.</returns>
        public Popup Show(PopupTemplate template)
        {
            Popup popup = new();
            popup.SetName("popup");

            BaseElement contentRoot = popup.ContentRoot;
            ApplyTemplateScale(popup, template, out float backgroundScaleX, out float backgroundScaleY);

            Image background = Image.Image_createWithResIDQuad(Resources.Img.MenuPopup, 0);
            background.DoRestoreCutTransparency();
            background.scaleX = backgroundScaleX;
            background.scaleY = backgroundScaleY;
            _ = contentRoot.AddChild(background);

            PopupLayout layout = new(background.width, background.height, backgroundScaleX, backgroundScaleY);

            foreach (PopupTextBlock textBlock in template.TextBlocks)
            {
                if (textBlock.Scrollable)
                {
                    ScrollableContainer scroll = CreateScrollableText(popup, textBlock, layout);
                    _ = contentRoot.AddChild(scroll);
                }
                else
                {
                    Text text = CreateText(textBlock);
                    layout.PositionElement(text, textBlock.Anchor, textBlock.OffsetX, textBlock.OffsetY);
                    _ = contentRoot.AddChild(text);
                }
            }

            foreach (PopupElementBlock elementBlock in template.Elements)
            {
                BaseElement element = elementBlock.Element;
                element.anchor = elementBlock.ElementAnchor;
                layout.PositionElement(element, elementBlock.Anchor, elementBlock.OffsetX, elementBlock.OffsetY);
                _ = contentRoot.AddChild(element);
            }

            AddButtons(contentRoot, template, layout);

            popup.ShowPopup();
            _ = menuController.ActiveView().AddChild(popup);
            return popup;
        }

        /// <summary>
        /// Applies template scaling either to popup content or to the background, based on the template mode.
        /// </summary>
        private static void ApplyTemplateScale(Popup popup, PopupTemplate template, out float backgroundScaleX, out float backgroundScaleY)
        {
            float scaleX = 1f;
            float scaleY = 1f;

            switch (template.Size)
            {
                case PopupSize.Large:
                    scaleX = LargeScale;
                    scaleY = LargeScale;
                    break;
                case PopupSize.XLarge:
                    scaleX = XLargeScale;
                    scaleY = XLargeScale;
                    break;
                case PopupSize.Normal:
                default:
                    break;
            }

            // if (template.ScaleXOverride > 0f)
            // {
            //     scaleX = template.ScaleXOverride;
            // }
            // if (template.ScaleYOverride > 0f)
            // {
            //     scaleY = template.ScaleYOverride;
            // }

            if (template.ScaleMode == PopupScaleMode.Background)
            {
                popup.SetContentScale(1f, 1f);
                backgroundScaleX = scaleX;
                backgroundScaleY = scaleY;
                return;
            }

            popup.SetContentScale(scaleX, scaleY);
            backgroundScaleX = 1f;
            backgroundScaleY = 1f;
        }

        /// <summary>
        /// Creates a text element from a text block definition.
        /// </summary>
        private static Text CreateText(PopupTextBlock textBlock)
        {
            Text text = new Text().InitWithFont(Application.GetFont(textBlock.FontResourceName));
            text.SetAlignment(textBlock.Alignment);
            if (textBlock.WrapWidth > 0f)
            {
                text.SetStringandWidth(textBlock.Text, textBlock.WrapWidth);
            }
            else
            {
                text.SetString(textBlock.Text);
            }
            text.anchor = textBlock.ElementAnchor;
            return text;
        }

        /// <summary>
        /// Creates a scrollable text container for long content.
        /// </summary>
        private static ScrollableContainer CreateScrollableText(Popup popup, PopupTextBlock textBlock, PopupLayout layout)
        {
            float width = textBlock.WrapWidth > 0f ? textBlock.WrapWidth : DefaultScrollableWidth;
            float height = textBlock.ScrollHeight > 0f ? textBlock.ScrollHeight : DefaultScrollableHeight;

            Text text = CreateText(textBlock);
            text.anchor = 9; // top left
            text.parentAnchor = 9; // top left
            text.x = 0f;
            text.y = 0f;

            if (text.height > 0 && text.height < height)
            {
                height = text.height;
            }

            BaseElement content = new()
            {
                width = (int)width,
                height = text.height
            };
            _ = content.AddChild(text);

            ScrollableContainer scroll = new ScrollableContainer().InitWithWidthHeightContainer(width, height, content);
            scroll.anchor = textBlock.ElementAnchor;
            scroll.shouldBounceVertically = true;
            scroll.shouldBounceHorizontally = false;
            scroll.touchMoveIgnoreLength = 5f;
            scroll.resetScrollOnShow = true;
            layout.PositionElement(scroll, textBlock.Anchor, textBlock.OffsetX, textBlock.OffsetY);
            popup.RegisterScrollableContainer(scroll);
            return scroll;
        }

        /// <summary>
        /// Adds buttons to the popup based on the template layout rules.
        /// </summary>
        private void AddButtons(BaseElement contentRoot, PopupTemplate template, PopupLayout layout)
        {
            int buttonCount = template.Buttons.Count;
            if (buttonCount == 0)
            {
                return;
            }

            List<Button> buttons = [];
            foreach (PopupButtonSpec spec in template.Buttons)
            {
                Button button = spec.UseShortButton
                    ? MenuController.CreateShortButtonWithTextIDDelegate(spec.Label, spec.ButtonId, menuController)
                    : MenuController.CreateButtonWithTextIDDelegate(spec.Label, spec.ButtonId, menuController);
                button.anchor = FrameworkTypes.CENTER;
                buttons.Add(button);
            }

            Vector anchor = layout.GetScaledPosition(template.ButtonAnchor);
            float anchorX = anchor.X;
            float anchorY = anchor.Y;

            if (template.ButtonLayout == PopupButtonLayout.Horizontal)
            {
                float totalWidth = 0f;
                for (int i = 0; i < buttonCount; i++)
                {
                    totalWidth += buttons[i].width;
                }
                totalWidth += template.ButtonSpacing * (buttonCount - 1);

                float startX = anchorX - (totalWidth / 2f);
                for (int i = 0; i < buttonCount; i++)
                {
                    Button button = buttons[i];
                    button.x = startX + (button.width / 2f);
                    button.y = anchorY;
                    _ = contentRoot.AddChild(button);
                    startX += button.width + template.ButtonSpacing;
                }
                return;
            }

            float y = anchorY;
            for (int i = buttonCount - 1; i >= 0; i--)
            {
                Button button = buttons[i];
                button.x = anchorX;
                button.y = y;
                _ = contentRoot.AddChild(button);
                y -= button.height + template.ButtonSpacing;
            }
        }

        /// <summary>
        /// Named anchor points based on the popup texture quad offsets.
        /// </summary>
        internal enum PopupAnchor
        {
            Text1 = 1,
            Text2 = 2,
            Text3 = 3,
            Button = 4,
            StarsValue = 5
        }

        /// <summary>
        /// Supported popup sizing modes.
        /// </summary>
        internal enum PopupSize
        {
            Normal,
            Large,
            XLarge
        }

        /// <summary>
        /// Defines whether scaling affects content or only the popup background.
        /// </summary>
        internal enum PopupScaleMode
        {
            Content,
            Background
        }

        /// <summary>
        /// Button layout direction.
        /// </summary>
        internal enum PopupButtonLayout
        {
            Vertical,
            Horizontal
        }

        /// <summary>
        /// Defines all content and layout rules for building a popup.
        /// </summary>
        internal sealed class PopupTemplate(PopupSize size)
        {
            public PopupSize Size = size;
            public PopupScaleMode ScaleMode = PopupScaleMode.Content;
            // public float ScaleXOverride;
            // public float ScaleYOverride;
            public PopupButtonLayout ButtonLayout = PopupButtonLayout.Vertical;
            public float ButtonSpacing = DefaultButtonSpacing;
            public PopupAnchor ButtonAnchor = PopupAnchor.Button;
            // public float ButtonOffsetX;
            // public float ButtonOffsetY;
            public readonly List<PopupTextBlock> TextBlocks = [];
            public readonly List<PopupElementBlock> Elements = [];
            public readonly List<PopupButtonSpec> Buttons = [];

            /// <summary>
            /// Creates a new popup template with the specified size.
            /// </summary>
            public static PopupTemplate Create(PopupSize size = PopupSize.Normal)
            {
                return new(size);
            }

            /// <summary>
            /// Sets the scale mode for this popup.
            /// </summary>
            public PopupTemplate WithScaleMode(PopupScaleMode mode)
            {
                ScaleMode = mode;
                return this;
            }

            /// <summary>
            /// Sets the button layout direction.
            /// </summary>
            public PopupTemplate WithButtonLayout(PopupButtonLayout layout, float spacing = DefaultButtonSpacing)
            {
                ButtonLayout = layout;
                ButtonSpacing = spacing;
                return this;
            }

            /// <summary>
            /// Adds a text block to the popup.
            /// </summary>
            public PopupTemplate AddText(
                string text,
                string font,
                PopupAnchor anchor,
                float wrapWidth = -1f,
                float offsetX = 0f,
                float offsetY = 0f)
            {
                TextBlocks.Add(new PopupTextBlock(text, font, wrapWidth, anchor, offsetX, offsetY));
                return this;
            }

            /// <summary>
            /// Adds a scrollable text block to the popup.
            /// </summary>
            public PopupTemplate AddScrollableText(
                string text,
                string font,
                PopupAnchor anchor,
                float wrapWidth = -1f,
                float scrollHeight = 0f,
                float offsetX = 0f,
                float offsetY = 0f)
            {
                TextBlocks.Add(new PopupTextBlock(text, font, wrapWidth, anchor, offsetX, offsetY)
                {
                    Scrollable = true,
                    ScrollHeight = scrollHeight
                });
                return this;
            }

            /// <summary>
            /// Adds a custom element to the popup.
            /// </summary>
            public PopupTemplate AddElement(BaseElement element, PopupAnchor anchor, float offsetX = 0f, float offsetY = 0f)
            {
                Elements.Add(new PopupElementBlock(element, anchor, offsetX, offsetY));
                return this;
            }

            /// <summary>
            /// Adds a button to the popup.
            /// </summary>
            public PopupTemplate AddButton(string label, MenuButtonId buttonId, bool useShortButton = false)
            {
                Buttons.Add(new PopupButtonSpec(label, buttonId) { UseShortButton = useShortButton });
                return this;
            }
        }

        /// <summary>
        /// Defines a text block to be placed inside a popup.
        /// </summary>
        internal sealed class PopupTextBlock(
            string text,
            string fontResourceName,
            float wrapWidth,
            PopupAnchor anchor,
            float offsetX,
            float offsetY)
        {
            public string Text = text;
            public string FontResourceName = fontResourceName;
            public float WrapWidth = wrapWidth;
            public PopupAnchor Anchor = anchor;
            public float OffsetX = offsetX;
            public float OffsetY = offsetY;
            public int Alignment = 2;
            public sbyte ElementAnchor = FrameworkTypes.CENTER;
            public bool Scrollable;
            public float ScrollHeight;
        }

        /// <summary>
        /// Defines a non-text element to be placed inside a popup.
        /// </summary>
        internal sealed class PopupElementBlock(BaseElement element, PopupAnchor anchor, float offsetX, float offsetY)
        {
            public BaseElement Element = element;
            public PopupAnchor Anchor = anchor;
            public float OffsetX = offsetX;
            public float OffsetY = offsetY;
            public sbyte ElementAnchor = FrameworkTypes.CENTER;
        }

        /// <summary>
        /// Defines a popup button label and its associated menu button id.
        /// </summary>
        internal sealed class PopupButtonSpec(string label, MenuButtonId buttonId)
        {
            public string Label = label;
            public MenuButtonId ButtonId = buttonId;
            public bool UseShortButton;
        }

        /// <summary>
        /// Encapsulates popup background dimensions and scale factors for layout calculations.
        /// </summary>
        internal readonly struct PopupLayout(float width, float height, float scaleX, float scaleY)
        {
            public readonly float Width = width;
            public readonly float Height = height;
            public readonly float ScaleX = scaleX;
            public readonly float ScaleY = scaleY;

            /// <summary>
            /// Gets a scaled anchor position with optional offsets applied.
            /// </summary>
            public readonly Vector GetScaledPosition(PopupAnchor anchor, float offsetX = 0f, float offsetY = 0f)
            {
                Vector offset = Image.GetQuadOffset(Resources.Img.MenuPopup, (int)anchor);
                if (ScaleX == 1f && ScaleY == 1f)
                {
                    return new Vector(offset.X + offsetX, offset.Y + offsetY);
                }

                float centerX = Width / 2f;
                float centerY = Height / 2f;
                return new Vector(
                    centerX + ((offset.X - centerX) * ScaleX) + offsetX,
                    centerY + ((offset.Y - centerY) * ScaleY) + offsetY);
            }

            /// <summary>
            /// Positions an element at the specified anchor with offsets.
            /// </summary>
            public readonly void PositionElement(BaseElement element, PopupAnchor anchor, float offsetX, float offsetY)
            {
                Vector position = GetScaledPosition(anchor, offsetX, offsetY);
                element.x = position.X;
                element.y = position.Y;
            }
        }
    }
}
