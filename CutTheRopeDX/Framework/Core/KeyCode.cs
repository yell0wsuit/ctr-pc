namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Mirror of the XNA <c>Keys</c> members the game reads, Core-owned. Numeric values MUST match
    /// XNA's <c>Keys</c> enum (they are the Windows virtual-key codes) so the Desktop host can cast
    /// straight across without a lookup table. Only the keys Core code actually queries are listed.
    /// </summary>
    internal enum KeyCode
    {
        /// <summary>Enter / Return. XNA <c>Keys.Enter</c> = 13.</summary>
        Enter = 13,

        /// <summary>Escape. XNA <c>Keys.Escape</c> = 27.</summary>
        Escape = 27,

        /// <summary>Space bar. XNA <c>Keys.Space</c> = 32.</summary>
        Space = 32,

        /// <summary>Left arrow. XNA <c>Keys.Left</c> = 37.</summary>
        Left = 37,

        /// <summary>Right arrow. XNA <c>Keys.Right</c> = 39.</summary>
        Right = 39,

        /// <summary>F5. XNA <c>Keys.F5</c> = 116.</summary>
        F5 = 116
    }
}
