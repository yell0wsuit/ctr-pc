namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Defines OpenGL-style capability and state constants used by the renderer abstraction layer.
    /// Values are sequential internal IDs, not actual OpenGL enum values.
    /// </summary>
    public class GLenum
    {
        /// <summary>2D texture capability.</summary>
        internal const int GL_TEXTURE_2D = 0;

        /// <summary>Alpha blending capability.</summary>
        internal const int GL_BLEND = 1;

        /// <summary>Array buffer target for vertex data.</summary>
        internal const int GL_ARRAY_BUFFER = 2;

        /// <summary>Dynamic draw usage hint for buffer data.</summary>
        internal const int GL_DYNAMIC_DRAW = 3;

        /// <summary>Scissor test capability for clipping.</summary>
        internal const int GL_SCISSOR_TEST = 4;

        /// <summary>Float data type identifier.</summary>
        internal const int GL_FLOAT = 5;

        /// <summary>Unsigned short data type identifier.</summary>
        internal const int GL_UNSIGNED_SHORT = 6;

        /// <summary>Triangle list primitive type.</summary>
        internal const int GL_TRIANGLES = 7;

        /// <summary>Triangle strip primitive type.</summary>
        internal const int GL_TRIANGLE_STRIP = 8;

        /// <summary>Line strip primitive type.</summary>
        internal const int GL_LINE_STRIP = 9;

        /// <summary>Point list primitive type.</summary>
        internal const int GL_POINTS = 10;

        /// <summary>Vertex array client state.</summary>
        internal const int GL_VERTEX_ARRAY = 11;

        /// <summary>Texture coordinate array client state.</summary>
        internal const int GL_TEXTURE_COORD_ARRAY = 12;

        /// <summary>Color array client state.</summary>
        internal const int GL_COLOR_ARRAY = 13;

        /// <summary>Model-view matrix mode.</summary>
        internal const int GL_MODELVIEW = 14;

        /// <summary>Projection matrix mode.</summary>
        internal const int GL_PROJECTION = 15;

        /// <summary>Texture matrix mode.</summary>
        internal const int GL_TEXTURE = 16;

        /// <summary>Color matrix mode.</summary>
        internal const int GL_COLOR = 17;
    }
}
