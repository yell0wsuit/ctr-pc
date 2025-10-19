using System;
using System.Collections.Generic;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
	// Token: 0x02000084 RID: 132
	internal class MapPickerController : ViewController, ButtonDelegate
	{
		// Token: 0x0600055C RID: 1372 RVA: 0x0002A3E0 File Offset: 0x000285E0
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
				text.anchor = (text.parentAnchor = 18);
				view.addChild(text);
				this.addViewwithID(view, 1);
				this.setNormalMode();
			}
			return this;
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x0002A4A8 File Offset: 0x000286A8
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
			text2.scaleX = (text2.scaleY = 1.2f);
			Button button = new Button().initWithUpElementDownElementandID(text, text2, 0);
			button.anchor = (button.parentAnchor = 34);
			button.delegateButtonDelegate = this;
			view.addChild(button);
			this.addViewwithID(view, 0);
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0002A598 File Offset: 0x00028798
		public override void activate()
		{
			base.activate();
			if (this.autoLoad)
			{
				string text = "maps/";
				NSString nsstring = this.selectedMap;
				NSString nSString = NSObject.NSS(text + ((nsstring != null) ? nsstring.ToString() : null));
				XMLNode xMLNode = XMLNode.parseXML(nSString.ToString());
				this.xmlLoaderFinishedWithfromwithSuccess(xMLNode, nSString, xMLNode != null);
				return;
			}
			this.showView(0);
			this.loadList();
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x0002A5FB File Offset: 0x000287FB
		public virtual void loadList()
		{
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x0002A5FD File Offset: 0x000287FD
		public override void deactivate()
		{
			base.deactivate();
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x0002A605 File Offset: 0x00028805
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

		// Token: 0x06000562 RID: 1378 RVA: 0x0002A63F File Offset: 0x0002883F
		public virtual void setNormalMode()
		{
			this.autoLoad = false;
			((CTRRootController)Application.sharedRootController()).setPicker(true);
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x0002A658 File Offset: 0x00028858
		public virtual void setAutoLoadMap(NSString map)
		{
			this.autoLoad = true;
			((CTRRootController)Application.sharedRootController()).setPicker(false);
			NSObject.NSREL(this.selectedMap);
			this.selectedMap = (NSString)NSObject.NSRET(map);
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x0002A68D File Offset: 0x0002888D
		public virtual void onButtonPressed(int n)
		{
			if (n == 0)
			{
				this.loadList();
			}
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x0002A698 File Offset: 0x00028898
		public override void dealloc()
		{
			NSObject.NSREL(this.selectedMap);
			base.dealloc();
		}

		// Token: 0x0400044C RID: 1100
		private NSString selectedMap;

		// Token: 0x0400044D RID: 1101
		private Dictionary<string, XMLNode> maplist;

		// Token: 0x0400044E RID: 1102
		private bool autoLoad;
	}
}
