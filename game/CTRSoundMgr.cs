using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.media;
using Microsoft.Xna.Framework.Audio;

namespace CutTheRope.game
{
	// Token: 0x0200007A RID: 122
	internal class CTRSoundMgr : SoundMgr
	{
		// Token: 0x060004D1 RID: 1233 RVA: 0x0001B7F9 File Offset: 0x000199F9
		public static void _playSound(int s)
		{
			if (Preferences._getBooleanForKey("SOUND_ON"))
			{
				Application.sharedSoundMgr().playSound(s);
			}
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x0001B812 File Offset: 0x00019A12
		public static void EnableLoopedSounds(bool bEnable)
		{
			CTRSoundMgr.s_EnableLoopedSounds = bEnable;
			if (!CTRSoundMgr.s_EnableLoopedSounds)
			{
				CTRSoundMgr._stopLoopedSounds();
			}
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x0001B826 File Offset: 0x00019A26
		public static SoundEffectInstance _playSoundLooped(int s)
		{
			if (CTRSoundMgr.s_EnableLoopedSounds && Preferences._getBooleanForKey("SOUND_ON"))
			{
				return Application.sharedSoundMgr().playSoundLooped(s);
			}
			return null;
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x0001B848 File Offset: 0x00019A48
		public static void _playRandomMusic(int minId, int maxId)
		{
			int num;
			do
			{
				num = MathHelper.RND_RANGE(minId, maxId);
			}
			while (num == CTRSoundMgr.prevMusic);
			CTRSoundMgr.prevMusic = num;
			CTRSoundMgr._playMusic(num);
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x0001B871 File Offset: 0x00019A71
		public static void _playMusic(int f)
		{
			if (Preferences._getBooleanForKey("MUSIC_ON"))
			{
				Application.sharedSoundMgr().playMusic(f);
			}
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x0001B88A File Offset: 0x00019A8A
		public static void _stopLoopedSounds()
		{
			Application.sharedSoundMgr().stopLoopedSounds();
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x0001B896 File Offset: 0x00019A96
		public static void _stopSounds()
		{
			Application.sharedSoundMgr().stopAllSounds();
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x0001B8A2 File Offset: 0x00019AA2
		public static void _stopAll()
		{
			CTRSoundMgr._stopSounds();
			CTRSoundMgr._stopMusic();
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x0001B8AE File Offset: 0x00019AAE
		public static void _stopMusic()
		{
			Application.sharedSoundMgr().stopMusic();
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x0001B8BA File Offset: 0x00019ABA
		public static void _pause()
		{
			Application.sharedSoundMgr().pause();
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0001B8C6 File Offset: 0x00019AC6
		public static void _unpause()
		{
			Application.sharedSoundMgr().unpause();
		}

		// Token: 0x04000380 RID: 896
		private static bool s_EnableLoopedSounds = true;

		// Token: 0x04000381 RID: 897
		private static int prevMusic = -1;
	}
}
