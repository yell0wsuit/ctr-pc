using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System.Collections.Generic;

namespace CutTheRope.game
{
    internal class MapPickerController : ViewController, ButtonDelegate
    {
        public override NSObject initWithParent(ViewController p)
        {
            if (base.initWithParent(p) != null)
            {
                selectedMap = null;
                maplist = null;
                createPickerView();
                View view = (View)new View().initFullscreen();
                RectangleElement rectangleElement = (RectangleElement)new RectangleElement().init();
                rectangleElement.color = RGBAColor.whiteRGBA;
                rectangleElement.width = (int)SCREEN_WIDTH;
                rectangleElement.height = (int)SCREEN_HEIGHT;
                view.addChild(rectangleElement);
                FontGeneric font = Application.getFont(4);
                Text text = new Text().initWithFont(font);
                text.setString(NSS("Loading..."));
                text.anchor = text.parentAnchor = 18;
                view.addChild(text);
                addViewwithID(view, 1);
                setNormalMode();
            }
            return this;
        }

        public virtual void createPickerView()
        {
            View view = (View)new View().initFullscreen();
            RectangleElement rectangleElement = (RectangleElement)new RectangleElement().init();
            rectangleElement.color = RGBAColor.whiteRGBA;
            rectangleElement.width = (int)SCREEN_WIDTH;
            rectangleElement.height = (int)SCREEN_HEIGHT;
            view.addChild(rectangleElement);
            FontGeneric font = Application.getFont(4);
            Text text = new Text().initWithFont(font);
            text.setString(NSS("START"));
            Text text2 = new Text().initWithFont(font);
            text2.setString(NSS("START"));
            text2.scaleX = text2.scaleY = 1.2f;
            Button button = new Button().initWithUpElementDownElementandID(text, text2, 0);
            button.anchor = button.parentAnchor = 34;
            button.delegateButtonDelegate = this;
            view.addChild(button);
            addViewwithID(view, 0);
        }

        public override void activate()
        {
            base.activate();
            if (autoLoad)
            {
                string text = "maps/";
                NSString nsstring = selectedMap;
                NSString nSString = NSS(text + (nsstring?.ToString()));
                XMLNode xMLNode = XMLNode.parseXML(nSString.ToString());
                xmlLoaderFinishedWithfromwithSuccess(xMLNode, nSString, xMLNode != null);
                return;
            }
            showView(0);
            loadList();
        }

        public virtual void loadList()
        {
        }

        public override void deactivate()
        {
            base.deactivate();
        }

        public virtual void xmlLoaderFinishedWithfromwithSuccess(XMLNode rootNode, NSString url, bool success)
        {
            if (rootNode != null)
            {
                CTRRootController ctrrootController = (CTRRootController)Application.sharedRootController();
                bool flag = autoLoad;
                ctrrootController.setMap(rootNode);
                ctrrootController.setMapName(selectedMap);
                ctrrootController.setMapsList(maplist);
                deactivate();
            }
        }

        public virtual void setNormalMode()
        {
            autoLoad = false;
            ((CTRRootController)Application.sharedRootController()).setPicker(true);
        }

        public virtual void setAutoLoadMap(NSString map)
        {
            autoLoad = true;
            ((CTRRootController)Application.sharedRootController()).setPicker(false);
            NSREL(selectedMap);
            selectedMap = (NSString)NSRET(map);
        }

        public virtual void onButtonPressed(int n)
        {
            if (n == 0)
            {
                loadList();
            }
        }

        public override void dealloc()
        {
            NSREL(selectedMap);
            base.dealloc();
        }

        private NSString selectedMap;

        private Dictionary<string, XMLNode> maplist;

        private bool autoLoad;
    }
}
