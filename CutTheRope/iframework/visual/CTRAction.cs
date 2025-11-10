using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
    internal class CTRAction : NSObject
    {
        public CTRAction()
        {
            data = new ActionData();
        }

        public static CTRAction createAction(BaseElement target, string action, int p, int sp)
        {
            CTRAction action2 = (CTRAction)new CTRAction().init();
            action2.actionTarget = target;
            action2.data.actionName = action;
            action2.data.actionParam = p;
            action2.data.actionSubParam = sp;
            return action2;
        }

        public BaseElement actionTarget;

        public ActionData data;
    }
}
