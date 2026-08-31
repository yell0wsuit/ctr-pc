using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Core gameplay scene that owns the loaded level state, interactive objects, and HUD.
    /// </summary>
    internal sealed partial class GameScene : BaseElement, ITimelineDelegate, IButtonDelegation, IRocketDelegate
    {
        /// <summary>
        /// Returns the largest value from four candidates.
        /// </summary>
        /// <param name="v1">The first candidate value.</param>
        /// <param name="v2">The second candidate value.</param>
        /// <param name="v3">The third candidate value.</param>
        /// <param name="v4">The fourth candidate value.</param>
        /// <returns>The maximum of the supplied values.</returns>
        private static float MaxOf4(float v1, float v2, float v3, float v4)
        {
            return v1 >= v2 && v1 >= v3 && v1 >= v4
                ? v1
                : v2 >= v1 && v2 >= v3 && v2 >= v4 ? v2 : v3 >= v2 && v3 >= v1 && v3 >= v4 ? v3 : v4 >= v2 && v4 >= v3 && v4 >= v1 ? v4 : -1f;
        }

        /// <summary>
        /// Returns the smallest value from four candidates.
        /// </summary>
        /// <param name="v1">The first candidate value.</param>
        /// <param name="v2">The second candidate value.</param>
        /// <param name="v3">The third candidate value.</param>
        /// <param name="v4">The fourth candidate value.</param>
        /// <returns>The minimum of the supplied values.</returns>
        private static float MinOf4(float v1, float v2, float v3, float v4)
        {
            return v1 <= v2 && v1 <= v3 && v1 <= v4
                ? v1
                : v2 <= v1 && v2 <= v3 && v2 <= v4 ? v2 : v3 <= v2 && v3 <= v1 && v3 <= v4 ? v3 : v4 <= v2 && v4 <= v3 && v4 <= v1 ? v4 : -1f;
        }

        /// <summary>
        /// Determines whether a constrained point has moved outside the playable screen bounds.
        /// </summary>
        /// <param name="p">The point to evaluate.</param>
        /// <returns><see langword="true"/> when the point is beyond the allowed bounds; otherwise, <see langword="false"/>.</returns>
        public bool PointOutOfScreen(ConstraintedPoint p)
        {
            // Mobile matches the WP7 kill bounds (+100/-50, scaled x3); the horizontal
            // margin stays as a safety net in both modes (the reference kills on Y only).
            float bottomMargin = ActivePhysicsConstants.UseMobilePhysicsModel ? 300f : 400f;
            float topMargin = ActivePhysicsConstants.UseMobilePhysicsModel ? 150f : 400f;
            return p.pos.Y > mapHeight + bottomMargin || p.pos.Y < -topMargin
                || p.pos.X < -SCREEN_WIDTH || p.pos.X > mapWidth + SCREEN_WIDTH;
        }

        /// <summary>
        /// Handles completion of XML loading for the current map and restarts the scene state.
        /// </summary>
        /// <param name="rootNode">The loaded XML root node.</param>
        /// <param name="_">The XML loader source identifier.</param>
        /// <param name="_1">The XML loader success flag.</param>
        public void XmlLoaderFinishedWithfromwithSuccess(XElement rootNode, string _, bool _1)
        {
            CTRRootController rootController = (CTRRootController)Application.SharedRootController();
            string resolvedMapName = ResolveMapName(_);
            rootController.PrepareMapAndEnsureResources(rootNode, resolvedMapName);
            if (animateRestartDim)
            {
                AnimateLevelRestart();
                return;
            }
            Restart();
        }

        /// <summary>
        /// Resolves the persisted map name from an XML loader source string.
        /// </summary>
        /// <param name="source">The XML loader source path or virtual identifier.</param>
        /// <returns>The filename for disk-backed maps, or the current root-controller map name for virtual reload sources.</returns>
        private static string ResolveMapName(string source)
        {
            return string.IsNullOrWhiteSpace(source) || source.Contains("://", StringComparison.Ordinal)
                ? ((CTRRootController)Application.SharedRootController()).GetMapName()
                : Path.GetFileName(source);
        }

        /// <summary>
        /// Determines whether a tutorial element should be skipped for the active language.
        /// </summary>
        /// <param name="c">The tutorial XML element to inspect.</param>
        /// <returns><see langword="true"/> when the element does not match the active locale; otherwise, <see langword="false"/>.</returns>
        public static bool ShouldSkipTutorialElement(XElement c)
        {
            string currentLang = LanguageHelper.CurrentCode;
            string locale = c.Attribute("locale")?.Value ?? string.Empty;
            return LanguageHelper.IsUiLanguageCode(currentLang) ? locale != currentLang : locale != "en";
        }

        /// <summary>
        /// Plays the active Om Nom greeting animation and matching sounds.
        /// </summary>
        public void ShowGreeting()
        {
            // On a two-Om-Nom level, randomly greet with the mutual chat instead of the wave.
            // TryShowChatGreeting returns false for diagonal/coincident pairs, falling back here.
            if (targets.Count == 2 && RND_RANGE(0, 1) == 0 && TryShowChatGreeting())
            {
                return;
            }

            // General greeting: the primary Om Nom waves.
            targetAnimationController?.PlayGreeting();
            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterGreeting, targetAnimationController?.SkinDefinition);
            if (SpecialEvents.IsXmas && Preferences.GetIntForKey("PREFS_SELECTED_OMNOM") == 0)
            {
                CTRSoundMgr.PlaySound(Resources.Snd.XmasBell);
            }
        }

        /// <summary>
        /// Rolls for the two-Om-Nom chat greeting as a random idle reaction. Called when an
        /// Om Nom's idle timer fires on an eligible level. When it starts a chat, both Om Noms'
        /// idle timers are reset so the reaction is not re-triggered mid-hand-off.
        /// </summary>
        /// <returns><see langword="true"/> when a chat greeting was started; otherwise, <see langword="false"/>.</returns>
        private bool TryStartChatReaction()
        {
            if (targets.Count != 2
                || targets[0].Idle.HasPendingChat
                || targets[1].Idle.HasPendingChat
                || targets[0].Feeding.IsFed
                || targets[1].Feeding.IsFed
                || RND_RANGE(0, ChatReactionOdds - 1) != 0)
            {
                return false;
            }

            if (!TryShowChatGreeting())
            {
                return false;
            }

            targets[0].Idle.ScheduleIdle(RND_RANGE(5, 20));
            targets[1].Idle.ScheduleIdle(RND_RANGE(5, 20));
            return true;
        }

        /// <summary>
        /// Makes the two Om Noms turn their heads toward each other. A randomly chosen Om Nom
        /// turns first; the other follows a fixed 1.0s later, matching Time Travel's chat
        /// hand-off. Each Om Nom's direction is auto-detected from position; diagonal or
        /// coincident pairs are skipped.
        /// </summary>
        /// <returns><see langword="true"/> when a chat greeting was started; otherwise, <see langword="false"/>.</returns>
        private bool TryShowChatGreeting()
        {
            if (targets.Count != 2 || targets[0].targetObject == null || targets[1].targetObject == null)
            {
                return false;
            }

            (TargetAnimationState first, TargetAnimationState second)? states = ChatGreeting.ResolveStates(
                targets[0].targetObject.x, targets[0].targetObject.y,
                targets[1].targetObject.x, targets[1].targetObject.y);
            if (!states.HasValue)
            {
                return false;
            }

            // Each Om Nom's direction is fixed by position; only the order is randomized.
            int firstIndex = RND_RANGE(0, 1);
            int secondIndex = 1 - firstIndex;
            TargetAnimationState firstState = firstIndex == 0 ? states.Value.first : states.Value.second;
            TargetContext secondTarget = targets[secondIndex];
            TargetAnimationState secondState = secondIndex == 0 ? states.Value.first : states.Value.second;
            if (!secondTarget.Idle.TryScheduleChat(secondState))
            {
                return false;
            }

            // Initiator turns now; the other follows a fixed beat later.
            targets[firstIndex].controller?.PlayGreetingTurn(firstState);
            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterGreeting, targets[firstIndex].controller?.SkinDefinition);

            dd.CallObjectSelectorParamafterDelay(
                new DelayedDispatcher.DispatchFunc(Selector_showSecondChatGreeting), null, ChatGreetingGapSeconds);
            return true;
        }

        /// <summary>
        /// Timeline selector callback that plays the second Om Nom's chat greeting one
        /// fixed beat after the first.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_showSecondChatGreeting(FrameworkTypes param)
        {
            if (targets.Count != 2)
            {
                return;
            }

            TargetContext second = null;
            TargetAnimationState state = default;
            for (int ti = 0; ti < targets.Count; ti++)
            {
                if (targets[ti].Idle.TryConsumeChat(out state))
                {
                    second = targets[ti];
                    break;
                }
            }

            if (second?.targetObject == null)
            {
                return;
            }

            second.controller?.PlayGreetingTurn(state);
            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterGreeting, second.controller?.SkinDefinition);
        }

        /// <inheritdoc />
        public override void Hide()
        {
            gravityState.RemoveButtonsFrom(this);
            if (waterLayer != null)
            {
                waterLayer.PrepareToRelease();
                waterLayer.Dispose();
                waterLayer = null;
            }
            // The half bodies belong to the primary candy's lifecycle, which goes away with the scene;
            // only a half-finished load can still be holding one here.
            pendingLeftHalf = null;
            pendingRightHalf = null;
            Lantern.RemoveAllLanterns();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dd?.Dispose();
                dd = null;
                camera?.Dispose();
                camera = null;
                back?.Dispose();
                back = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Updates scene layout when fullscreen mode changes.
        /// </summary>
        /// <param name="isFullscreen">Whether fullscreen mode is enabled.</param>
        public void FullscreenToggled(bool isFullscreen)
        {
            _ = isFullscreen;
            RelayoutHud();
        }

        /// <summary>
        /// Places the in-game chrome for the current viewport.
        /// </summary>
        /// <remarks>
        /// The collected-stars row is scaled by <see cref="FrameworkTypes.ContentScale"/>, the
        /// same boost menu content gets on a narrow viewport, read here rather than passed in: a
        /// parameter meant one caller could hand the HUD a scale that disagreed with the rest of
        /// the frame, and the one that took the default handed it 1 on every fullscreen toggle.
        /// The row is grown from the screen's top-left corner rather than about each icon's own
        /// center, so it stays flush against the corner instead of bleeding past it: each icon is
        /// center-anchored, which already keeps it centered on its own position as it scales, so
        /// multiplying that position by the scale is enough to grow the whole row from the corner
        /// with no extra correction term.
        /// </remarks>
        public void RelayoutHud()
        {
            float hudScale = ContentScale;
            PlaceLevelLabel();

            for (int i = 0; i < 3; i++)
            {
                int starSize = hudStar[i].width;
                hudStar[i].x = ((starSize * i) + (starSize / 2)) * hudScale;
                hudStar[i].y = hudStar[i].height / 2f * hudScale;
                hudStar[i].scaleX = hudStar[i].scaleY = hudScale;
            }
            UpdateBackgroundScale();
        }

        /// <summary>
        /// Projects the camera onto the viewport again, without a frame of gameplay having run.
        /// </summary>
        /// <remarks>
        /// The fit is otherwise applied by <see cref="Update"/>, which a paused scene never gets:
        /// a window resized while the pause menu was up left the world drawn for the shape it had
        /// when it was paused, and the background - whose cover scale is measured through the
        /// camera's - short of the edges it no longer reached. Safe to call outside the update
        /// loop, because the fit reads the tracked position and writes only what gets drawn.
        /// </remarks>
        public void RelayoutCamera()
        {
            ApplyCameraFit(ScreenPresentation.Instance.Snapshot);
        }

        /// <summary>
        /// Sizes and positions the level-name label for the current viewport.
        /// </summary>
        /// <remarks>
        /// Both insets are margins from their own edge of the viewport, so both grow with the HUD
        /// the label belongs to: a label drawn half again as large against a margin that stayed
        /// where it was would sit proportionally tighter into the corner the larger it got. The
        /// Y offset was once a negative fine-tune of the font's metrics and was deliberately held
        /// unscaled for that reason; it is a real margin now, and is treated like the X one.
        /// </remarks>
        private void PlaceLevelLabel()
        {
            BaseElement levelLabel = staticAniPool?.GetChildWithName("levelLabel");
            if (levelLabel == null)
            {
                return;
            }

            float hudScale = ContentScale;
            levelLabel.scaleX = levelLabel.scaleY = hudScale;
            levelLabel.x = LayoutMath.CornerAnchoredOffset(LevelLabelInsetX, levelLabel.width, hudScale, farEdge: false);

            bool isChinese = LanguageHelper.IsCurrentAny(Language.LANGZH, Language.LANGZHTW);
            float bottomInset = isChinese ? LevelLabelInsetYCJK : LevelLabelInsetY;
            levelLabel.y = VisibleBounds.h
                + LayoutMath.CornerAnchoredOffset(-bottomInset, levelLabel.height, hudScale, farEdge: true);
        }

        /// <summary>
        /// Computes the scale that makes a background texture cover the region of world the
        /// screen exposes, letting the axis with room to spare crop.
        /// </summary>
        /// <remarks>
        /// The art is one screen of the shape the game was drawn for, and the camera decides how
        /// much world a screen of the current shape sees. Covering that region is a cover fit
        /// against it: on a window taller in proportion, the height drives the scale and the
        /// width crops; on a wider one, the width drives it. A fit by width alone leaves the art
        /// short of the top and bottom edges of a tall window, and grows so much on a level whose
        /// map is narrower than the screen that the repeat's seam is dragged into view. At the
        /// shape the art was drawn for the two axes agree, so this is the authored scale exactly.
        /// </remarks>
        /// <param name="texture">Background texture to measure.</param>
        /// <returns>A safe cover scale for the background texture.</returns>
        private float GetBackgroundCoverScale(CTRTexture2D texture)
        {
            if (texture == null || texture._realWidth <= 0 || texture._realHeight <= 0)
            {
                return 1f;
            }

            float cameraScale = camera?.Scale ?? 1f;
            if (cameraScale <= 0f || float.IsNaN(cameraScale) || float.IsInfinity(cameraScale))
            {
                return 1f;
            }

            // The region of world a screen of this shape exposes, which is the viewport taken back
            // through the camera's own scale. Covering that is an ordinary cover fit against it.
            CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
            CTRRectangle worldWindow = new(0f, 0f, visible.w / cameraScale, visible.h / cameraScale);
            float scale = LayoutMath.Cover(texture._realWidth, texture._realHeight, worldWindow).Scale;
            return scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale) ? 1f : scale;
        }

        /// <summary>
        /// Sizes and places the background for the region of world the screen currently exposes.
        /// </summary>
        private void UpdateBackgroundScale()
        {
            backgroundScale = GetBackgroundCoverScale(backTexture);
            if (back == null || backTexture == null)
            {
                return;
            }

            back.scaleX = backgroundScale;
            back.scaleY = backgroundScale;

            // Centered on the design screen: the frame the art and every level's placement inside
            // it were drawn against. Growing about that center carries the art out past whichever
            // edges the window reveals, where growing about its top-left corner would only push it
            // down and to the right and leave the far edges to the repeat. It stays put in the
            // world as the camera moves, so a level taller than the screen still scrolls past it.
            back.x = (SCREEN_WIDTH / 2f / backgroundScale) - (backTexture._realWidth / 2f);
            back.y = (SCREEN_HEIGHT / 2f / backgroundScale) - (backTexture._realHeight / 2f);
            gravityState.RelayoutEarthAnimations(back.x, back.y);
        }

        /// <summary>
        /// The range the tracking may drive the camera through: the positions of the design-box
        /// window's top-left corner that keep the window inside the level.
        /// </summary>
        /// <remarks>
        /// Measured from <see cref="cameraBounds"/>'s own origin rather than from world zero. A
        /// level wider than the design box is laid out centered on it, so its left edge sits at a
        /// negative world X; a range starting at zero would strand that half outside what the
        /// camera can reach and pin it against the right edge for everything past the level's
        /// midpoint. An axis the level does not exceed contributes no extent, leaving the one
        /// position that shows the whole level on that axis.
        /// </remarks>
        /// <returns>The origin of the range and its extent on each axis.</returns>
        private CTRRectangle CameraTrackingRange()
        {
            return new CTRRectangle(
                cameraBounds.x,
                cameraBounds.y,
                MathF.Max(0f, mapWidth - SCREEN_WIDTH),
                MathF.Max(0f, mapHeight - SCREEN_HEIGHT));
        }

        /// <summary>Clamps a desired camera position into <see cref="CameraTrackingRange"/>.</summary>
        /// <param name="x">Desired camera X, in world units.</param>
        /// <param name="y">Desired camera Y, in world units.</param>
        /// <returns>The clamped position.</returns>
        private Vector BoundedCameraPosition(float x, float y)
        {
            CTRRectangle range = CameraTrackingRange();
            return Vect(
                FIT_TO_BOUNDARIES(x, range.x, range.x + range.w),
                FIT_TO_BOUNDARIES(y, range.y, range.y + range.h));
        }

        /// <summary>
        /// Fits the camera to the level for the current viewport, holding it centered when the
        /// whole level is already visible.
        /// </summary>
        /// <param name="snapshot">The viewport to fit against.</param>
        private void ApplyCameraFit(ViewportLayoutSnapshot snapshot)
        {
            if (cameraBounds.w <= 0f || cameraBounds.h <= 0f)
            {
                return;
            }

            CTRRectangle viewport = snapshot.VisibleBounds;

            // The fit scales cameraWindow, never the whole map, so the scale - and with it how
            // much world beyond the window the viewport exposes - is settled before any anchor is
            // chosen. Computing it here lets the anchor be made against that exposed slack.
            float scale = MathF.Min(viewport.w / cameraWindow.w, viewport.h / cameraWindow.h);
            float slackX = (viewport.w / scale) - cameraWindow.w;
            float slackY = (viewport.h / scale) - cameraWindow.h;

            // On an axis the level exceeds, cameraWindow is capped to the design size rather than
            // the full map, so it is the window - not the whole map - that fits the viewport; the
            // anchor slides that window through the map's scrollable range first, exactly where
            // the legacy bounded-pixel camera put it, before the fit's own anchor distributes
            // whatever slack remains on an axis the level does not exceed.
            float scrollableX = MathF.Max(0f, cameraBounds.w - cameraWindow.w);
            float scrollableY = MathF.Max(0f, cameraBounds.h - cameraWindow.h);

            // The anchor is read from where the tracking has driven the camera, which nothing here
            // writes back to. A fit that took its anchor from its own previous result would
            // subtract the viewport's slack afresh on every pass and walk the camera off the level.
            float anchorX = GameplayCamera.Anchor(camera.pos.X, cameraBounds.x, scrollableX, slackX);
            float anchorY = GameplayCamera.Anchor(camera.pos.Y, cameraBounds.y, scrollableY, slackY);

            CTRRectangle window = new(
                cameraBounds.x + (scrollableX * anchorX),
                cameraBounds.y + (scrollableY * anchorY),
                cameraWindow.w,
                cameraWindow.h);

            camera.ApplyFit(LayoutMath.FitCamera(window, viewport, anchorX, anchorY));
            RelayoutWaterCoverage(snapshot);
        }

        /// <summary>Extends water through any world exposed beyond the authored level frame.</summary>
        private void RelayoutWaterCoverage(ViewportLayoutSnapshot snapshot)
        {
            if (camera.Scale <= 0f)
            {
                return;
            }

            CTRRectangle viewport = snapshot.VisibleBounds;
            if (pauseSwitcherWaves != null)
            {
                pauseSwitcherWaves.x = viewport.x;
                pauseSwitcherWaves.y = viewport.y;
                pauseSwitcherWaves.Resize(viewport.w, viewport.h);
            }
            if (waterLayer == null)
            {
                return;
            }

            float visibleLeft = camera.RenderPos.X;
            float visibleRight = visibleLeft + (viewport.w / camera.Scale);
            float visibleBottom = camera.RenderPos.Y + (viewport.h / camera.Scale);

            float authoredLeft = mapWidth < SCREEN_WIDTH ? 0f : mapOriginX;
            float authoredRight = mapWidth < SCREEN_WIDTH ? SCREEN_WIDTH : mapOriginX + mapWidth;
            waterLayer.RelayoutCoverage(
                MathF.Min(authoredLeft, visibleLeft),
                MathF.Max(authoredRight, visibleRight),
                MathF.Max(mapOriginY + mapHeight, visibleBottom));
            waterLayer.RelayoutViewportClip(camera.RenderPos, camera.Scale);
        }

        /// <summary>
        /// Timeline selector callback that transitions the scene to the lost state.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_gameLost(FrameworkTypes param)
        {
            GameLost();
        }

        /// <summary>
        /// Timeline selector callback that transitions the scene to the won state.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_gameWon(FrameworkTypes param)
        {
            CTRSoundMgr.EnableLoopedSounds(false);
            if (!gameplayFlow.CompleteWinTransition())
            {
                // A restart claimed the level while the win was still presenting. Leave the result
                // pending: Show clears it, so the abandoned win never reaches the results box.
                return;
            }

            if (pendingLevelResult is LevelResult result)
            {
                pendingLevelResult = null;
                gameSceneDelegate?.GameWon(result);
            }
        }

        /// <summary>
        /// Timeline selector callback that begins the level restart animation.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_animateLevelRestart(FrameworkTypes param)
        {
            AnimateLevelRestart();
        }

        /// <summary>
        /// Timeline selector callback that triggers the greeting animation and sound.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_showGreeting(FrameworkTypes param)
        {
            ShowGreeting();
        }

        /// <summary>
        /// Timeline selector callback that starts the candy blink animation.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_doCandyBlink(FrameworkTypes param)
        {
            DoCandyBlink();
        }

        /// <summary>
        /// Delayed-dispatch callback that completes one candy transport.
        /// </summary>
        /// <param name="param">
        /// The <see cref="CandyTransportSession"/> enqueued when the candy entered the transporter.
        /// Anything else is ignored, as is a session the candy has since replaced.
        /// </param>
        private void Selector_teleport(FrameworkTypes param)
        {
            Teleport(param as CandyTransportSession);
        }

        /// <summary>
        /// Restores the candy transform and color after temporary visual effects (singleton = candies[0]).
        /// </summary>
        private void RestoreCandyProperties()
        {
            RestoreCandyProperties(candies[0]);
        }

        /// <summary>
        /// Restores a specific candy's transform and color after temporary visual effects.
        /// </summary>
        private static void RestoreCandyProperties(CandyContext ctx)
        {
            ctx.WholeBody.Visual.passTransformationsToChilds = false;
            foreach (BaseElement visual in ctx.HandCatchVisuals())
            {
                visual.scaleX = visual.scaleY = ctx.HandCatchScale;
            }
            ctx.WholeBody.Visual.color = RGBAColor.solidOpaqueRGBA;
        }

        /// <summary>
        /// Resolves the logical candy that owns <paramref name="point"/> - either its whole body's
        /// point or, for a split candy, one of its halves' points.
        /// </summary>
        /// <param name="point">The physics point to resolve.</param>
        /// <returns>
        /// The owning candy, or <see langword="null"/> when no candy owns the point. It used to fall
        /// back to <c>candies[0]</c>, which silently handed an unrelated point to the primary candy;
        /// a split half only resolved that way by accident, because the primary happens to own the
        /// halves. Callers decide what an unowned point means.
        /// </returns>
        private CandyContext CandyForPointOrNull(ConstraintedPoint point)
        {
            if (point == null)
            {
                return null;
            }

            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (ctx.WholeBody.Point == point)
                {
                    return ctx;
                }

                SplitCandyState split = ctx.Lifecycle.Split;
                if (split != null
                    && (split.Left.Body.Point == point || split.Right.Body.Point == point))
                {
                    return ctx;
                }
            }

            return null;
        }

        /// <summary>
        /// Timeline selector callback that restores the candy after it leaves a lantern.
        /// </summary>
        /// <param name="param">Released candy physics point.</param>
        private void Selector_revealCandyFromLantern(FrameworkTypes param)
        {
            ConstraintedPoint releasedPoint = param as ConstraintedPoint;
            _ = LanternRelease.RestoreReleasedCandy(candies, releasedPoint);
        }

        /// <summary>
        /// Normalizes an angle into the range of negative PI to positive PI.
        /// </summary>
        /// <param name="a">The angle to normalize.</param>
        /// <returns>The normalized angle.</returns>
        public static float FBOUND_PI(float a)
        {
            return a > MathF.PI ? a - MathF.Tau : a < -MathF.PI ? a + MathF.Tau : a;
        }

        /// <summary>
        /// Handles exhaustion of a rocket currently carrying the candy.
        /// </summary>
        /// <param name="r">The rocket that has exhausted its fuel.</param>
        public void Exhausted(Rocket r)
        {
            CandyContext ctx = RocketBoundCandy(r);
            if (ctx != null)
            {
                _ = ctx.Lifecycle.Attachments.TryReleaseRocket(r);
                ctx.WholeBody.Point.disableGravity = IsCandyGravitySuppressed(ctx);
            }
        }

        /// <summary>
        /// Chooses the equivalent source angle nearest to a target angle.
        /// </summary>
        /// <param name="ta">The target angle to compare against.</param>
        /// <param name="fa">The source angle to normalize near the target.</param>
        /// <returns>The source angle adjusted by full rotations to be closest to the target angle.</returns>
        private static float NearestAngleTofrom(float ta, float fa)
        {
            float minus360 = fa - DEG_360;
            float plus360 = fa + DEG_360;
            return MathF.Abs(fa - ta) < MathF.Abs(minus360 - ta) && MathF.Abs(fa - ta) < MathF.Abs(plus360 - ta)
                ? fa
                : MathF.Abs(minus360 - ta) < MathF.Abs(plus360 - ta) ? minus360 : NearestAngleTofrom(ta, plus360);
        }

        /// <summary>
        /// Calculates the smallest angular difference between two angles.
        /// </summary>
        /// <param name="a">The first angle.</param>
        /// <param name="b">The second angle.</param>
        /// <returns>The absolute minimum angle between the two inputs.</returns>
        private static float MinAngleBetweenAandB(float a, float b)
        {
            float normalizedDelta;
            for (normalizedDelta = MathF.Abs(a - b); normalizedDelta > DEG_360; normalizedDelta -= DEG_360)
            {
            }
            normalizedDelta = MathF.Abs(normalizedDelta);
            if (normalizedDelta > DEG_180)
            {
                normalizedDelta -= DEG_360;
            }
            return MathF.Abs(normalizedDelta);
        }

        /// <summary>
        /// The maximum number of concurrent touches tracked by the scene.
        /// </summary>
        public const int MAX_TOUCHES = 5;

        /// <summary>
        /// The delay before the restart dim animation advances.
        /// </summary>
        public const float DIM_TIMEOUT = 0.15f;

        /// <summary>
        /// Restart state value for fading the dim overlay in.
        /// </summary>
        public const int RESTART_STATE_FADE_IN = 0;

        /// <summary>
        /// Restart state value for fading the dim overlay out.
        /// </summary>
        public const int RESTART_STATE_FADE_OUT = 1;

        /// <summary>
        /// Selector state for moving an element downward.
        /// </summary>
        public const int S_MOVE_DOWN = 0;

        /// <summary>
        /// Selector state for waiting between movements.
        /// </summary>
        public const int S_WAIT = 1;

        /// <summary>
        /// Selector state for moving an element upward.
        /// </summary>
        public const int S_MOVE_UP = 2;

        /// <summary>
        /// Camera mode that tracks a split candy half.
        /// </summary>
        public const int CAMERA_MOVE_TO_CANDY_PART = 0;

        /// <summary>
        /// Camera mode that tracks the full candy body.
        /// </summary>
        public const int CAMERA_MOVE_TO_CANDY = 1;

        /// <summary>
        /// Button identifier for the gravity toggle.
        /// </summary>
        public const int BUTTON_GRAVITY = 0;

        /// <summary>
        /// The timeout window for combo rope cuts.
        /// </summary>
        public const float SCOMBO_TIMEOUT = 0.2f;

        /// <summary>
        /// The score awarded for a rope cut.
        /// </summary>
        public const int SCUT_SCORE = 10;

        /// <summary>
        /// The maximum number of candy losses allowed before game over handling changes.
        /// </summary>
        public const int MAX_LOST_CANDIES = 3;

        /// <summary>
        /// The time window for counting multiple ropes cut at once.
        /// </summary>
        public const float ROPE_CUT_AT_ONCE_TIMEOUT = 0.1f;

        /// <summary>
        /// The pickup radius used for stars.
        /// </summary>
        public const int STAR_RADIUS = 42;

        /// <summary>
        /// The radius around Om Nom that triggers mouth-open behavior.
        /// </summary>
        public const float MOUTH_OPEN_RADIUS = 200f;

        /// <summary>
        /// The frame skip interval applied to blink timing.
        /// </summary>
        public const int BLINK_SKIP = 3;

        /// <summary>
        /// The duration to keep the mouth-open state active.
        /// </summary>
        public const float MOUTH_OPEN_TIME = 1f;

        /// <summary>
        /// The minimum delay between pump activations.
        /// </summary>
        public const float PUMP_TIMEOUT = 0.05f;

        /// <summary>
        /// The default camera follow speed.
        /// </summary>
        public const int CAMERA_SPEED = 14;

        /// <summary>
        /// The speed multiplier applied to moving socks.
        /// </summary>
        public const float SOCK_SPEED_K = 0.9f;

        /// <summary>
        /// The vertical offset used when testing sock collisions.
        /// </summary>
        public const int SOCK_COLLISION_Y_OFFSET = 85;

        /// <summary>
        /// The interaction radius used for bubbles.
        /// </summary>
        public const int BUBBLE_RADIUS = 60;

        /// <summary>
        /// The interaction radius used for wheel objects.
        /// </summary>
        public const int WHEEL_RADIUS = 110;

        /// <summary>
        /// The movement radius used for grab interactions.
        /// </summary>
        public const int GRAB_MOVE_RADIUS = 65;

        /// <summary>
        /// The target-relative Y pivot ratio used by the night sleep pulse overlay.
        /// </summary>
        private const float SleepPulsePivotYRatio = 433f / 480f;

        /// <summary>
        /// The interval between looping night sleep sounds.
        /// </summary>
        private const float NightSleepSoundInterval = 4f;

        /// <summary>
        /// The interaction radius for remote-control objects.
        /// </summary>
        public const int RC_CONTROLLER_RADIUS = 90;

        /// <summary>
        /// Initial state for the candy blink sequence.
        /// </summary>
        public const int CANDY_BLINK_INITIAL = 0;

        /// <summary>
        /// State for blinking the candy star highlight.
        /// </summary>
        public const int CANDY_BLINK_STAR = 1;

        /// <summary>
        /// Tutorial animation state for showing the prompt.
        /// </summary>
        public const int TUTORIAL_SHOW_ANIM = 0;

        /// <summary>
        /// Tutorial animation state for hiding the prompt.
        /// </summary>
        public const int TUTORIAL_HIDE_ANIM = 1;

        /// <summary>
        /// Animation index for the normal earth state.
        /// </summary>
        public const int EARTH_NORMAL_ANIM = 0;

        /// <summary>
        /// Animation index for the inverted earth state.
        /// </summary>
        public const int EARTH_UPSIDEDOWN_ANIM = 1;

        /// <summary>
        /// Dispatcher used for delayed selector callbacks.
        /// </summary>
        private DelayedDispatcher dd;

        /// <summary>
        /// Delegate notified when the scene reaches win or loss conditions.
        /// </summary>
        public IGameSceneDelegate gameSceneDelegate;

        /// <summary>
        /// Animation pool for gameplay objects.
        /// </summary>
        private readonly AnimationsPool aniPool;

        /// <summary>
        /// Animation pool for particle effects.
        /// </summary>
        private readonly AnimationsPool particlesAniPool;

        /// <summary>
        /// Layer containing decals and other overlay visuals.
        /// </summary>
        private readonly BaseElement decalsLayer;

        /// <summary>
        /// Animation pool for static HUD and decoration objects.
        /// </summary>
        private readonly AnimationsPool staticAniPool;

        /// <summary>
        /// Pollen particle renderer used by applicable levels.
        /// </summary>
        private PollenDrawer pollenDrawer;

        /// <summary>
        /// Primary background tile map.
        /// </summary>
        private TileMap back;

        /// <summary>
        /// Primary background texture used for computing scale.
        /// </summary>
        private readonly CTRTexture2D backTexture;

        /// <summary>
        /// Cached background scale derived from internal screen width.
        /// </summary>
        private float backgroundScale = 1f;

