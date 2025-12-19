namespace CutTheRope.GameMain
{
    /// <summary>
    /// Helper class for managing candy skins
    /// </summary>
    internal static class CandySkinHelper
    {
        /// <summary>
        /// Gets the candy resource name for a given skin index (0-50).
        /// </summary>
        /// <param name="skinIndex">The candy skin index (0 for candy_01, 1 for candy_02, etc.)</param>
        /// <returns>The resource name for the candy skin</returns>
        public static string GetCandyResource(int skinIndex)
        {
            return skinIndex switch
            {
                0 => Resources.Img.ObjCandy01New,
                1 => Resources.Img.ObjCandy02,
                2 => Resources.Img.ObjCandy03,
                3 => Resources.Img.ObjCandy04,
                4 => Resources.Img.ObjCandy05,
                5 => Resources.Img.ObjCandy06,
                6 => Resources.Img.ObjCandy07,
                7 => Resources.Img.ObjCandy08,
                8 => Resources.Img.ObjCandy09,
                9 => Resources.Img.ObjCandy10,
                10 => Resources.Img.ObjCandy11,
                11 => Resources.Img.ObjCandy12,
                12 => Resources.Img.ObjCandy13,
                13 => Resources.Img.ObjCandy14,
                14 => Resources.Img.ObjCandy15,
                15 => Resources.Img.ObjCandy16,
                16 => Resources.Img.ObjCandy17,
                17 => Resources.Img.ObjCandy18,
                18 => Resources.Img.ObjCandy19,
                19 => Resources.Img.ObjCandy20,
                20 => Resources.Img.ObjCandy21,
                21 => Resources.Img.ObjCandy22,
                22 => Resources.Img.ObjCandy23,
                23 => Resources.Img.ObjCandy24,
                24 => Resources.Img.ObjCandy25,
                25 => Resources.Img.ObjCandy26,
                26 => Resources.Img.ObjCandy27,
                27 => Resources.Img.ObjCandy28,
                28 => Resources.Img.ObjCandy29,
                29 => Resources.Img.ObjCandy30,
                30 => Resources.Img.ObjCandy31,
                31 => Resources.Img.ObjCandy32,
                32 => Resources.Img.ObjCandy33,
                33 => Resources.Img.ObjCandy34,
                34 => Resources.Img.ObjCandy35,
                35 => Resources.Img.ObjCandy36,
                36 => Resources.Img.ObjCandy37,
                37 => Resources.Img.ObjCandy38,
                38 => Resources.Img.ObjCandy39,
                39 => Resources.Img.ObjCandy40,
                40 => Resources.Img.ObjCandy41,
                41 => Resources.Img.ObjCandy42,
                42 => Resources.Img.ObjCandy43,
                43 => Resources.Img.ObjCandy44,
                44 => Resources.Img.ObjCandy45,
                45 => Resources.Img.ObjCandy46,
                46 => Resources.Img.ObjCandy47,
                47 => Resources.Img.ObjCandy48,
                48 => Resources.Img.ObjCandy49,
                49 => Resources.Img.ObjCandy50,
                50 => Resources.Img.ObjCandy51,
                _ => Resources.Img.ObjCandy01New  // Default to candy 01
            };
        }
    }
}
