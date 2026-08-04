namespace CutTheRopeDX.GameMain
{
    internal static class AxeGrabBinding
    {
        /// <summary>
        /// Resolves the axe key requested by a grab. Explicit <c>axeNumber</c> wins; imported
        /// Time Travel <c>axed="true"</c> grabs fall back to <c>candyNumber</c> compatibility.
        /// </summary>
        public static string ResolveAxeNumber(string candyNumber, string axeNumber, bool axed)
        {
            return axeNumber ?? (axed ? candyNumber : null);
        }
    }
}
