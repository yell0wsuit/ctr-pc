using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System.Collections.Generic;

namespace CutTheRope.game
{
    internal sealed class MapPickerController : ViewController, IButtonDelegation
    {
        public override NSObject InitWithParent(ViewController p)
        {
            if (base.InitWithParent(p) != null)
            {
                selectedMap = null;
                maplist = null;
                CreatePickerView();
                View view = (View)new View().InitFullscreen();
                RectangleElement rectangleElement = (RectangleElement)new RectangleElement().Init();
                rectangleElement.color = RGBAColor.whiteRGBA;
                rectangleElement.width = (int)SCREEN_WIDTH;
                rectangleElement.height = (int)SCREEN_HEIGHT;
                _ = view.AddChild(rectangleElement);
                FontGeneric font = Application.GetFont(4);
                Text text = new Text().InitWithFont(font);
                text.SetString(NSS("Loading..."));
                text.anchor = text.parentAnchor = 18;
                _ = view.AddChild(text);
                AddViewwithID(view, 1);
                SetNormalMode();
            }
            return this;
        }

        public void CreatePickerView()
        {
            View view = (View)new View().InitFullscreen();
            RectangleElement rectangleElement = (RectangleElement)new RectangleElement().Init();
            rectangleElement.color = RGBAColor.whiteRGBA;
            rectangleElement.width = (int)SCREEN_WIDTH;
            rectangleElement.height = (int)SCREEN_HEIGHT;
            _ = view.AddChild(rectangleElement);
            FontGeneric font = Application.GetFont(4);
            Text text = new Text().InitWithFont(font);
            text.SetString(NSS("START"));
            Text text2 = new Text().InitWithFont(font);
            text2.SetString(NSS("START"));
            text2.scaleX = text2.scaleY = 1.2f;
            Button button = new Button().InitWithUpElementDownElementandID(text, text2, 0);
            button.anchor = button.parentAnchor = 34;
            button.delegateButtonDelegate = this;
            _ = view.AddChild(button);
            AddViewwithID(view, 0);
        }

        public override void Activate()
        {
            base.Activate();
            if (autoLoad)
            {
                string text = "maps/";
                NSString nsstring = selectedMap;
                NSString nSString = NSS(text + (nsstring?.ToString()));
                XMLNode xMLNode = XMLNode.ParseXML(nSString.ToString());
                XmlLoaderFinishedWithfromwithSuccess(xMLNode, nSString, xMLNode != null);
                return;
            }
            ShowView(0);
            LoadList();
        }

        public static void LoadList()
        {
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public void XmlLoaderFinishedWithfromwithSuccess(XMLNode rootNode, NSString url, bool success)
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

        public void SetNormalMode()
        {
            autoLoad = false;
            ((CTRRootController)Application.SharedRootController()).SetPicker(true);
        }

        public void SetAutoLoadMap(NSString map)
        {
            autoLoad = true;
            ((CTRRootController)Application.SharedRootController()).SetPicker(false);
            NSREL(selectedMap);
            selectedMap = (NSString)NSRET(map);
        }

        public void OnButtonPressed(int n)
        {
            if (n == 0)
            {
                LoadList();
            }
        }

        public override void Dealloc()
        {
            NSREL(selectedMap);
            base.Dealloc();
        }

        private NSString selectedMap;

        private Dictionary<string, XMLNode> maplist;

        private bool autoLoad;
    }
}
