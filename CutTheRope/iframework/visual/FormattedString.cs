namespace CutTheRope.iframework.visual
{
    internal sealed class FormattedString : FrameworkTypes
    {
        public FormattedString InitWithStringAndWidth(string str, float w)
        {
            string_ = str;
            width = w;
            return this;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                string_ = null;
            }
            base.Dispose(disposing);
        }

        public string string_;

        public float width;
    }
}
