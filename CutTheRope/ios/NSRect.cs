using CutTheRope.iframework;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.ios
{
    internal struct NSRect
    {
        public NSRect(float _x, float _y, float _w, float _h)
        {
            origin = new NSPoint
            {
                x = _x,
                y = _y
            };
            size = new NSSize
            {
                width = _w,
                height = _h
            };
        }

        public NSRect(Rectangle xnaRect)
        {
            origin = new NSPoint
            {
                x = (float)xnaRect.X,
                y = (float)xnaRect.Y
            };
            size = new NSSize
            {
                width = (float)xnaRect.Width,
                height = (float)xnaRect.Height
            };
        }

        public NSRect(CTRRectangle ctrRect)
        {
            origin = new NSPoint
            {
                x = ctrRect.x,
                y = ctrRect.y
            };
            size = new NSSize
            {
                width = ctrRect.w,
                height = ctrRect.h
            };
        }

        public NSPoint origin;

        public NSSize size;
    }
}