#pragma warning disable IDE1006
        /// <summary>
        /// The active Om Nom gameplay object.
        /// </summary>
        private GameObject targetObject => targets.Count > 0 ? targets[0].targetObject : null;

        /// <summary>
        /// Controller for Om Nom animation state transitions.
        /// </summary>
        private TargetAnimationController targetAnimationController => targets.Count > 0 ? targets[0].controller : null;
#pragma warning restore IDE1006

        /// <summary>
        /// Support visual attached to certain level setups.
        /// </summary>
        private Image support;

#pragma warning disable IDE1006
        /// <summary>
        /// The main candy gameplay object.
        /// </summary>
        private GameObject candy => candies[0].WholeBody.Visual;

        /// <summary>
        /// The base candy sprite for split or layered visuals.
        /// </summary>
        private GameObject candyMain => candies[0].WholeBody.Main;

        /// <summary>
        /// The top candy sprite for split or layered visuals.
        /// </summary>
        private GameObject candyTop => candies[0].WholeBody.Top;

        /// <summary>
        /// Animation used for the candy blink effect.
        /// </summary>
        private Animation candyBlink => candies[0].WholeBody.BlinkAnimation;

        /// <summary>
        /// The constrained point currently representing the candy anchor.
        /// </summary>
        private ConstraintedPoint star => candies[0].WholeBody.Point;
