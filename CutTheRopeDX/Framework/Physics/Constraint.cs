namespace CutTheRopeDX.Framework.Physics
{
    /// <summary>
    /// Connection data describing how one <see cref="ConstraintedPoint"/> is constrained relative to another.
    /// </summary>
    internal sealed class Constraint : FrameworkTypes
    {
        /// <summary>
        /// The other point referenced by this constraint.
        /// </summary>
        public ConstraintedPoint cp;

        /// <summary>
        /// Target length used when enforcing the constraint.
        /// </summary>
        public float restLength;

        /// <summary>
        /// Rule used when evaluating the constraint.
        /// </summary>
        public CONSTRAINT type;

        /// <summary>
        /// Available constraint rules for point-to-point distance evaluation.
        /// </summary>
        public enum CONSTRAINT
        {
            /// <summary>
            /// Enforces an exact distance.
            /// </summary>
            DISTANCE,

            /// <summary>
            /// Prevents the distance from becoming greater than the rest length.
            /// </summary>
            NOT_MORE_THAN,

            /// <summary>
            /// Prevents the distance from becoming less than the rest length.
            /// </summary>
            NOT_LESS_THAN
        }
    }
}
