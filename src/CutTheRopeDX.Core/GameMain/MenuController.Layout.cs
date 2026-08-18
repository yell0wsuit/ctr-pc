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

            LayOutCenteredScenes(visible);
            LayOutLevelSelect(visible);
            LayOutPackSelect(visible);
            LayOutAbout();
            CandySelectionView.Relayout(visible);
            LayOutReset(snapshot);

            // Every menu's navigation chrome is laid out together: it is the same element playing
            // the same role in each, and a scene that is not built yet simply has none to place.
            foreach (KeyValuePair<int, View> entry in views)
            {
                FloorChrome(entry.Value?.GetChildWithName("backb") as Button, snapshot);
                FloorChrome(entry.Value?.GetChildWithName("backButton") as Button, snapshot);
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
            PlaceFittedGroup(mainMenuGroup);
            Span(mainMenuSocial, visible);
            Span(moviePlate, visible);
            PlaceFittedGroup(optionsGroup);
            SetScale(optionsShadow, FullScreenScale(visible, 2f));
            PlaceFittedGroup(languageGroup);
            PlaceFittedGroup(resetGroup);
        }

        /// <summary>
        /// Lays out the level picker. Its backdrop is the pack's box cover, drawn once and once
        /// mirrored about the middle of the screen, so the seam between the halves is the anchor
        /// everything on it is placed from.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private void LayOutLevelSelect(CTRRectangle visible)
        {
            if (levelsCoverLeft == null)
            {
                return;
            }

            float scale = FullScreenScale(visible, 1f);
            float seam = visible.w / 2f;
            float coverWidth = levelsCoverLeft.width;
            float coverTop = LayoutMath.CornerAnchoredOffset(
                0f, levelsCoverLeft.height, scale, farEdge: false);

            // Each half is scaled about its own center, so its position is chosen to put the
            // scaled inner edge on the seam and the scaled top edge on the top of the screen.
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
                levelContainer.height = (int)(visible.h - LevelsTopInset);
            }
            else
            {
                // Capped so the widest row never grows past what the viewport actually shows -
                // FittedScale alone assumes content narrow enough never to approach the design
                // box's own edges, which a multi-column grid can violate well before that.
                float gridScale = levelsGridWidth > 0f
                    ? MathF.Min(FittedScale, visible.w / levelsGridWidth)
                    : FittedScale;
                PlaceFittedGroup(levelsGroup, gridScale);
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
        /// Places the two halves of the box binding, which the artwork positions relative to the
        /// middle of the design box, on the middle of whatever width the viewport exposes, at the
        /// same scale as the cover they sit on.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private void PlaceLevelSpines(CTRRectangle visible)
        {
            float scale = FullScreenScale(visible, 1f);
            PlaceLevelSpine(levelsSpineLeft, 6, visible, scale);
            PlaceLevelSpine(levelsSpineRight, 7, visible, scale);
        }

        /// <summary>
        /// Places one half of the box binding.
        /// </summary>
        /// <param name="spine">The binding half to place.</param>
        /// <param name="quad">Its quad in the level picker's texture, which carries its authored position.</param>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="scale">Scale the box cover is drawn at.</param>
        private static void PlaceLevelSpine(Image spine, int quad, CTRRectangle visible, float scale)
        {
            float authoredX = Image.GetQuadOffset(Resources.Img.MenuLevelUi, quad).X;
            float fromSeam = authoredX + (spine.width / 2f) - (ViewportLayout.DesignWidth / 2f);
            spine.scaleX = spine.scaleY = scale;
            spine.x = (visible.w / 2f) + (fromSeam * scale) - (spine.width / 2f);
            spine.y = LayoutMath.CornerAnchoredOffset(LevelsSpineTop, spine.height, scale, farEdge: false);
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
        /// Lays out the About view by rebuilding it when the content scale it was built at no
        /// longer matches the viewport.
        /// </summary>
        /// <remarks>
        /// Rebuilt rather than rescaled in place, for the same reason the pack picker is: the
        /// scale sets the wrap width every credits block was measured at, so it decides how many
        /// lines each becomes and therefore where every block below it sits. There is nothing to
        /// move that building it again does not place correctly. Where the reader had scrolled to
        /// survives, because a resize is not a reason to throw them back to the top.
        /// </remarks>
        private void LayOutAbout()
        {
            if (aboutView == null
                || GetView(VIEW_ABOUT) == null
                || aboutView.BuiltForScale == FittedScale)
            {
                return;
            }

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
            resetText?.SetStringandWidth(
                Application.GetString("RESET_TEXT"),
                VisibleBounds.w * ResetTextWidthShare);
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
        /// Grows a navigation button when the surface is dense or narrow enough that its drawn size
        /// would fall below a comfortable physical target, and leaves it alone otherwise. The touch
        /// zone is recomputed from the same scale, so where the button reacts follows where it is
        /// drawn.
        /// </summary>
        /// <param name="button">Button to size, or <see langword="null"/> when the scene has none.</param>
        /// <param name="snapshot">The viewport to size against.</param>
        private static void FloorChrome(Button button, ViewportLayoutSnapshot snapshot)
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

            float scale = MathF.Max(1f, HudMetrics.ChromeSize(snapshot, IsTouchHost) / longest);
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
        /// Whether the host drives the menus by touch. No host reports this yet, so the pointer
        /// branch applies everywhere; it is the conservative one, because it carries the floor.
        /// </summary>
        private const bool IsTouchHost = false;

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
