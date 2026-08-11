using System;
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
            Apply(Matrix4x4.CreateRotationZ(degrees * (float)Math.PI / 180f));
        }

        /// <summary>Applies the skew the original renderer used for skewed sprites.</summary>
        /// <remarks>
        /// This is Flash's skew, which the animation data is authored against: each axis rotates
        /// by its own angle rather than shearing while the other axis stays put. That makes the
        /// matrix <c>[cos(skewY), sin(skewY); -sin(skewX), cos(skewX)]</c> — a plain shear of
        /// <c>tan</c> without the cosine terms leaves both axes at their original length and
        /// skews them the wrong way, which pulls animated parts off the bodies they belong to.
        /// </remarks>
        /// <param name="skewXDegrees">Rotation of the Y axis, in degrees.</param>
        /// <param name="skewYDegrees">Rotation of the X axis, in degrees.</param>
        public void Skew(float skewXDegrees, float skewYDegrees)
        {
            float skewX = skewXDegrees * (float)Math.PI / 180f;
            float skewY = skewYDegrees * (float)Math.PI / 180f;

            Matrix4x4 shear = Matrix4x4.Identity;
            shear.M11 = (float)Math.Cos(skewY);
            shear.M12 = (float)Math.Sin(skewY);
            shear.M21 = -(float)Math.Sin(skewX);
            shear.M22 = (float)Math.Cos(skewX);
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
