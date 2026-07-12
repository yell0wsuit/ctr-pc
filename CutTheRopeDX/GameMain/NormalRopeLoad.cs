using System.Xml.Linq;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Policies used while loading fixed ropes and their initial candy state.</summary>
    internal static class NormalRopeLoad
    {
        /// <summary>
        /// Determines whether a fixed rope should be created for its target candy.
        /// </summary>
        /// <param name="targetCandyInLantern">
        /// <see langword="true"/> when the target candy starts captured in a lantern.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the target candy is available for a fixed rope; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool ShouldCreate(bool targetCandyInLantern)
        {
            return !targetCandyInLantern;
        }

        /// <summary>
        /// Determines whether the level contains a lantern whose candy is captured at startup.
        /// </summary>
        /// <param name="map">The level XML element whose descendants contain the game objects.</param>
        /// <returns>
        /// <see langword="true"/> when any lantern has <c>candyCaptured="true"</c>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool CandyStartsInLantern(XElement map)
        {
            foreach (XElement element in map.Descendants())
            {
                if (element.Name.LocalName == "lantern"
                    && bool.TryParse(element.Attribute("candyCaptured")?.Value, out bool captured)
                    && captured)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
