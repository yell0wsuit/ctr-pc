using System.Collections.Generic;
using System.Numerics;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// The GL-ES-1 style matrix stack the renderer API is written against.
    /// </summary>
    /// <remarks>
    /// Transforms pre-multiply, matching fixed-function OpenGL: a translate issued before
    /// a scale still applies after it, so call sequences port from the original engine
    /// unchanged. Kept in Core, free of any graphics API, so both hosts share one
    /// implementation and the headless suite can verify it.
    /// </remarks>
    internal sealed class MatrixStack
    {
        private readonly Stack<Matrix4x4> _saved = new();

        /// <summary>The current model-view matrix.</summary>
        public Matrix4x4 ModelView { get; private set; } = Matrix4x4.Identity;

        /// <summary>The current projection matrix.</summary>
        public Matrix4x4 Projection { get; private set; } = Matrix4x4.Identity;

        /// <summary>Resets the model-view or projection matrix to identity.</summary>
        /// <param name="projection">Whether to reset the projection rather than the model-view.</param>
        public void LoadIdentity(bool projection)
        {
            if (projection)
            {
                Projection = Matrix4x4.Identity;
            }
            else
            {
                ModelView = Matrix4x4.Identity;
            }
        }

        /// <summary>Saves the model-view matrix.</summary>
        public void Push()
        {
            _saved.Push(ModelView);
        }

        /// <summary>Restores the most recently saved model-view matrix.</summary>
        public void Pop()
        {
            if (_saved.Count > 0)
            {
                ModelView = _saved.Pop();
            }
        }

        private void Apply(Matrix4x4 transform)
        {
            ModelView = transform * ModelView;
        }

        /// <summary>Translates the model-view matrix.</summary>
        /// <param name="x">X offset.</param>
        /// <param name="y">Y offset.</param>
        public void Translate(float x, float y)
        {
            Apply(Matrix4x4.CreateTranslation(x, y, 0f));
        }

        /// <summary>Scales the model-view matrix.</summary>
        /// <param name="x">X scale.</param>
        /// <param name="y">Y scale.</param>
        public void Scale(float x, float y)
        {
            Apply(Matrix4x4.CreateScale(x, y, 1f));
        }

        /// <summary>Rotates the model-view matrix about Z.</summary>
        /// <param name="degrees">Rotation in degrees.</param>
        public void RotateDegrees(float degrees)
        {
            Apply(Matrix4x4.CreateRotationZ(degrees * (float)System.Math.PI / 180f));
        }

        /// <summary>Applies the shear the original iOS renderer used for skewed sprites.</summary>
        /// <param name="skewXDegrees">Shear of X by Y, in degrees.</param>
        /// <param name="skewYDegrees">Shear of Y by X, in degrees.</param>
        public void Skew(float skewXDegrees, float skewYDegrees)
        {
            Matrix4x4 shear = Matrix4x4.Identity;
            shear.M21 = (float)System.Math.Tan(skewXDegrees * System.Math.PI / 180.0);
            shear.M12 = (float)System.Math.Tan(skewYDegrees * System.Math.PI / 180.0);
            Apply(shear);
        }

        /// <summary>Replaces the projection with an orthographic frustum.</summary>
        /// <param name="left">Left plane.</param>
        /// <param name="right">Right plane.</param>
        /// <param name="bottom">Bottom plane.</param>
        /// <param name="top">Top plane.</param>
        /// <param name="near">Near plane.</param>
        /// <param name="far">Far plane.</param>
        public void SetOrthographic(
            float left, float right, float bottom, float top, float near, float far)
        {
            Projection = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
        }
    }
}
