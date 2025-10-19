using System;

namespace CutTheRope
{
	// Token: 0x02000006 RID: 6
	internal static class Program
	{
		// Token: 0x06000039 RID: 57 RVA: 0x00002FC4 File Offset: 0x000011C4
		private static void Main(string[] args)
		{
			using (Game1 game = new Game1())
			{
				game.Run();
			}
		}
	}
}
