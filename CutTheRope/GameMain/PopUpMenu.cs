using System.Collections.Generic;
using System.Globalization;

using CutTheRope.Commons;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class PopUpMenu(MenuController controller)
    {
        private const float LargeScale = 1.2f;
        private const float DefaultScrollableWidth = 700f;
        private const float DefaultScrollableHeight = 300f;
        private const float DefaultButtonSpacing = 0f;

        private readonly MenuController menuController = controller;

        public void ShowCantUnlockPopup()
        {
            const int textOffset = 20;
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            int totalStars = CTRPreferences.GetTotalStars();
            string requiredStars = (CTRPreferences.PackUnlockStars(cTRRootController.GetPack() + 1) - totalStars)
                .ToString(CultureInfo.InvariantCulture);

            PopupTemplate template = new(PopupSize.Large)
            {
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

        public void ShowGameFinishedPopup()
        {
            PopupTemplate template = new(PopupSize.Normal)
            {
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

        public Popup ShowYesNoPopup(string str, MenuButtonId buttonYesId, MenuButtonId buttonNoId)
        {
            PopupTemplate template = new(PopupSize.Normal)
            {
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

        public Popup ShowTemplatePopup(PopupTemplate template)
        {
            Popup popup = new();
            popup.SetName("popup");

            BaseElement contentRoot = popup.ContentRoot;
            ApplyTemplateScale(popup, template);

            Image background = Image.Image_createWithResIDQuad(Resources.Img.MenuPopup, 0);
            background.DoRestoreCutTransparency();
            _ = contentRoot.AddChild(background);

            foreach (PopupTextBlock textBlock in template.TextBlocks)
            {
                if (textBlock.Scrollable)
                {
                    ScrollableContainer scroll = CreateScrollableText(popup, textBlock);
                    _ = contentRoot.AddChild(scroll);
                }
                else
                {
                    Text text = CreateText(textBlock);
                    PositionAtAnchor(text, textBlock.Anchor, textBlock.OffsetX, textBlock.OffsetY);
                    _ = contentRoot.AddChild(text);
                }
            }

            foreach (PopupElementBlock elementBlock in template.Elements)
            {
                BaseElement element = elementBlock.Element;
                element.anchor = elementBlock.ElementAnchor;
                PositionAtAnchor(element, elementBlock.Anchor, elementBlock.OffsetX, elementBlock.OffsetY);
                _ = contentRoot.AddChild(element);
            }

            AddButtons(contentRoot, template);

            popup.ShowPopup();
            _ = menuController.ActiveView().AddChild(popup);
            return popup;
        }

        private static void ApplyTemplateScale(Popup popup, PopupTemplate template)
        {
            float scaleX = 1f;
            float scaleY = 1f;
            if (template.Size == PopupSize.Large)
            {
                scaleX = LargeScale;
                scaleY = LargeScale;
            }

            if (template.ScaleXOverride > 0f)
            {
                scaleX = template.ScaleXOverride;
            }
            if (template.ScaleYOverride > 0f)
            {
                scaleY = template.ScaleYOverride;
            }

            popup.SetContentScale(scaleX, scaleY);
        }

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

        private static ScrollableContainer CreateScrollableText(Popup popup, PopupTextBlock textBlock)
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
            PositionAtAnchor(scroll, textBlock.Anchor, textBlock.OffsetX, textBlock.OffsetY);
            popup.RegisterScrollableContainer(scroll);
            return scroll;
        }

        private void AddButtons(BaseElement contentRoot, PopupTemplate template)
        {
            int buttonCount = template.Buttons.Count;
            if (buttonCount == 0)
            {
                return;
            }

            List<Button> buttons = new(buttonCount);
            foreach (PopupButtonSpec spec in template.Buttons)
            {
                Button button = spec.UseShortButton
                    ? MenuController.CreateShortButtonWithTextIDDelegate(spec.Label, spec.ButtonId, menuController)
                    : MenuController.CreateButtonWithTextIDDelegate(spec.Label, spec.ButtonId, menuController);
                button.anchor = FrameworkTypes.CENTER;
                buttons.Add(button);
            }

            Vector anchor = GetAnchorOffset(template.ButtonAnchor);
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

        private static void PositionAtAnchor(BaseElement element, PopupAnchor anchor, float offsetX, float offsetY)
        {
            Image.SetElementPositionWithQuadOffset(element, Resources.Img.MenuPopup, (int)anchor);
            element.x += offsetX;
            element.y += offsetY;
        }

        private static Vector GetAnchorOffset(PopupAnchor anchor)
        {
            return Image.GetQuadOffset(Resources.Img.MenuPopup, (int)anchor);
        }

        internal enum PopupAnchor
        {
            Text1 = 1,
            Text2 = 2,
            Text3 = 3,
            Button = 4,
            StarsValue = 5
        }

        internal enum PopupSize
        {
            Normal,
            Large
        }

        internal enum PopupButtonLayout
        {
            Vertical,
            Horizontal
        }

        internal sealed class PopupTemplate(PopupSize size)
        {
            public PopupSize Size = size;
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

        internal sealed class PopupElementBlock(BaseElement element, PopupAnchor anchor, float offsetX, float offsetY)
        {
            public BaseElement Element = element;
            public PopupAnchor Anchor = anchor;
            public float OffsetX = offsetX;
            public float OffsetY = offsetY;
            public sbyte ElementAnchor = FrameworkTypes.CENTER;
        }

        internal sealed class PopupButtonSpec(string label, MenuButtonId buttonId)
        {
            public string Label = label;
            public MenuButtonId ButtonId = buttonId;
            public bool UseShortButton;
        }
    }
}
