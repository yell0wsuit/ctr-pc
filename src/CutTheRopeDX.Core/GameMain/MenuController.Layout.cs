using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <content>
    /// The menu layout pass. Every rule here is written so it reduces to the constant the scene
    /// was authored with when the viewport exposes the design box, which is what keeps the shipped
    /// composition unchanged at that shape while letting every other shape reflow.
    /// </content>
    internal sealed partial class MenuController
    {
        /// <summary>
        /// The layers of one menu backdrop, kept so a layout pass can re-cover them without
        /// searching the element tree for them by position.
        /// </summary>
        /// <param name="Root">Element the backdrop layers hang from; spans the visible bounds.</param>
        /// <param name="Backdrop">The painted background image.</param>
        /// <param name="Front">The foreground half of the background, or <see langword="null"/>.</param>
        /// <param name="Shadow">The rotating light shaft layer, or <see langword="null"/>.</param>
        private readonly record struct MenuBackdrop(
            BaseElement Root,
            Image Backdrop,
            Image Front,
            Image Shadow);

        /// <summary>
        /// Menu backdrops by the view that owns them. Rebuilding a view replaces its entry rather
        /// than adding one, so a recreated view never leaves a stale backdrop behind.
        /// </summary>
        private readonly Dictionary<int, MenuBackdrop> backdrops = [];

        /// <summary>
        /// The reset view's confirmation text, re-wrapped whenever the viewport changes.
        /// </summary>
        private Text resetText;

        /// <summary>
        /// The main menu's design-space content. Fitting this one element positions and scales the
        /// logo and the button column together, the way the scene was composed.
        /// </summary>
        private BaseElement mainMenuGroup;


        /// <summary>The main menu's social-link tray, anchored to the bottom right.</summary>
        private BaseElement mainMenuSocial;

        /// <summary>The plate the movie view draws behind playback.</summary>
        private RectangleElement moviePlate;

        /// <summary>The options view's design-space content.</summary>
        private BaseElement optionsGroup;

        /// <summary>The options view's rotating light shaft, which it owns rather than the backdrop.</summary>
        private Image optionsShadow;

        /// <summary>The language picker's design-space content.</summary>
        private BaseElement languageGroup;

        /// <summary>The reset confirmation's design-space content.</summary>
        private BaseElement resetGroup;

        /// <summary>The left half of the level picker's box-cover backdrop.</summary>
        private Image levelsCoverLeft;

        /// <summary>The mirrored right half of the level picker's box-cover backdrop.</summary>
        private Image levelsCoverRight;

        /// <summary>The left half of the binding drawn over the box cover's seam.</summary>
        private Image levelsSpineLeft;

        /// <summary>The right half of the binding drawn over the box cover's seam.</summary>
        private Image levelsSpineRight;

        /// <summary>The level picker's rotating light shaft.</summary>
        private Image levelsShadow;

        /// <summary>The level picker's grid of level buttons.</summary>
        private VBox levelsBox;

        /// <summary>
        /// The level picker's design-space content: the grid, fitted independently of the box
        /// cover, spine, and shadow behind it, which stay on <see cref="FullScreenScale"/> so they
        /// keep covering the screen edge to edge. <see langword="null"/> for a pack large enough
        /// to scroll (<see cref="levelContainer"/> non-null instead) - see the comment in
        /// <see cref="LayOutLevelSelect"/> for why that case is not fitted too.
        /// </summary>
        private FittedGroup levelsGroup;

        /// <summary>
        /// Width of the grid's widest row, at design scale. The fitted scale assumes
        /// content narrow enough to never approach <see cref="ViewController.DesignBox"/>'s own
        /// width even boosted, which a multi-column grid can violate long before the design box's
        /// edges do; <see cref="LayOutLevelSelect"/> caps the scale actually applied against this
        /// so the grid never grows wider than the viewport has room for.
        /// </summary>
        private float levelsGridWidth;

        /// <summary>The level picker's total-stars-in-pack label, corner-anchored outside the grid's fitted group.</summary>
        private HBox levelsStarText;

        /// <summary>
        /// How many buttons per row the language picker was built with, so a viewport that fits a
        /// different number can be noticed.
        /// </summary>
        private int languageColumnsBuiltFor;

        /// <summary>
        /// The visible bounds the pack picker was built for. How many boxes fit across, and
        /// therefore the scroll points, follow from it, so a viewport of a different shape needs
        /// the view built again rather than nudged.
        /// </summary>
        private CTRRectangle packSelectBuiltFor;

        /// <inheritdoc />
        protected override void Relayout(ViewportLayoutSnapshot snapshot)
        {
            base.Relayout(snapshot);

            CTRRectangle visible = snapshot.VisibleBounds;

            foreach (MenuBackdrop backdrop in backdrops.Values)
            {
                LayOutBackdrop(backdrop, visible);
            }

            LayOutLanguageSelect();
            LayOutCenteredScenes(visible);
            LayOutLevelSelect(snapshot);
            LayOutPackSelect(visible);
            LayOutAbout(snapshot);
            CandySelectionView.Relayout(snapshot);
            LayOutReset(snapshot);

            // Every menu's navigation chrome is laid out together: it is the same element playing
            // the same role in each, and a scene that is not built yet simply has none to place.
            foreach (KeyValuePair<int, View> entry in views)
            {
                PlaceCornerChrome(BackButton(entry.Value), snapshot);
            }
        }

        /// <summary>
        /// Covers the visible bounds with a menu backdrop's layers.
        /// </summary>
        /// <param name="backdrop">Backdrop to lay out.</param>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private static void LayOutBackdrop(MenuBackdrop backdrop, CTRRectangle visible)
        {
            backdrop.Root.width = (int)visible.w;
            backdrop.Root.height = (int)visible.h;

            // Both halves of the painting scale about the same point - the bottom center of the
            // viewport - so one covering scale keeps them registered with each other.
            CoverFit fit = LayoutMath.Cover(
                backdrop.Backdrop.width,
                backdrop.Backdrop.height,
                visible);
            backdrop.Backdrop.scaleX = backdrop.Backdrop.scaleY = fit.Scale;
            SetScale(backdrop.Front, fit.Scale);
            SetScale(backdrop.Shadow, FullScreenScale(visible, 2f));
        }

        /// <summary>
        /// Spans an element across the visible bounds, ignoring one a scene has not built.
        /// </summary>
        /// <param name="element">Element to span, or <see langword="null"/>.</param>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private static void Span(BaseElement element, CTRRectangle visible)
        {
            if (element == null)
            {
                return;
            }

            element.width = (int)visible.w;
            element.height = (int)visible.h;
        }

        /// <summary>
        /// Scales an element uniformly, ignoring one a scene has not built.
        /// </summary>
        /// <param name="element">Element to scale, or <see langword="null"/>.</param>
        /// <param name="scale">Uniform scale to apply.</param>
        private static void SetScale(BaseElement element, float scale)
        {
            if (element == null)
            {
                return;
            }

            element.scaleX = element.scaleY = scale;
        }

        /// <summary>
        /// Lays out the scenes whose composition is nothing but a column or a plate centered in the
        /// viewport: the main menu, the movie plate, the options view and the language picker.
        /// Each keeps the anchors it was authored with, and spanning what those anchors resolve
        /// against is the whole of the conversion.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private void LayOutCenteredScenes(CTRRectangle visible)
        {
            // The main menu composes inside its design box, so the whole composition scales and
            // moves as one rather than each end being pinned to an edge on its own.
            PlaceFittedContent(mainMenuGroup);
            Span(mainMenuSocial, visible);
            Span(moviePlate, visible);
            PlaceFittedContent(optionsGroup);
            SetScale(optionsShadow, FullScreenScale(visible, 2f));
            PlaceFittedContent(languageGroup);
            PlaceFittedContent(resetGroup);
        }

        /// <summary>
        /// Lays out the level picker. Its backdrop is the pack's box cover, drawn once and once
        /// mirrored about the middle of the screen, so the seam between the halves is the anchor
        /// everything on it is placed from.
        /// </summary>
        /// <param name="snapshot">The viewport to lay out against.</param>
        private void LayOutLevelSelect(ViewportLayoutSnapshot snapshot)
        {
            if (levelsCoverLeft == null)
            {
                return;
            }

            CTRRectangle visible = snapshot.VisibleBounds;
            float scale = FullScreenScale(visible, 1f);
            float seam = visible.w / 2f;
            float coverWidth = levelsCoverLeft.width;
            float coverTopEdge = LevelCoverTopEdge(visible, levelsCoverLeft.height, scale);
            float coverTop = coverTopEdge + LayoutMath.CornerAnchoredOffset(
                0f, levelsCoverLeft.height, scale, farEdge: false);

            // Each half is scaled about its own center, so its position is chosen to put the
            // scaled inner edge on the seam and the scaled top edge where the cover starts.
            levelsCoverLeft.scaleX = levelsCoverLeft.scaleY = scale;
            levelsCoverLeft.x = seam - (coverWidth * (1f + scale) / 2f);
            levelsCoverLeft.y = coverTop;
            levelsCoverRight.scaleX = levelsCoverRight.scaleY = scale;
            levelsCoverRight.x = seam - (coverWidth * (1f - scale) / 2f);
            levelsCoverRight.y = coverTop - 0.5f;

            PlaceLevelSpines(visible);
            SetScale(levelsShadow, FullScreenScale(visible, 2f));

            // A pack with more levels than fit scrolls inside a container sized to the screen; one
            // that fits is fitted like a menu's design-space content instead.
            if (levelContainer != null)
            {
                // Not fitted: ScrollableContainer clips with a scissor rect computed from
                // drawX/width and the render backend's single global scale
                // (MonoGameRenderBackend.SetScissor), with no way to fold in an ancestor's
                // FittedScale on top of that. Nesting it under a scaled FittedGroup would clip at
                // the pre-scale size while the content draws at the larger, scaled one.
                levelsBox.width = (int)visible.w;
                levelContainer.width = (int)visible.w;

                // Stopped above the button in the corner as well as below the top inset. The grid
                // spans the whole width here, so unlike the fitted one it has no room to stand
                // clear of that corner sideways: the last row would be scrolled to a rest under
                // the button, and a tile under the button cannot be pressed.
                levelContainer.height = (int)(visible.h
                    - LevelsTopInset
                    - CornerChromeRect(BackButton(GetView(VIEW_LEVEL_SELECT)), snapshot).h);
            }
            else
            {
                // Capped so the widest row never grows past what the viewport actually shows -
                // FittedScale alone assumes content narrow enough never to approach the design
                // box's own edges, which a multi-column grid can violate well before that - and
                // then capped again against the chrome in the corners, which the grid would
                // otherwise be drawn under.
                float gridScale = levelsGridWidth > 0f
                    ? MathF.Min(FittedScale, visible.w / levelsGridWidth)
                    : FittedScale;
                gridScale = LevelGridFit.ScaleFor(
                    visible,
                    gridScale,
                    levelsGridWidth,
                    levelsBox?.height ?? 0f,
                    StarTotalRect(levelsStarText, visible),
                    CornerChromeRect(BackButton(GetView(VIEW_LEVEL_SELECT)), snapshot));
                PlaceFittedContent(levelsGroup, gridScale);
            }

            PlaceStarTotal(levelsStarText);
        }

        /// <summary>
        /// Places a total-stars label in the top-right corner, growing it from that corner so its
        /// authored insets scale along with it rather than the label enlarging in place.
        /// </summary>
        /// <remarks>
        /// The level picker places its label on every layout pass and the pack picker places its
        /// own as it builds one, but they are the same label in the same corner, so the insets
        /// live here rather than once per caller.
        /// </remarks>
        /// <param name="starText">Label to place, or <see langword="null"/> when the scene has none.</param>
        internal static void PlaceStarTotal(BaseElement starText)
        {
            if (starText == null)
            {
                return;
            }

            float scale = FittedScale;
            starText.scaleX = starText.scaleY = scale;
            starText.x = LayoutMath.CornerAnchoredOffset(StarTotalInsetX, starText.width, scale, farEdge: true);
            starText.y = LayoutMath.CornerAnchoredOffset(StarTotalInsetY, starText.height, scale, farEdge: false);
        }

        /// <summary>
        /// Where a total-stars label placed by <see cref="PlaceStarTotal"/> is drawn.
        /// </summary>
        /// <param name="starText">Label to measure, or <see langword="null"/> when the scene has none.</param>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <returns>The label's drawn rectangle, empty when there is no label.</returns>
        private static CTRRectangle StarTotalRect(BaseElement starText, CTRRectangle visible)
        {
            if (starText == null)
            {
                return new CTRRectangle(0f, 0f, 0f, 0f);
            }

            float scale = FittedScale;
            float width = starText.width * scale;
            float height = starText.height * scale;
            return new CTRRectangle(
                visible.w - width + StarTotalInsetX,
                StarTotalInsetY,
                width,
                height);
        }

        /// <summary>
        /// The navigation button in a view's bottom-left corner, whichever of the two names the
        /// scenes give it.
        /// </summary>
        /// <remarks>
        /// The level picker calls it one thing and every other scene the other. Asked for by one
        /// name alone, the picker's button came back as nothing at all - and the grid, told there
        /// was no chrome to clear, was drawn straight over it.
        /// </remarks>
        /// <param name="view">View to look in, or <see langword="null"/>.</param>
        /// <returns>The button, or <see langword="null"/> when the view has none.</returns>
        private static Button BackButton(View view)
        {
            return (view?.GetChildWithName("backb") ?? view?.GetChildWithName("backButton")) as Button;
        }

        /// <summary>
        /// Where the navigation button in the bottom-left corner is drawn.
        /// </summary>
        /// <param name="button">Button to measure, or <see langword="null"/> when the scene has none.</param>
        /// <param name="snapshot">The viewport to measure against.</param>
        /// <returns>The room the button takes in that corner, empty when there is no button.</returns>
        private static CTRRectangle CornerChromeRect(Button button, ViewportLayoutSnapshot snapshot)
        {
            if (button == null)
            {
                return new CTRRectangle(0f, 0f, 0f, 0f);
            }

            ChromeRoom room = HudMetrics.RoomFor(
                snapshot,
                button.width,
                button.height,
                HudMetrics.IsTouchHost);
            return new CTRRectangle(
                0f,
                snapshot.VisibleBounds.h - room.Height,
                room.Width,
                room.Height);
        }

        /// <summary>
        /// Places the two halves of the box binding, which the artwork positions relative to the
        /// middle of the design box, on the middle of whatever width the viewport exposes, at the
        /// same scale as the cover they sit on.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private void PlaceLevelSpines(CTRRectangle visible)
        {
            float scale = FullScreenScale(visible, 1f);
            float coverTopEdge = LevelCoverTopEdge(visible, levelsCoverLeft.height, scale);
            PlaceLevelSpine(levelsSpineLeft, 6, visible, scale, coverTopEdge);
            PlaceLevelSpine(levelsSpineRight, 7, visible, scale, coverTopEdge);
        }

        /// <summary>
        /// Where the top edge of the level picker's box cover is drawn.
        /// </summary>
        /// <remarks>
        /// The cover is authored to fill the design box exactly and is grown to cover whatever the
        /// viewport exposes instead, which on a window wider than the design shape makes it taller
        /// than the screen. That growth is split between the top and the bottom, because the
        /// painting is lit about its own middle: hung entirely off the bottom, as it was, a window
        /// half again as wide as 16:9 put the cover's middle on the bottom edge of the screen and
        /// left the dark end of it across the top.
        /// </remarks>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="coverHeight">The cover's authored height.</param>
        /// <param name="scale">Scale the cover is drawn at.</param>
        /// <returns>The cover's drawn top edge, at or above the top of the screen.</returns>
        private static float LevelCoverTopEdge(CTRRectangle visible, float coverHeight, float scale)
        {
            return (visible.h - (coverHeight * scale)) / 2f;
        }

        /// <summary>
        /// Places one half of the box binding.
        /// </summary>
        /// <param name="spine">The binding half to place.</param>
        /// <param name="quad">Its quad in the level picker's texture, which carries its authored position.</param>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="scale">Scale the box cover is drawn at.</param>
        /// <param name="coverTopEdge">Where the cover the binding sits on starts.</param>
        private static void PlaceLevelSpine(
            Image spine,
            int quad,
            CTRRectangle visible,
            float scale,
            float coverTopEdge)
        {
            float authoredX = Image.GetQuadOffset(Resources.Img.MenuLevelUi, quad).X;
            float fromSeam = authoredX + (spine.width / 2f) - (ViewportLayout.DesignWidth / 2f);
            spine.scaleX = spine.scaleY = scale;
            spine.x = (visible.w / 2f) + (fromSeam * scale) - (spine.width / 2f);

            // Measured down from the cover rather than from the screen: the binding is part of the
            // painting, so it travels with it when the cover is taller than the screen.
            spine.y = coverTopEdge
                + LayoutMath.CornerAnchoredOffset(LevelsSpineTop, spine.height, scale, farEdge: false);
        }

        /// <summary>
        /// Lays out the pack picker by rebuilding it when the viewport no longer has the shape it
        /// was built for. How many boxes fit across drives the container width, the scroll points
        /// and every frame drawn around them, so there is nothing to move that building it again
        /// does not place correctly.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private void LayOutPackSelect(CTRRectangle visible)
        {
            if (GetView(VIEW_PACK_SELECT) == null
                || (packSelectBuiltFor.w == visible.w && packSelectBuiltFor.h == visible.h))
            {
                return;
            }

            bool wasActive = activeViewID == VIEW_PACK_SELECT;
            DeleteView(VIEW_PACK_SELECT);
            CreatePackSelect();
            if (wasActive)
            {
                GetView(VIEW_PACK_SELECT).Show();
            }
        }

        /// <summary>
        /// Lays out the language picker by rebuilding it when the viewport fits a different number
        /// of buttons across than it was built with.
        /// </summary>
        /// <remarks>
        /// Rebuilt rather than reflowed: the rows are plain buttons, cheap to make again, and the
        /// picker holds nothing a rebuild would lose.
        /// </remarks>
        private void LayOutLanguageSelect()
        {
            if (GetView(VIEW_LANGUAGE_SELECT) == null || LanguageColumns() == languageColumnsBuiltFor)
            {
                return;
            }

            bool wasActive = activeViewID == VIEW_LANGUAGE_SELECT;
            DeleteView(VIEW_LANGUAGE_SELECT);
            CreateLanguageSelection();
            if (wasActive)
            {
                GetView(VIEW_LANGUAGE_SELECT).Show();
            }
        }

        /// <summary>
        /// Lays out the About view: the window the credits scroll inside follows the viewport on
        /// every pass, and the credits themselves are rebuilt when the content scale they were
        /// laid out at no longer matches it.
        /// </summary>
        /// <remarks>
        /// Rebuilt rather than rescaled in place, for the same reason the pack picker is: the
        /// scale sets the wrap width every credits block was measured at, so it decides how many
        /// lines each becomes and therefore where every block below it sits. There is nothing to
        /// move that building it again does not place correctly. Where the reader had scrolled to
        /// survives, because a resize is not a reason to throw them back to the top.
        /// </remarks>
        /// <param name="snapshot">The viewport to lay out against.</param>
        private void LayOutAbout(ViewportLayoutSnapshot snapshot)
        {
            if (aboutView == null || GetView(VIEW_ABOUT) == null)
            {
                return;
            }

            if (aboutView.BuiltForScale != FittedScale)
            {
                Vector scroll = aboutView.ScrollOffset;
                bool autoScroll = aboutView.AutoScrollEnabled;
                bool wasActive = activeViewID == VIEW_ABOUT;

                DeleteView(VIEW_ABOUT);
                CreateAbout();

                aboutView.ScrollOffset = scroll;
                aboutView.AutoScrollEnabled = autoScroll;
                if (wasActive)
                {
                    GetView(VIEW_ABOUT).Show();
                }
            }

            aboutView.ResizeWindow(snapshot);
        }

        /// <summary>
        /// Lays out the reset confirmation view.
        /// </summary>
        /// <param name="snapshot">The viewport to lay out against.</param>
        private void LayOutReset(ViewportLayoutSnapshot snapshot)
        {
            _ = snapshot;
            WrapResetText();
        }

        /// <summary>
        /// Re-wraps the reset confirmation text to the width the viewport allows.
        /// </summary>
        /// <remarks>
        /// The wrap width is a length in the same space the text is laid out in, so it comes from
        /// the visible bounds rather than the surface. Measuring it in pixels made the column
        /// track the window's pixel count instead of its logical width: too narrow on a dense
        /// display, and wider than the screen on a large one.
        /// </remarks>
        private void WrapResetText()
        {
            // Divided by the scale the group it hangs in is drawn at, so what ends up that share
            // of the screen is the drawn line rather than the one measured before the group grew.
            // Measured in logical units alone it ran off both edges of a phone screen by the whole
            // of the boost.
            resetText?.SetStringandWidth(
                Application.GetString("RESET_TEXT"),
                VisibleBounds.w * ResetTextWidthShare / FittedScale);
        }

        /// <summary>
        /// Scales a decorative layer authored to cover the design box by however much the viewport
        /// exceeds that box on either axis, so it still reaches every edge. Returns
        /// <paramref name="authoredScale"/> unchanged at the design shape.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="authoredScale">The scale the layer ships with.</param>
        /// <returns>The scale to draw the layer at.</returns>
        private static float FullScreenScale(CTRRectangle visible, float authoredScale)
        {
            float growth = MathF.Max(
                1f,
                LayoutMath.Cover(ViewportLayout.DesignWidth, ViewportLayout.DesignHeight, visible).Scale);
            return authoredScale * growth;
        }

        /// <summary>
        /// Sizes a navigation button at the larger of the scale the menus around it are drawn at
        /// and the size the surface needs it to be to stay physically reachable. The touch zone is
        /// recomputed from the same scale, so where the button reacts follows where it is drawn.
        /// </summary>
        /// <remarks>
        /// The two answer different questions - one is how far this viewport is from the shape the
        /// game was drawn for, the other how small this button is allowed to get in the player's
        /// hand - and the button has to satisfy both, so it takes whichever asks for more. Held at
        /// the floor alone it was drawn at its authored size on every ordinary window, which is
        /// where a phone-shaped one left it: a button the size 16:9 draws it, in a corner of a menu
        /// whose every other element had grown by half.
        /// </remarks>
        /// <param name="button">Button to size, or <see langword="null"/> when the scene has none.</param>
        /// <param name="snapshot">The viewport to size against.</param>
        private static void PlaceCornerChrome(Button button, ViewportLayoutSnapshot snapshot)
        {
            if (button == null)
            {
                return;
            }

            float longest = MathF.Max(button.width, button.height);
            if (longest <= 0f)
            {
                return;
            }

            float scale = HudMetrics.ChromeScale(snapshot, longest, HudMetrics.IsTouchHost);
            button.scaleX = button.scaleY = scale;

            // Growing about its own center would push a button anchored into the bottom-left corner
            // out through it, so the growth is taken back out of its offset from that corner. The
            // authored offset is zero on both axes: the button sits in the corner itself.
            button.x = LayoutMath.CornerAnchoredOffset(0f, button.width, scale, farEdge: false);
            button.y = LayoutMath.CornerAnchoredOffset(0f, button.height, scale, farEdge: true);

            // The button is scaled about its own center, so the forced touch rectangle - which the
            // back button carries to match its art rather than its bounding box - moves with it.
            CTRTexture2D texture = Application.GetTexture(Resources.Img.MenuExtraButtons);
            Vector offset = texture.quadOffsets[0];
            CTRRectangle quad = texture.quadRects[0];
            float centerX = button.width / 2f;
            float centerY = button.height / 2f;
            button.ForceTouchRect(new CTRRectangle(
                centerX + ((offset.X - centerX) * scale),
                centerY + ((offset.Y - centerY) * scale),
                quad.w * scale,
                quad.h * scale));
        }

        /// <summary>
        /// Places one of the pieces drawn against the edges of the pack strip - a frame down one
        /// of its sides, or a navigation arrow beside it - at the strip's scale.
        /// </summary>
        /// <remarks>
        /// A piece is scaled about its own center, so the drift that puts on the edge it was
        /// authored against is taken back out of its position here: the same correction a
        /// corner-anchored element needs, measured against the strip's edge rather than the
        /// screen's. Which edge that is, the piece's own anchor names.
        /// </remarks>
        /// <param name="element">Piece to place.</param>
        /// <param name="edgeX">Where the piece's anchored edge belongs, in logical space.</param>
        /// <param name="scale">Scale the strip is drawn at.</param>
        /// <param name="mirroredX">Whether the piece is drawn mirrored horizontally.</param>
        /// <param name="mirroredY">Whether the piece is drawn mirrored vertically.</param>
        private static void PlacePackEdge(
            BaseElement element,
            float edgeX,
            float scale,
            bool mirroredX,
            bool mirroredY)
        {
            float drift = element.width * (1f - scale) / 2f;
            bool anchoredRight = (element.anchor & 4) != 0;
            element.x = anchoredRight ? edgeX + drift : edgeX - drift;
            element.scaleX = mirroredX ? -scale : scale;
            element.scaleY = mirroredY ? -scale : scale;
        }

        /// <summary>Distance from the top of the screen to the scrolling level grid.</summary>
        private const float LevelsTopInset = 110f;

        /// <summary>Distance from the top of the box cover to the top of its binding.</summary>
        private const float LevelsSpineTop = 80f;

        /// <summary>Smallest gap between a pack picker arrow and the edge of the screen.</summary>
        private const float PackArrowInset = 10f;

        /// <summary>Authored offset of a total-stars label from the right edge of the screen.</summary>
        private const float StarTotalInsetX = -30f;

        /// <summary>Authored offset of a total-stars label from the top edge of the screen.</summary>
        private const float StarTotalInsetY = 40f;

        /// <summary>Share of the visible width the reset confirmation text wraps within.</summary>
        private const float ResetTextWidthShare = 0.95f;

    }
}
