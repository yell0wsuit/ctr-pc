using CutTheRope.iframework;

namespace CutTheRope.ios
{
    internal class NSObject : FrameworkTypes
    {
        public static void NSREL(NSObject obj)
        {
        }

        public static object NSRET(object obj)
        {
            return obj;
        }

        public static NSString NSS(string s)
        {
            return new NSString(s);
        }

        public virtual NSObject init()
        {
            return this;
        }

        public virtual void dealloc()
        {
        }

        public virtual void release()
        {
        }

        public virtual void retain()
        {
        }

        public static void free(object o)
        {
        }
    }
}
