using System.Globalization;

using CutTheRope.Commons;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using static CutTheRope.GameMain.ResDataPhoneFull;

namespace CutTheRope.GameMain
{
    internal sealed class PopUpMenu(MenuController controller)
    {
        private readonly MenuController menuController = controller;

        public void ShowCantUnlockPopup()
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            Popup popup = new();
            popup.SetName("popup");
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuPopup, 0);
            image.DoRestoreCutTransparency();
            _ = popup.AddChild(image);
            int num = 20;
            image.scaleX = 1.3f;
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetAlignment(2);
            text.SetString(Application.GetString(STR_MENU_CANT_UNLOCK_TEXT1));
            text.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text, Resources.Img.MenuPopup, 1);
            text.y -= num;
            _ = popup.AddChild(text);
            Text text2 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text2.SetAlignment(2);
            text2.SetString(Application.GetString(STR_MENU_CANT_UNLOCK_TEXT2));
            text2.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text2, Resources.Img.MenuPopup, 2);
            _ = popup.AddChild(text2);
            text2.y -= num;
            Text text3 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            text3.SetAlignment(2);
            text3.SetStringandWidth(Application.GetString(STR_MENU_CANT_UNLOCK_TEXT3), 600f);
            text3.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text3, Resources.Img.MenuPopup, 3);
            text3.y += 50f;
            _ = popup.AddChild(text3);
            int totalStars = CTRPreferences.GetTotalStars();
            HBox hBox = MenuController.CreateTextWithStar((CTRPreferences.PackUnlockStars(cTRRootController.GetPack() + 1) - totalStars).ToString(CultureInfo.InvariantCulture));
            hBox.anchor = 18;
            Image.SetElementPositionWithQuadOffset(hBox, Resources.Img.MenuPopup, 5);
            hBox.y -= num;
            _ = popup.AddChild(hBox);
            Button button = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_OK), MenuButtonId.PopupOk, menuController);
            button.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button, Resources.Img.MenuPopup, 4);
            _ = popup.AddChild(button);
            popup.ShowPopup();
            _ = menuController.ActiveView().AddChild(popup);
        }

        public void ShowGameFinishedPopup()
        {
            Popup popup = new();
            popup.SetName("popup");
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuPopup, 0);
            image.DoRestoreCutTransparency();
            _ = popup.AddChild(image);
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetAlignment(2);
            text.SetStringandWidth(Application.GetString(STR_MENU_GAME_FINISHED_TEXT), 600.0);
            text.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text, Resources.Img.MenuPopup, 2);
            text.y -= 170f;
            _ = image.AddChild(text);
            Text text2 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            text2.SetAlignment(2);
            text2.SetStringandWidth(Application.GetString(STR_MENU_GAME_FINISHED_TEXT2), 700.0);
            text2.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text2, Resources.Img.MenuPopup, 3);
            text2.y += 30f;
            _ = image.AddChild(text2);
            Button button = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_OK), MenuButtonId.PopupOk, menuController);
            button.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button, Resources.Img.MenuPopup, 4);
            _ = image.AddChild(button);
            popup.ShowPopup();
            _ = menuController.ActiveView().AddChild(popup);
        }

        public Popup ShowYesNoPopup(string str, MenuButtonId buttonYesId, MenuButtonId buttonNoId)
        {
            Popup popup = new();
            popup.SetName("popup");
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuPopup, 0);
            image.DoRestoreCutTransparency();
            _ = popup.AddChild(image);
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetAlignment(2);
            text.SetStringandWidth(str, 680.0);
            text.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text, Resources.Img.MenuPopup, 2);
            text.y -= 120f;
            _ = image.AddChild(text);
            Button button = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_YES), buttonYesId, menuController);
            button.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button, Resources.Img.MenuPopup, 4);
            button.y -= button.height;
            _ = image.AddChild(button);
            Button button2 = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_NO), buttonNoId, menuController);
            button2.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button2, Resources.Img.MenuPopup, 4);
            _ = image.AddChild(button2);
            popup.ShowPopup();
            _ = menuController.ActiveView().AddChild(popup);
            return popup;
        }
    }
}
