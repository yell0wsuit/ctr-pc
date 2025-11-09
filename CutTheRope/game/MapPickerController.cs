using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.game
{
    internal class MapPickerController : ViewController, ButtonDelegate
    {
        public override NSObject initWithParent(ViewController p)
        {
            if (base.initWithParent(p) != null)
            {
                this.selectedMap = null;
                this.maplist = null;
                this.createPickerView();
                View view = (View)new View().initFullscreen();
                RectangleElement rectangleElement = (RectangleElement)new RectangleElement().init();
                rectangleElement.color = RGBAColor.whiteRGBA;
                rectangleElement.width = (int)FrameworkTypes.SCREEN_WIDTH;
                rectangleElement.height = (int)FrameworkTypes.SCREEN_HEIGHT;
                view.addChild(rectangleElement);
                FontGeneric font = Application.getFont(4);
                Text text = new Text().initWithFont(font);
                text.setString(NSObject.NSS("Loading..."));
                text.anchor = text.parentAnchor = 18;
                view.addChild(text);
                this.addViewwithID(view, 1);
                this.setNormalMode();
            }
            return this;
        }

        public virtual void createPickerView()
        {
            View view = (View)new View().initFullscreen();
            RectangleElement rectangleElement = (RectangleElement)new RectangleElement().init();
            rectangleElement.color = RGBAColor.whiteRGBA;
            rectangleElement.width = (int)FrameworkTypes.SCREEN_WIDTH;
            rectangleElement.height = (int)FrameworkTypes.SCREEN_HEIGHT;
            view.addChild(rectangleElement);
            FontGeneric font = Application.getFont(4);
            Text text = new Text().initWithFont(font);
            text.setString(NSObject.NSS("START"));
            Text text2 = new Text().initWithFont(font);
            text2.setString(NSObject.NSS("START"));
            text2.scaleX = text2.scaleY = 1.2f;
            Button button = new Button().initWithUpElementDownElementandID(text, text2, 0);
            button.anchor = button.parentAnchor = 34;
            button.delegateButtonDelegate = this;
            view.addChild(button);
            this.addViewwithID(view, 0);
        }

        public override void activate()
        {
            base.activate();
            if (this.autoLoad)
            {
                string text = "maps/";
                NSString nsstring = this.selectedMap;
                NSString nSString = NSObject.NSS(text + (nsstring?.ToString()));
                XMLNode xMLNode = XMLNode.parseXML(nSString.ToString());
                this.xmlLoaderFinishedWithfromwithSuccess(xMLNode, nSString, xMLNode != null);
                return;
            }
            this.showView(0);
            this.loadList();
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
                bool flag = this.autoLoad;
                ctrrootController.setMap(rootNode);
                ctrrootController.setMapName(this.selectedMap);
                ctrrootController.setMapsList(this.maplist);
                this.deactivate();
            }
        }

        public virtual void setNormalMode()
        {
            this.autoLoad = false;
            ((CTRRootController)Application.sharedRootController()).setPicker(true);
        }

        public virtual void setAutoLoadMap(NSString map)
        {
            this.autoLoad = true;
            ((CTRRootController)Application.sharedRootController()).setPicker(false);
            NSObject.NSREL(this.selectedMap);
            this.selectedMap = (NSString)NSObject.NSRET(map);
        }

        public virtual void onButtonPressed(int n)
        {
            if (n == 0)
            {
                this.loadList();
            }
        }

        public override void dealloc()
        {
            NSObject.NSREL(this.selectedMap);
            base.dealloc();
        }

        private NSString selectedMap;

        private Dictionary<string, XMLNode> maplist;

        private bool autoLoad;
    }
}
