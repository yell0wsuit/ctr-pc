namespace CutTheRope.Commons
{
    /// <summary>
    /// Receives notification when the lift scrollbar's active scroll point changes.
    /// </summary>
    internal interface ILiftScrollbarDelegate
    {
        /// <summary>Called when the active scroll point changes from <paramref name="pp"/> to <paramref name="cp"/>.</summary>
        void ChangedActiveSpointFromTo(int pp, int cp);
    }
}