#pragma warning restore IDE1006

        /// <summary>All independent candies in the level. Single-candy packs hold one element.</summary>
        private readonly List<CandyContext> candies = [];

#pragma warning disable IDE0052
        /// <summary>All Om Noms in the level. Single-target packs hold one element.</summary>
        private readonly List<TargetContext> targets = [];
#pragma warning restore IDE0052

        /// <summary>True once the first &lt;candy&gt; element has claimed the pre-built primary candy (candies[0]).</summary>
        private bool primaryCandyClaimed;

        /// <summary>
        /// All active grab/bungee objects in the loaded level.
        /// </summary>
        private List<Grab> bungees;

        /// <summary>Index over every rope in the level, hook ropes and the candy connector alike.</summary>
        private RopeRegistry ropes;

        /// <summary>The elastic rope joining the two candies in a candiesConnected level, or null.</summary>
        private Bungee candyConnector;

        /// <summary>Whether the level joins its two candies with the connecting elastic.</summary>
        private bool candiesConnected;

        /// <summary>
        /// Whether the connecting elastic is finger-cuttable. Defaults to <see langword="true"/>
        /// (a normal rope); only an explicit <c>candiesConnectedBreakable="false"</c> makes it a
        /// chain, mirroring grab <c>breakable</c>. (The original defaulted the connector to a chain;
        /// this default is chosen for consistency with <c>breakable</c>.)
        /// </summary>
        private bool candiesConnectedBreakable;

        /// <summary>Rest/limit length of the connecting elastic, already scaled.</summary>
        private float candiesConnectedLength;

        /// <summary>Fixed delay between the first and second Om Nom in a two-Om-Nom chat greeting (Time Travel hand-off).</summary>
        private const float ChatGreetingGapSeconds = 1.0f;

        /// <summary>
        /// One-in-N odds that an idle reaction on a two-Om-Nom level becomes a chat greeting.
        /// </summary>
        private const int ChatReactionOdds = 3;

        /// <summary>
        /// All active razor objects in the loaded level.
        /// </summary>
        private List<Razor> razors;

        /// <summary>
        /// All active spike objects in the loaded level.
        /// </summary>
        private List<Spikes> spikes;

        /// <summary>
        /// All collectible stars in the loaded level.
        /// </summary>
        private List<Star> stars;

        /// <summary>
        /// All active bubble objects in the loaded level.
        /// </summary>
        private List<Bubble> bubbles;

        /// <summary>
        /// All active pump objects in the loaded level.
        /// </summary>
        private List<Pump> pumps;

        /// <summary>
        /// All active steam tube objects in the loaded level.
        /// </summary>
        private List<SteamTube> tubes;

        /// <summary>
        /// All active bamboo tube objects in the loaded level.
        /// </summary>
        private List<BambooTube> bambooTubes;

        /// <summary>
        /// All active sock objects in the loaded level.
        /// </summary>
        private List<Sock> socks;

        /// <summary>
        /// All active bouncer objects in the loaded level.
        /// </summary>
        private List<Bouncer> bouncers;

        /// <summary>
        /// All active rotated circle objects in the loaded level.
        /// </summary>
        private List<RotatedCircle> rotatedCircles;

        /// <summary>
        /// All active rocket objects in the loaded level.
        /// </summary>
        private List<Rocket> rockets;

        /// <summary>Buttons that stop and restart time.</summary>
        private List<PauseSwitcher> pauseSwitchers;

        /// <summary>Screen-covering effect shown while time is stopped.</summary>
        private PauseSwitcherWaves pauseSwitcherWaves;

        /// <summary>
        /// Whether time is currently stopped. Objects that stop with it are told at their update
        /// call instead of keeping their own copy. The iOS mover gate also checks for clock
        /// elements, which this port does not have.
        /// </summary>
        private bool timeFrozen;

        /// <summary>The time-freeze button a pointer is holding, or <see langword="null"/>.</summary>
        private PauseSwitcherTouch? pauseSwitcherTouch;

        /// <summary>
        /// All active mechanical hand objects in the loaded level.
        /// </summary>
        private List<MechanicalHand> hands;

        /// <summary>
        /// All active snail objects in the loaded level.
        /// </summary>
        private List<Snail> snailobjects;

        /// <summary>
        /// All tutorial image objects attached to the scene.
        /// </summary>
        private List<CTRGameObject> tutorialImages;

        /// <summary>
        /// All tutorial text labels attached to the scene.
        /// </summary>
        private List<Text> tutorials;

        /// <summary>
        /// All active ghost objects in the loaded level.
        /// </summary>
        private List<Ghost> ghosts;

        /// <summary>
        /// All active mouse objects in the loaded level.
        /// </summary>
        private List<Mouse> mice;

        /// <summary>
        /// Manager for composite mouse interactions.
        /// </summary>
        private MiceObject miceManager;

        /// <summary>
        /// Manager for conveyor belt interactions.
        /// </summary>
        private ConveyorBeltObject conveyors;

        /// <summary>
        /// All ant path segments in the current level.
        /// </summary>
        private List<AntsPathSegment> antsPathsSegments;

        /// <summary>
        /// All ant paths in the current level.
        /// </summary>
        private List<AntsPath> antsPaths;

        /// <summary>Atlas quad holding the left split-candy sprite.</summary>
        private const int SplitCandyLeftQuad = 8;

        /// <summary>Atlas quad holding the right split-candy sprite.</summary>
        private const int SplitCandyRightQuad = 9;

        /// <summary>
        /// The left half body built while <c>&lt;candyL&gt;</c> parses, held until the halves are
        /// handed to the primary candy's lifecycle. Null outside loading.
        /// </summary>
        private CandyBody pendingLeftHalf;

        /// <summary>
        /// The right half body built while <c>&lt;candyR&gt;</c> parses, held until the halves are
        /// handed to the primary candy's lifecycle. Null outside loading.
        /// </summary>
        private CandyBody pendingRightHalf;

        /// <summary>
        /// The default horizontal scale applied to the Om Nom target.
        /// </summary>
        private float targetBaseScaleX = 1f;

        /// <summary>
        /// The default vertical scale applied to the Om Nom target.
        /// </summary>
        private float targetBaseScaleY = 1f;

        /// <summary>Number of simultaneous pointer gestures supported by gameplay.</summary>
        private const int PointerGestureCount = 5;

        /// <summary>Authoritative per-pointer rope-cut gesture state.</summary>
        private readonly PointerGestureState[] pointerGestures = new PointerGestureState[PointerGestureCount];

        /// <summary>Whether an outcome allows visual-only pointer trails while gameplay input is blocked.</summary>
        internal bool AcceptsVisualOnlyPointerInput => gameplayFlow.HasOutcome;

        /// <summary>
        /// Current rope physics time scale.
        /// </summary>
        private float ropePhysicsSpeed;

        /// <summary>
        /// Current water surface height.
        /// </summary>
        private float waterLevel;

        /// <summary>
        /// Current water movement speed.
        /// </summary>
        private float waterSpeed;

        /// <summary>
        /// Water simulation layer for underwater levels.
        /// </summary>
        private WaterElement waterLayer;

        /// <summary>
        /// The HUD star animations that show collected stars.
        /// </summary>
        private readonly Animation[] hudStar = new Animation[3];

        /// <summary>
        /// The gameplay camera.
        /// </summary>
        private Camera2D camera;

        /// <summary>
        /// The width of the loaded map in world units.
        /// </summary>
        private float mapWidth;

        /// <summary>
        /// The height of the loaded map in world units.
        /// </summary>
        private float mapHeight;

        /// <summary>
        /// The X origin of the loaded map.
        /// </summary>
        private float mapOriginX;

        /// <summary>
        /// The Y origin of the loaded map.
        /// </summary>
        private float mapOriginY;

        /// <summary>
        /// The level's extent in world units. The camera fits this region into the viewport.
        /// </summary>
        private CTRRectangle cameraBounds;

        /// <summary>
        /// The region the camera can show at once, in world units. Equal to the level extent on
        /// an axis the level does not exceed, and to the design size on an axis it does, which is
        /// the axis the camera scrolls along.
        /// </summary>
        private CTRRectangle cameraWindow;

        // private bool spiderTookCandy;

        /// <summary>
        /// Special-case level behavior flag.
        /// </summary>
        private int special;

        /// <summary>
        /// Whether the camera should remain locked to its current target.
        /// </summary>
        private bool fastenCamera;

        /// <summary>
        /// Atomic ownership ticket for the ghost bubble parked behind a merged candy's own bubble.
        /// It is released when that bubble pops or the ticket is replaced, and is
        /// <see langword="null"/> whenever no second ghost is waiting.
        /// </summary>
        private ParkedGhostBubble parkedGhostBubble;

        /// <summary>Atomic ticket waiting for the lantern's delayed capture callback.</summary>
        private PendingLanternCapture pendingLanternCapture;

        /// <summary>
        /// The number of ropes cut within the active combo window.
        /// </summary>
        private int ropesCutAtOnce;

        /// <summary>
        /// Remaining time in the active rope-cut combo window.
        /// </summary>
        private float ropeAtOnceTimer;

        /// <summary>
        /// Whether click-to-cut input mode is enabled.
        /// </summary>
        private readonly bool clickToCut;

        /// <summary>
        /// The number of stars collected in the level.
        /// </summary>
        public int starsCollected;

        /// <summary>
        /// The elapsed level time.
        /// </summary>
        public float time;

        /// <summary>The completed result awaiting the delayed win callback.</summary>
        private LevelResult? pendingLevelResult;

        /// <summary>
        /// The initial camera distance to the candy anchor.
        /// </summary>
        public float initialCameraToStarDistance;

        /// <summary>Single owner of the restart-dim machine and the win/lose flags.</summary>
        public readonly LevelFlowState gameplayFlow = new();

        /// <summary>
        /// Whether restart should animate through the dim overlay.
        /// </summary>
        public bool animateRestartDim;

        /// <summary>
        /// Whether camera motion is temporarily frozen.
        /// </summary>
        public bool freezeCamera;

        /// <summary>
        /// The current camera follow mode.
        /// </summary>
        public int cameraMoveMode;

        /// <summary>
        /// Whether touch input is temporarily ignored.
        /// </summary>
        public bool ignoreTouches;

        /// <summary>
        /// Whether the loaded level uses night-specific behavior.
        /// </summary>
        public bool nightLevel;

        /// <summary>Single owner of authored gravity, orientation, input, physics, and presentation.</summary>
        public readonly GravityState gravityState = new();

        /// <summary>
        /// Whether the loaded map declares a split candy. Level metadata, parsed before the split
        /// lifecycle exists, so the loader cannot ask the primary candy's lifecycle instead.
        /// </summary>
        private bool levelAuthorsSplitCandy;

        /// <summary>
        /// Optional string to load as a name for the level.
        /// </summary>
        public string levelName;

        /// <summary>
        /// Count of tummy teaser interactions completed in the scene.
        /// </summary>
        public int tummyTeasers;

        /// <summary>
        /// The last recorded touch position for scene-level gestures.
        /// </summary>
        public Vector slastTouch;

        /// <summary>
        /// Represents one rendered cut segment between two touch positions.
        /// </summary>
        public sealed class FingerCut : FrameworkTypes
        {
            /// <summary>
            /// The world-space start position of the cut segment.
            /// </summary>
            public Vector start;

            /// <summary>
            /// The world-space end position of the cut segment.
            /// </summary>
            public Vector end;

            /// <summary>
            /// The rendered width at the start of the cut segment.
            /// </summary>
            public float startSize;

            /// <summary>
            /// The rendered width at the end of the cut segment.
            /// </summary>
            public float endSize;

            /// <summary>
            /// The color used to render the cut segment.
            /// </summary>
            public RGBAColor c;
        }

        // private sealed class SCandy : ConstraintedPoint
        // {
        // public bool good;

        // public float speed;

        // public float angle;

        // public float lastAngleChange;
        // }

        /// <summary>
        /// Specialized tutorial text element that stores a behavior flag.
        /// </summary>
        private sealed class TutorialText : Text
        {
            /// <summary>
            /// Special-case behavior identifier for the tutorial text.
            /// </summary>
            public int special;
        }

        /// <summary>
        /// Specialized gameplay object that carries a behavior flag.
        /// </summary>
        private sealed class GameObjectSpecial : CTRGameObject
        {
            /// <summary>
            /// Creates a special game object from a texture.
            /// </summary>
            /// <param name="t">The texture assigned to the new object.</param>
            /// <returns>A new special game object initialized with the provided texture.</returns>
            private static GameObjectSpecial GameObjectSpecial_create(CTRTexture2D t)
            {
                GameObjectSpecial gameObjectSpecial = new();
                _ = gameObjectSpecial.InitWithTexture(t);
                return gameObjectSpecial;
            }

            /// <summary>
            /// Creates a special game object from a resource texture and draw quad.
            /// </summary>
            /// <param name="resourceName">The texture resource identifier.</param>
            /// <param name="q">The draw quad index to use.</param>
            /// <returns>A new special game object configured for the requested texture quad.</returns>
            public static GameObjectSpecial GameObjectSpecial_createWithResIDQuad(string resourceName, int q)
            {
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial_create(Application.GetTexture(resourceName));
                gameObjectSpecial.SetDrawQuad(q);
                return gameObjectSpecial;
            }

            /// <summary>
            /// Special-case behavior identifier for this object.
            /// </summary>
            public int special;
        }
    }
}
