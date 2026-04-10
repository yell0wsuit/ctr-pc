namespace CutTheRopeDX.Commons
{
    /// <summary>
    /// Receives notification when the lift scrollbar's active scroll point changes.
    /// </summary>
    internal interface ILiftScrollbarDelegate
    {
        /// <summary>
        /// Called when the active scroll point changes from <paramref name="pp"/> to <paramref name="cp"/>.
        /// </summary>
        /// <param name="pp">Previous scroll point.</param>
        /// <param name="cp">Current scroll point.</param>
        void ChangedActiveSpointFromTo(int pp, int cp);
    }
}
