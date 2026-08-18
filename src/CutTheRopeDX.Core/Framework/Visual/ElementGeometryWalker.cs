using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Renders an element subtree's resolved geometry as deterministic text, so a layout can be
    /// compared against a recorded baseline without a framebuffer. Records position, size, scale
    /// and rotation only; visibility, color, draw order and texture selection are not represented.
    /// </summary>
    internal static class ElementGeometryWalker
    {
        /// <summary>
        /// Describes <paramref name="root"/> and its descendants, one line per element,
        /// depth-first in child-slot order, indented by depth. Each element's position is
        /// resolved as the walk reaches it, so the description needs no renderer and covers
        /// hidden elements, which a drawn tree never reaches.
        /// </summary>
        /// <param name="root">Element to describe.</param>
        /// <returns>The rendered description.</returns>
        public static string Describe(BaseElement root)
        {
            StringBuilder builder = new();
            Append(builder, root, 0);
            return builder.ToString();
        }

        /// <summary>
        /// Resolves one element's position, appends its line, and recurses into its children.
        /// </summary>
        /// <param name="builder">Destination buffer.</param>
        /// <param name="element">Element to append.</param>
        /// <param name="depth">Current tree depth, used for indentation.</param>
        private static void Append(StringBuilder builder, BaseElement element, int depth)
        {
            // Resolve before reading: a parent must be resolved before its children, because
            // CalculateTopLeft reads the parent's drawX and drawY.
            BaseElement.CalculateTopLeft(element);

            _ = builder.Append(' ', depth * 2);
            _ = builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} {1:0.##} {2:0.##} {3} {4} {5:0.###} {6:0.###} {7:0.##}\n",
                element.GetName() ?? element.GetType().Name,
                element.drawX,
                element.drawY,
                element.width,
                element.height,
                element.scaleX,
                element.scaleY,
                element.rotation);

            Dictionary<int, BaseElement> children = element.GetChilds();
            int emitted = 0;
            int childId = 0;
            while (emitted < children.Count)
            {
                if (children.TryGetValue(childId, out BaseElement child))
                {
                    if (child != null)
                    {
                        Append(builder, child, depth + 1);
                    }
                    emitted++;
                }
                childId++;
            }
        }
    }
}
