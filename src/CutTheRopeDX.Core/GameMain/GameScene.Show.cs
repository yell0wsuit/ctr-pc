using System.Globalization;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <inheritdoc />
        public override void Show()
        {
            // Initialize game state and load level data
            InitializeGameState();
            InitializeCandyObjects();
            InitializeHUDStars();

            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            XElement map = cTRRootController.GetMap();

            float mapScale = 3f;
            float mapOffsetY = 0f;

            // Load level metadata (map dimensions, game design settings, candy positions)
            LoadAllLevelMetadata(map, mapScale, mapOffsetY, out float mapOffsetX, out int mapGridOffsetX, out int mapGridOffsetY);
            mapOriginX = mapOffsetX + mapGridOffsetX;
            mapOriginY = mapOffsetY + mapGridOffsetY;

            // Load all game objects from XML
            LoadObjectsFromMap(map, mapScale, mapOffsetX, mapOffsetY, mapGridOffsetX, mapGridOffsetY);

            // Claim candies that start inside a claw before a taut authored rope gets its first
            // constraint pass. The normal hand update still owns all later catches and releases.
            UpdateHands(0f);

            conveyors.SortBelts();

            // Bind objects to transporters once at scene setup (matches iOS [GameScene show])
            conveyors.ProcessItems(bubbles);
            conveyors.ProcessItems(stars);
            conveyors.ProcessItems(bouncers);
            conveyors.ProcessItems(socks);
            conveyors.ProcessItems(tubes);
            conveyors.ProcessItems(pumps);
            conveyors.ProcessItems(bungees);
            conveyors.ProcessItems(LightEmitterVisuals());

            foreach (object obj in rotatedCircles)
            {
                RotatedCircle rotatedCircle2 = (RotatedCircle)obj;
                rotatedCircle2.operating = -1;
                rotatedCircle2.circlesArray = rotatedCircles;
            }
            StartCamera();
            tummyTeasers = 0;
            starsCollected = 0;
            // Update RPC with current level info (on start/restart)
            PlatformServices.RichPresence?.SetLevelPresence(cTRRootController.GetPack(), cTRRootController.GetLevel(), starsCollected, false, levelName);
            foreach (CandyBody body in ActiveCandyBodies())
            {
                body.Bubble = null;
            }
            for (int ti = 0; ti < targets.Count; ti++)
            {
                targets[ti].controller?.ResetBlink();
            }
            // spiderTookCandy = false;
            time = 0f;
            gravityState.Activate();
            ropesCutAtOnce = 0;
            ropeAtOnceTimer = 0f;
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_doCandyBlink), null, 1);
            string packAndLevelNumbers = (cTRRootController.GetPack() + 1).ToString(CultureInfo.InvariantCulture) + " - " + (cTRRootController.GetLevel() + 1).ToString(CultureInfo.InvariantCulture);
            LevelLabelText levelLabel = LevelLabel.Resolve(
                CustomLevelSession.IsActive,
                ResolveLevelDisplayName(),
                Application.GetString("LEVEL"),
                packAndLevelNumbers);
            if (levelLabel.Primary != null)
            {
                Text text = Text.CreateWithFontandString(Resources.Fnt.BigFont, levelLabel.Primary);
                text.anchor = 33;
                text.SetName("levelLabel");
                bool isChinese = LanguageHelper.IsCurrentAny(Language.LANGZH, Language.LANGZHTW);
                if (levelLabel.Secondary != null)
                {
                    Text text2 = Text.CreateWithFontandString(Resources.Fnt.BigFont, levelLabel.Secondary);
                    text2.anchor = 33;
                    text2.parentAnchor = 9;
                    text2.y = isChinese ? 3f : 30f; // the "Level" label in game
                    text2.rotationCenterX -= text2.width / 2f;
                    text2.scaleX = text2.scaleY = 0.7f;
                    _ = text.AddChild(text2);
                }
                Timeline timeline6 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
                timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                text.AddTimelinewithID(timeline6, 0);
                text.PlayTimeline(0);
                timeline6.delegateTimelineDelegate = staticAniPool;
                _ = staticAniPool.AddChild(text);

                // The label is rebuilt from scratch here on every level start and restart, so it
                // needs the current HUD scale applied immediately rather than waiting on the next
                // viewport-driven relayout, which may not come before the player sees it.
                PlaceLevelLabel();
            }
            foreach (PointerGestureState gesture in pointerGestures)
            {
                gesture?.Reset();
            }
            if (clickToCut)
            {
                ResetBungeeHighlight();
            }
            PlatformServices.Cursor?.ReleaseButtons();
            CTRRootController.LogEvent("IG_SHOWN");
        }

        /// <summary>
        /// Vertical offset, in design units, subtracted from <see cref="FrameworkTypes.VisibleBounds"/>'s
        /// height to place the non-CJK level-number label, lifting it clear of the bottom edge.
        /// </summary>
        /// <remarks>
        /// The label used to be placed at -15, hanging below the bounds rather than sitting
        /// inside them, which read as clipped on a viewport that draws the HUD large. These are
        /// the H5 edition's numbers instead: its base profile is the same 2560x1440 design space,
        /// so its authored offsets are read in the same units and transplant as written.
        /// </remarks>
        private const float LevelLabelInsetY = 5f;

        /// <summary>
        /// <see cref="LevelLabelInsetY"/> for CJK, whose taller glyph box sits lower in the line.
        /// </summary>
        /// <remarks>
        /// Carries across the 15-unit lift this branch has always had over the Latin one. The H5
        /// edition places every language alike, so there is nothing to copy here - only the
        /// existing correction to preserve, now that the edge it is measured from has moved.
        /// </remarks>
        private const float LevelLabelInsetYCJK = 20f;

        /// <summary>
        /// Authored offset, in design units, of the level-number label from the left edge.
        /// </summary>
        private const float LevelLabelInsetX = 40f;

        /// <summary>
        /// Resolves the level's display name from its <c>levelName</c> attribute.
        /// </summary>
        /// <remarks>
        /// Shipped packs use <c>levelName</c> as a localization key; hand-authored levels have no
        /// entry in the string tables and fall through <see cref="Application.GetString"/> verbatim.
        /// </remarks>
        /// <returns>The name to display, or <see langword="null"/> when the level has none.</returns>
        internal string ResolveLevelDisplayName()
        {
            return string.IsNullOrWhiteSpace(levelName) ? null : Application.GetString(levelName);
        }

        /// <summary>
        /// Positions the camera for the newly loaded map and sets the initial camera movement mode.
        /// </summary>
        public void StartCamera()
        {
            if (mapWidth > SCREEN_WIDTH || mapHeight > SCREEN_HEIGHT)
            {
                ignoreTouches = true;
                fastenCamera = false;
                camera.type = CAMERATYPE.CAMERASPEEDPIXELS;
                camera.speed = 20f;
                cameraMoveMode = 0;
                ConstraintedPoint constraintedPoint = CameraFocusPoint();

                // The pan starts at whichever end of the tracking range is away from the focus
                // point. Both ends and the midpoint they are chosen by are the level's own: a
                // level wider than the design box is centered on it, so its near end is a negative
                // world X and neither end is at the origin.
                CTRRectangle range = CameraTrackingRange();
                float cameraStartX;
                float cameraStartY;
                if (mapWidth > SCREEN_WIDTH)
                {
                    cameraStartX = constraintedPoint.pos.X > range.x + (mapWidth / 2f)
                        ? range.x
                        : range.x + range.w;
                    cameraStartY = range.y;
                }
                else if (constraintedPoint.pos.Y > range.y + (mapHeight / 2f))
                {
                    cameraStartX = range.x;
                    cameraStartY = range.y;
                }
                else
                {
                    cameraStartX = range.x;
                    cameraStartY = range.y + range.h;
                }
                Vector boundedCamera = BoundedCameraPosition(
                    constraintedPoint.pos.X - (SCREEN_WIDTH / 2f),
                    constraintedPoint.pos.Y - (SCREEN_HEIGHT / 2f));

                // Seat the tracked position at the authored start point and let the fit derive the
                // rest from it, the way every later frame does.
                camera.MoveToXYImmediate(cameraStartX, cameraStartY, true);
                ApplyCameraFit(ScreenPresentation.Instance.Snapshot);
                initialCameraToStarDistance = VectDistance(camera.pos, boundedCamera);
                return;
            }
            ignoreTouches = false;
            Vector resting = BoundedCameraPosition(0f, 0f);
            camera.MoveToXYImmediate(resting.X, resting.Y, true);
            ApplyCameraFit(ScreenPresentation.Instance.Snapshot);
        }

        /// <summary>
        /// Plays the candy blink animation.
        /// </summary>
        public void DoCandyBlink()
        {
            candyBlink.PlayTimeline(0);
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
            if (t.element is RotatedCircle rotatedCircle && rotatedCircles.IndexOf(rotatedCircle) != -1)
            {
                return;
            }
            TargetContext owner = null;
            for (int ti = 0; ti < targets.Count; ti++)
            {
                if (targets[ti].targetObject == t.element)
                {
                    owner = targets[ti];
                    break;
                }
            }
            if (owner == null)
            {
                return;
            }
            if (nightLevel && !owner.NightSleep.IsAwake)
            {
                return;
            }
            if (i == 1)
            {
                TargetIdleStep idleStep = owner.Idle.AdvanceCadence();
                if (idleStep.BlinkDue && owner.Idle.ConsumeBlink(3))
                {
                    owner.controller?.TriggerBlink();
                }
                if (idleStep.IdleDue)
                {
                    // On two-Om-Nom levels the idle reaction may instead become a mutual chat
                    // greeting (Time Travel). When it does, both timers are reset by the chat.
                    if (!TryStartChatReaction())
                    {
                        owner.controller?.PlayRandomIdleVariant(RND_RANGE);
                        _ = owner.Idle.ConsumeIdle(RND_RANGE(5, 20));
                    }
                }
                return;
            }
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            if (t.element == candy)
            {
                RestoreCandyProperties();
            }
            else if (t.element is RotatedCircle rotatedCircle && rotatedCircles.IndexOf(rotatedCircle) != -1)
            {
                ((RotatedCircle)t.element).removeOnNextUpdate = true;
            }
        }
    }
}
