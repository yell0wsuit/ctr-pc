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

        public virtual NSObject Init()
        {
            return this;
        }

        public virtual void Dealloc()
        {
        }

        public virtual void Release()
        {
        }

        public virtual void Retain()
        {
        }

        public static void Free(object o)
        {
        }
    }
}
