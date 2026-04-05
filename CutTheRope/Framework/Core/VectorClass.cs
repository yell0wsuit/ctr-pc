namespace CutTheRope.Framework.Core
{
    /// <summary>
    /// Reference-type wrapper around a <see cref="Vector"/> value.
    /// </summary>
    public class VectorClass
    {
        /// <summary>
        /// Initializes a new <see cref="VectorClass"/> with a default vector.
        /// </summary>
        public VectorClass()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="VectorClass"/> with the specified vector.
        /// </summary>
        /// <param name="Value">Vector value to wrap.</param>
        public VectorClass(Vector Value)
        {
            VectorPoint = Value;
        }

        /// <summary>
        /// The wrapped vector value.
        /// </summary>
        public Vector VectorPoint { get; set; }
    }
}
