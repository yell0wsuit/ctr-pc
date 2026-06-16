namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure geometry for water buoyancy: whether a candy point is below the water
    /// surface and within the water column.
    /// </summary>
    internal static class WaterSubmersion
    {
        public static bool IsSubmerged(float pointX, float pointY, float waterX, float waterY, float waterWidth, float candyRadius)
        {
            return pointY > waterY
                && pointX + candyRadius >= waterX
                && pointX - candyRadius <= waterX + waterWidth;
        }
    }
}
