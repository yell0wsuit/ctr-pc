using System.Globalization;

using CutTheRope.Commons;
using CutTheRope.Framework.Core;

using static CutTheRope.Commons.PopupBuilder;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Provides pre-built popup dialogs for common menu scenarios.
    /// </summary>
    internal sealed class PopUpMenu(MenuController controller)
    {
        private readonly PopupBuilder builder = new(controller);

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

            PopupTemplate template = PopupTemplate.Create(PopupSize.Large)
                .WithScaleMode(PopupScaleMode.Background)
                .AddText(Application.GetString("CANT_UNLOCK_TEXT1"), Resources.Fnt.BigFont, PopupAnchor.Text1, offsetY: -textOffset)
                .AddText(Application.GetString("CANT_UNLOCK_TEXT2"), Resources.Fnt.BigFont, PopupAnchor.Text2, offsetY: -textOffset)
                .AddText(Application.GetString("CANT_UNLOCK_TEXT3"), Resources.Fnt.SmallFont, PopupAnchor.Text3, wrapWidth: 600f, offsetY: 50f)
                .AddElement(MenuController.CreateTextWithStar(requiredStars), PopupAnchor.StarsValue, offsetY: -textOffset)
                .AddButton(Application.GetString("OK"), MenuButtonId.PopupOk);

            _ = builder.Show(template);
        }

        /// <summary>
        /// Shows the "game finished" popup with completion text and an OK button.
        /// </summary>
        public void ShowGameFinishedPopup()
        {
            PopupTemplate template = PopupTemplate.Create()
                .WithScaleMode(PopupScaleMode.Background)
                .AddText(Application.GetString("GAME_FINISHED_TEXT"), Resources.Fnt.BigFont, PopupAnchor.Text2, wrapWidth: 600f, offsetY: -170f)
                .AddText(Application.GetString("GAME_FINISHED_TEXT2"), Resources.Fnt.SmallFont, PopupAnchor.Text3, wrapWidth: 700f, offsetY: 30f)
                .AddButton(Application.GetString("OK"), MenuButtonId.PopupOk);

            _ = builder.Show(template);
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
            PopupTemplate template = PopupTemplate.Create()
                .WithScaleMode(PopupScaleMode.Background)
                .AddText(str, Resources.Fnt.BigFont, PopupAnchor.Text2, wrapWidth: 680f, offsetY: -120f)
                .AddButton(Application.GetString("YES"), buttonYesId)
                .AddButton(Application.GetString("NO"), buttonNoId);

            return builder.Show(template);
        }
    }
}
