using System.Numerics;
using System.Runtime.InteropServices.JavaScript;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>Routes DOM pointer and keyboard events into Core.</summary>
    internal static partial class InputRouter
    {
        /// <summary>Pointer went down.</summary>
        public const int PhaseDown = 0;

        /// <summary>Pointer moved.</summary>
        public const int PhaseMove = 1;

        /// <summary>Pointer went up.</summary>
        public const int PhaseUp = 2;

        internal static BrowserHostApp Host { get; set; }

        /// <summary>Handles one pointer event in canvas backing pixels.</summary>
        /// <param name="x">Pointer X in canvas backing pixels.</param>
        /// <param name="y">Pointer Y in canvas backing pixels.</param>
        /// <param name="phase">One of the pointer phase constants.</param>
        [JSExport]
        internal static void OnPointer(double x, double y, int phase)
        {
            ScreenPresentation presentation = ScreenPresentation.Instance;
            int viewX = presentation.TransformWindowToViewX((int)x);
            int viewY = presentation.TransformWindowToViewY((int)y);
            float logicalX = presentation.TransformViewToGameX(viewX);
            float logicalY = presentation.TransformViewToGameY(viewY);

            _ = Application.SharedRootController().MouseMoved(logicalX, logicalY);

            TouchLocationState state = phase switch
            {
                PhaseDown => TouchLocationState.Pressed,
                PhaseUp => TouchLocationState.Released,
                _ => TouchLocationState.Moved,
            };
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(
                [new TouchLocation(0, state, new Vector2(viewX, viewY))]);
        }

        /// <summary>Handles one keyboard transition.</summary>
        /// <param name="code">The DOM <c>KeyboardEvent.code</c> value.</param>
        /// <param name="down">Whether the key went down.</param>
        [JSExport]
        internal static void OnKey(string code, bool down)
        {
            KeyCode? mapped = Map(code);
            if (mapped is not null)
            {
                Host?.SetKey(mapped.Value, down);
            }
        }

        private static KeyCode? Map(string code)
        {
            return code switch
            {
                "Escape" => KeyCode.Escape,
                "Space" => KeyCode.Space,
                "Enter" => KeyCode.Enter,
                "ArrowLeft" => KeyCode.Left,
                "ArrowRight" => KeyCode.Right,
                "F5" => KeyCode.F5,
                _ => null,
            };
        }
    }
}
