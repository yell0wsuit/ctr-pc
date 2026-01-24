namespace CutTheRope.Framework.Core
{
    public class VectorClass
    {
        public VectorClass()
        {
        }

        public VectorClass(Vector Value)
        {
            VectorPoint = Value;
        }

        public Vector VectorPoint { get; set; }
    }
}
