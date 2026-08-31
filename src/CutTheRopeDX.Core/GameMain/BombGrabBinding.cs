namespace CutTheRopeDX.GameMain
{
    internal static class BombGrabBinding
    {
        /// <summary>
        /// Resolves the bomb key requested by a grab. Explicit <c>bombNumber</c> wins; imported
        /// Time Travel <c>bombed="true"</c> grabs fall back to <c>candyNumber</c> compatibility.
        /// </summary>
        public static string ResolveBombNumber(string candyNumber, string bombNumber, bool bombed)
        {
            return bombNumber ?? (bombed ? candyNumber : null);
        }
    }
}
