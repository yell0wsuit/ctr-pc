using System.Collections.Generic;
using System.Globalization;

using CutTheRope.Commons;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Creates and displays menu popups using reusable templates and layout helpers.
    /// </summary>
    internal sealed class PopUpMenu(MenuController controller)
    {
        private const float LargeScale = 1.2f;
        private const float XLargeScale = 1.5f;
        private const float DefaultScrollableWidth = 700f;
        private const float DefaultScrollableHeight = 300f;
        private const float DefaultButtonSpacing = 0f;

        private readonly MenuController menuController = controller;

        /// <summary>
        /// Shows the "can't unlock" popup with required stars and explanatory text.
        /// </summary>
        public void ShowCantUnlockPopup()
        {
            const int textOffset = 20;
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            int totalStars = CTRPreferences.GetTotalStars();
            string requiredStars = (CTRPreferences.PackUnlockStars(cTRRootController.GetPack() + 1) - totalStars)
                .ToString(CultureInfo.InvariantCulture);

            PopupTemplate template = new(PopupSize.Large)
            {
                ScaleMode = PopupScaleMode.Background,
                TextBlocks =
                {
                    new PopupTextBlock(
                        Application.GetString("CANT_UNLOCK_TEXT1"),
                        Resources.Fnt.BigFont,
                        -1f,
                        PopupAnchor.Text1,
                        0f,
                        -textOffset),
                    new PopupTextBlock(
                        Application.GetString("CANT_UNLOCK_TEXT2"),
                        Resources.Fnt.BigFont,
                        -1f,
                        PopupAnchor.Text2,
                        0f,
                        -textOffset),
                    new PopupTextBlock(
                        Application.GetString("CANT_UNLOCK_TEXT3"),
                        Resources.Fnt.SmallFont,
                        600f,
                        PopupAnchor.Text3,
                        0f,
                        50f),
                },
                Elements =
                {
                    new PopupElementBlock(
                        MenuController.CreateTextWithStar(requiredStars),
                        PopupAnchor.StarsValue,
                        0f,
                        -textOffset),
                },
                Buttons =
                {
                    new PopupButtonSpec(Application.GetString("OK"), MenuButtonId.PopupOk),
                }
            };

            _ = ShowTemplatePopup(template);
        }

        /// <summary>
        /// Shows the "game finished" popup with completion text and an OK button.
        /// </summary>
        public void ShowGameFinishedPopup()
        {
            PopupTemplate template = new(PopupSize.Normal)
            {
                ScaleMode = PopupScaleMode.Background,
                TextBlocks =
                {
                    new PopupTextBlock(
                        Application.GetString("GAME_FINISHED_TEXT"),
                        Resources.Fnt.BigFont,
                        600f,
                        PopupAnchor.Text2,
                        0f,
                        -170f),
                    new PopupTextBlock(
                        Application.GetString("GAME_FINISHED_TEXT2"),
                        Resources.Fnt.SmallFont,
                        700f,
                        PopupAnchor.Text3,
                        0f,
                        30f),
                },
                Buttons =
                {
                    new PopupButtonSpec(Application.GetString("OK"), MenuButtonId.PopupOk),
                }
            };

            _ = ShowTemplatePopup(template);
        }

        /// <summary>
        /// Shows a confirmation popup with Yes/No buttons.
        /// </summary>
        /// <param name="str">Main message to display.</param>
        /// <param name="buttonYesId">Menu button id for the "Yes" action.</param>
        /// <param name="buttonNoId">Menu button id for the "No" action.</param>
        /// <returns>The created popup instance.</returns>
        public Popup ShowYesNoPopup(string str, MenuButtonId buttonYesId, MenuButtonId buttonNoId)
        {
            PopupTemplate template = new(PopupSize.Normal)
            {
                ScaleMode = PopupScaleMode.Background,
                TextBlocks =
                {
                    new PopupTextBlock(
                        str,
                        Resources.Fnt.BigFont,
                        680f,
                        PopupAnchor.Text2,
                        0f,
                        -120f)
                    {
                        Scrollable = false
                    },
                },
                Buttons =
                {
                    new PopupButtonSpec(Application.GetString("YES"), buttonYesId),
                    new PopupButtonSpec(Application.GetString("NO"), buttonNoId),
                }
            };

            return ShowTemplatePopup(template);
        }

        /// <summary>
        /// Builds and shows a popup from the provided template definition.
        /// </summary>
        /// <param name="template">Template describing the popup's content and layout.</param>
        /// <returns>The created popup instance.</returns>
        public Popup ShowTemplatePopup(PopupTemplate template)
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

            float backgroundWidth = background.width;
            float backgroundHeight = background.height;

            foreach (PopupTextBlock textBlock in template.TextBlocks)
            {
                if (textBlock.Scrollable)
                {
                    ScrollableContainer scroll = CreateScrollableText(
                        popup,
                        textBlock,
                        backgroundWidth,
                        backgroundHeight,
                        backgroundScaleX,
                        backgroundScaleY);
                    _ = contentRoot.AddChild(scroll);
                }
                else
                {
                    Text text = CreateText(textBlock);
                    PositionAtAnchor(
                        text,
                        textBlock.Anchor,
                        textBlock.OffsetX,
                        textBlock.OffsetY,
                        backgroundWidth,
                        backgroundHeight,
                        backgroundScaleX,
                        backgroundScaleY);
                    _ = contentRoot.AddChild(text);
                }
            }

            foreach (PopupElementBlock elementBlock in template.Elements)
            {
                BaseElement element = elementBlock.Element;
                element.anchor = elementBlock.ElementAnchor;
                PositionAtAnchor(
                    element,
                    elementBlock.Anchor,
                    elementBlock.OffsetX,
                    elementBlock.OffsetY,
                    backgroundWidth,
                    backgroundHeight,
                    backgroundScaleX,
                    backgroundScaleY);
                _ = contentRoot.AddChild(element);
            }

            AddButtons(
                contentRoot,
                template,
                backgroundWidth,
                backgroundHeight,
                backgroundScaleX,
                backgroundScaleY);

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
                    break;
                default:
                    break;
            }

            if (template.ScaleXOverride > 0f)
            {
                scaleX = template.ScaleXOverride;
            }
            if (template.ScaleYOverride > 0f)
            {
                scaleY = template.ScaleYOverride;
            }

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
        private static ScrollableContainer CreateScrollableText(
            Popup popup,
            PopupTextBlock textBlock,
            float backgroundWidth,
            float backgroundHeight,
            float backgroundScaleX,
            float backgroundScaleY)
        {
            float width = textBlock.WrapWidth > 0f ? textBlock.WrapWidth : DefaultScrollableWidth;
            float height = textBlock.ScrollHeight > 0f ? textBlock.ScrollHeight : DefaultScrollableHeight;

            Text text = CreateText(textBlock);
            text.anchor = 9;
            text.parentAnchor = 9;
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
            PositionAtAnchor(
                scroll,
                textBlock.Anchor,
                textBlock.OffsetX,
                textBlock.OffsetY,
                backgroundWidth,
                backgroundHeight,
                backgroundScaleX,
                backgroundScaleY);
            popup.RegisterScrollableContainer(scroll);
            return scroll;
        }

        /// <summary>
        /// Adds buttons to the popup based on the template layout rules.
        /// </summary>
        private void AddButtons(
            BaseElement contentRoot,
            PopupTemplate template,
            float backgroundWidth,
            float backgroundHeight,
            float backgroundScaleX,
            float backgroundScaleY)
        {
            int buttonCount = template.Buttons.Count;
            if (buttonCount == 0)
            {
                return;
            }

            List<Button> buttons = [];
            _ = buttons.EnsureCapacity(buttonCount);
            foreach (PopupButtonSpec spec in template.Buttons)
            {
                Button button = spec.UseShortButton
                    ? MenuController.CreateShortButtonWithTextIDDelegate(spec.Label, spec.ButtonId, menuController)
                    : MenuController.CreateButtonWithTextIDDelegate(spec.Label, spec.ButtonId, menuController);
                button.anchor = FrameworkTypes.CENTER;
                buttons.Add(button);
            }

            Vector anchor = GetAnchorOffset(
                template.ButtonAnchor,
                backgroundWidth,
                backgroundHeight,
                backgroundScaleX,
                backgroundScaleY);
            float anchorX = anchor.X + template.ButtonOffsetX;
            float anchorY = anchor.Y + template.ButtonOffsetY;

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
        /// Positions an element relative to a popup anchor quad with optional offsets.
        /// </summary>
        private static void PositionAtAnchor(
            BaseElement element,
            PopupAnchor anchor,
            float offsetX,
            float offsetY,
            float backgroundWidth,
            float backgroundHeight,
            float backgroundScaleX,
            float backgroundScaleY)
        {
            Vector position = GetAnchorOffset(
                anchor,
                backgroundWidth,
                backgroundHeight,
                backgroundScaleX,
                backgroundScaleY);
            element.x = position.X + offsetX;
            element.y = position.Y + offsetY;
        }

        /// <summary>
        /// Gets the quad offset used for a given popup anchor.
        /// </summary>
        private static Vector GetAnchorOffset(
            PopupAnchor anchor,
            float backgroundWidth,
            float backgroundHeight,
            float backgroundScaleX,
            float backgroundScaleY)
        {
            Vector offset = Image.GetQuadOffset(Resources.Img.MenuPopup, (int)anchor);
            if (backgroundScaleX == 1f && backgroundScaleY == 1f)
            {
                return offset;
            }

            float centerX = backgroundWidth / 2f;
            float centerY = backgroundHeight / 2f;
            return new Vector(
                centerX + ((offset.X - centerX) * backgroundScaleX),
                centerY + ((offset.Y - centerY) * backgroundScaleY));
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
            public float ScaleXOverride;
            public float ScaleYOverride;
            public PopupButtonLayout ButtonLayout = PopupButtonLayout.Vertical;
            public float ButtonSpacing = DefaultButtonSpacing;
            public PopupAnchor ButtonAnchor = PopupAnchor.Button;
            public float ButtonOffsetX;
            public float ButtonOffsetY;
            public readonly List<PopupTextBlock> TextBlocks = [];
            public readonly List<PopupElementBlock> Elements = [];
            public readonly List<PopupButtonSpec> Buttons = [];
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
    }
}
