using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Controller that either presents the map picker view or auto-loads a selected map.
    /// </summary>
    internal sealed class MapPickerController : ViewController, IButtonDelegation
    {
        /// <summary>
        /// Initializes a map picker controller and its picker/loading views.
        /// </summary>
        /// <param name="parent">Parent view controller.</param>
        public MapPickerController(ViewController parent)
            : base(parent)
        {
            selectedMap = null;
            maplist = null;
            CreatePickerView();
            View view = new();
            RectangleElement rectangleElement = new()
            {
                color = RGBAColor.whiteRGBA,
                width = (int)SCREEN_WIDTH,
                height = (int)SCREEN_HEIGHT
            };
            _ = view.AddChild(rectangleElement);
            FontGeneric font = Application.GetFont(Resources.Fnt.SmallFont);
            Text text = new Text().InitWithFont(font);
            text.SetString("Loading...");
            text.anchor = text.parentAnchor = 18;
            _ = view.AddChild(text);
            AddViewwithID(view, 1);
            SetNormalMode();
        }

        /// <summary>
        /// Creates the basic map picker start view.
        /// </summary>
        public void CreatePickerView()
        {
            View view = new();
            RectangleElement rectangleElement = new()
            {
                color = RGBAColor.whiteRGBA,
                width = (int)SCREEN_WIDTH,
                height = (int)SCREEN_HEIGHT
            };
            _ = view.AddChild(rectangleElement);
            FontGeneric font = Application.GetFont(Resources.Fnt.SmallFont);
            Text text = new Text().InitWithFont(font);
            text.SetString("START");
            Text text2 = new Text().InitWithFont(font);
            text2.SetString("START");
            text2.scaleX = text2.scaleY = 1.2f;
            Button button = new Button().InitWithUpElementDownElementandID(text, text2, MapPickerControllerButtonId.Start);
            button.anchor = button.parentAnchor = 34;
            button.delegateButtonDelegate = this;
            _ = view.AddChild(button);
            AddViewwithID(view, 0);
        }

        /// <inheritdoc />
        public override void Activate()
        {
            base.Activate();
            if (autoLoad)
            {
                string mapPath = Path.Combine(ContentPaths.MapsDirectory, selectedMap);
                XElement mapElement = ContentPaths.LoadXml(mapPath);
                XmlLoaderFinishedWithfromwithSuccess(mapElement, mapPath, mapElement != null);
                return;
            }
            ShowView(0);
            LoadList();
        }

        /// <summary>
        /// Starts loading the available map list.
        /// </summary>
        public static void LoadList()
        {
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            base.Deactivate();
        }

        /// <summary>
        /// Handles completion of map XML loading.
        /// </summary>
        /// <param name="rootNode">Loaded map XML root node.</param>
        /// <param name="_">Path or source identifier reported by the XML loader.</param>
        /// <param name="_1">Whether the XML loader reported success.</param>
        public void XmlLoaderFinishedWithfromwithSuccess(XElement rootNode, string _, bool _1)
        {
            if (rootNode != null)
            {
                CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
                ctrrootController.SetMap(rootNode);
                ctrrootController.SetMapName(selectedMap);
                CTRRootController.SetMapsList(maplist);
                Deactivate();
            }
        }

        /// <summary>
        /// Enables normal picker mode instead of auto-loading a map.
        /// </summary>
        public void SetNormalMode()
        {
            autoLoad = false;
            ((CTRRootController)Application.SharedRootController()).SetPicker(true);
        }

        /// <summary>
        /// Enables auto-load mode for a specific map.
        /// </summary>
        /// <param name="map">Map file name to load when the controller activates.</param>
        public void SetAutoLoadMap(string map)
        {
            autoLoad = true;
            ((CTRRootController)Application.SharedRootController()).SetPicker(false);
            selectedMap = map;
        }

        /// <summary>
        /// Handles a typed map picker button press.
        /// </summary>
        /// <param name="n">Map picker button identifier that was pressed.</param>
        public static void OnButtonPressed(MapPickerControllerButtonId n)
        {
            if (n == MapPickerControllerButtonId.Start)
            {
                LoadList();
            }
        }

        /// <inheritdoc />
        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(MapPickerControllerButtonId.FromButtonId(buttonId));
        }

        /// <summary>Selected map file name used in auto-load mode.</summary>
        private string selectedMap;

        /// <summary>Loaded map XML documents keyed by map name.</summary>
        private readonly Dictionary<string, XElement> maplist;

        /// <summary>Whether the controller should load <see cref="selectedMap"/> immediately on activation.</summary>
        private bool autoLoad;
    }
}
