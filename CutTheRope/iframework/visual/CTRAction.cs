namespace CutTheRope.iframework.visual
{
    internal sealed class CTRAction : FrameworkTypes
    {
        public CTRAction()
        {
            data = new ActionData();
        }

        public static CTRAction CreateAction(BaseElement target, string action, int p, int sp)
        {
            CTRAction action2 = new()
            {
                actionTarget = target
            };
            action2.data.actionName = action;
            action2.data.actionParam = p;
            action2.data.actionSubParam = sp;
            return action2;
        }

        public BaseElement actionTarget;

        public ActionData data;
    }
}
