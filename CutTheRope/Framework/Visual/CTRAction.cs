using System;

namespace CutTheRope.Framework.Visual
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
            action2.data.actionParamFloat = p;
            action2.data.actionSubParamFloat = sp;
            return action2;
        }

        public static CTRAction CreateAction(BaseElement target, string action, float p, float sp)
        {
            CTRAction action2 = new()
            {
                actionTarget = target
            };
            action2.data.actionName = action;
            action2.data.actionParam = (int)MathF.Round(p);
            action2.data.actionSubParam = (int)MathF.Round(sp);
            action2.data.actionParamFloat = p;
            action2.data.actionSubParamFloat = sp;
            return action2;
        }

        public BaseElement actionTarget;

        public ActionData data;
    }
}
