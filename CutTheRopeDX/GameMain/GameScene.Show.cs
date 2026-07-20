using System.Globalization;
using System.Xml.Linq;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
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

            // Load two-parts candy bubble animations
            LoadCandyBubbleAnimations();
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
            Game1.RPC?.SetLevelPresence(cTRRootController.GetPack(), cTRRootController.GetLevel(), starsCollected, false, levelName);
            candyBubble = null;
            candyBubbleL = null;
            candyBubbleR = null;
            noCandy = twoParts != 2;
            noCandyL = false;
            noCandyR = false;
            for (int ti = 0; ti < targets.Count; ti++)
            {
                targets[ti].controller?.ResetBlink();
            }
            // spiderTookCandy = false;
            time = 0f;
            score = 0;
            gravityNormal = true;
            MaterialPoint.globalGravity = Vect(globalGravityX, globalGravityY);
            MaterialPoint.globalDisableGravity = VectEqual(MaterialPoint.globalGravity, vectZero);
            dimTime = 0f;
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
                text.x = 15f + Canvas.xOffsetScaled;
                bool isChinese = LanguageHelper.IsCurrentAny(Language.LANGZH, Language.LANGZHTW);
                text.y = isChinese ? SCREEN_HEIGHT : SCREEN_HEIGHT + 15f; // the box and level number or level name in game
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
            }
            for (int m = 0; m < 5; m++)
            {
                dragging[m] = false;
                startPos[m] = prevStartPos[m] = vectZero;
                fingerTraces[m]?.Reset();
            }
            if (clickToCut)
            {
                ResetBungeeHighlight();
            }
            Global.MouseCursor.ReleaseButtons();
            CTRRootController.LogEvent("IG_SHOWN");
        }

        /// <summary>
        /// Resolves the level's display name from its <c>levelName</c> attribute.
        /// </summary>
        /// <remarks>
        /// Shipped packs use <c>levelName</c> as a localization key; hand-authored levels have no
        /// entry in the string tables and fall through <see cref="Application.GetString"/> verbatim.
        /// </remarks>
        /// <returns>The name to display, or <see langword="null"/> when the level has none.</returns>
        private string ResolveLevelDisplayName()
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
                ConstraintedPoint constraintedPoint = twoParts != 2 ? starL : star;
                float cameraStartX;
                float cameraStartY;
                if (mapWidth > SCREEN_WIDTH)
                {
                    if (constraintedPoint.pos.X > mapWidth / 2)
                    {
                        cameraStartX = 0f;
                        cameraStartY = 0f;
                    }
                    else
                    {
                        cameraStartX = mapWidth - SCREEN_WIDTH;
                        cameraStartY = 0f;
                    }
                }
                else if (constraintedPoint.pos.Y > mapHeight / 2)
                {
                    cameraStartX = 0f;
                    cameraStartY = 0f;
                }
                else
                {
                    cameraStartX = 0f;
                    cameraStartY = mapHeight - SCREEN_HEIGHT;
                }
                float targetCameraX = constraintedPoint.pos.X - (SCREEN_WIDTH / 2f);
                float targetCameraY = constraintedPoint.pos.Y - (SCREEN_HEIGHT / 2f);
                float boundedCameraX = FIT_TO_BOUNDARIES(targetCameraX, 0f, mapWidth - SCREEN_WIDTH);
                float boundedCameraY = FIT_TO_BOUNDARIES(targetCameraY, 0f, mapHeight - SCREEN_HEIGHT);
                camera.MoveToXYImmediate(cameraStartX, cameraStartY, true);
                initialCameraToStarDistance = VectDistance(camera.pos, Vect(boundedCameraX, boundedCameraY));
                return;
            }
            ignoreTouches = false;
            camera.MoveToXYImmediate(0f, 0f, true);
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
            if (nightLevel && owner.isNightTargetAwake == false)
            {
                return;
            }
            if (i == 1)
            {
                owner.blinkTimer--;
                if (owner.blinkTimer == 0)
                {
                    owner.controller?.TriggerBlink();
                    owner.blinkTimer = 3;
                }
                owner.idlesTimer--;
                if (owner.idlesTimer == 0)
                {
                    // On two-Om-Nom levels the idle reaction may instead become a mutual chat
                    // greeting (Time Travel). When it does, both timers are reset by the chat.
                    if (!TryStartChatReaction())
                    {
                        owner.controller?.PlayRandomIdleVariant(RND_RANGE);
                        owner.idlesTimer = RND_RANGE(5, 20);
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
