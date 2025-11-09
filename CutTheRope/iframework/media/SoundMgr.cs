using CutTheRope.game;
using CutTheRope.ios;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.media
{
    // Token: 0x0200005C RID: 92
    internal class SoundMgr : NSObject
    {
        // Token: 0x06000317 RID: 791 RVA: 0x000124C7 File Offset: 0x000106C7
        public new SoundMgr init()
        {
            this.LoadedSounds = new Dictionary<int, SoundEffect>();
            this.activeSounds = new List<SoundEffectInstance>();
            this.activeLoopedSounds = new List<SoundEffectInstance>();
            return this;
        }

        // Token: 0x06000318 RID: 792 RVA: 0x000124EB File Offset: 0x000106EB
        public static void SetContentManager(ContentManager contentManager)
        {
            SoundMgr._contentManager = contentManager;
        }

        // Token: 0x06000319 RID: 793 RVA: 0x000124F3 File Offset: 0x000106F3
        public void freeSound(int resId)
        {
            this.LoadedSounds.Remove(resId);
        }

        // Token: 0x0600031A RID: 794 RVA: 0x00012504 File Offset: 0x00010704
        public SoundEffect getSound(int resId)
        {
            if (resId >= 145 && resId <= 148)
            {
                return null;
            }
            SoundEffect value;
            if (this.LoadedSounds.TryGetValue(resId, out value))
            {
                return value;
            }
            SoundEffect soundEffect;
            try
            {
                value = SoundMgr._contentManager.Load<SoundEffect>("sounds/" + CTRResourceMgr.XNA_ResName(resId));
                this.LoadedSounds.Add(resId, value);
                soundEffect = value;
            }
            catch (Exception)
            {
                soundEffect = value;
            }
            return soundEffect;
        }

        // Token: 0x0600031B RID: 795 RVA: 0x00012578 File Offset: 0x00010778
        private void ClearStopped()
        {
            List<SoundEffectInstance> list = new();
            foreach (SoundEffectInstance activeSound in this.activeSounds)
            {
                if (activeSound != null && activeSound.State != SoundState.Stopped)
                {
                    list.Add(activeSound);
                }
            }
            this.activeSounds.Clear();
            this.activeSounds = list;
        }

        // Token: 0x0600031C RID: 796 RVA: 0x000125F0 File Offset: 0x000107F0
        public virtual void playSound(int sid)
        {
            this.ClearStopped();
            this.activeSounds.Add(this.play(sid, false));
        }

        // Token: 0x0600031D RID: 797 RVA: 0x0001260C File Offset: 0x0001080C
        public virtual SoundEffectInstance playSoundLooped(int sid)
        {
            this.ClearStopped();
            SoundEffectInstance soundEffectInstance = this.play(sid, true);
            this.activeLoopedSounds.Add(soundEffectInstance);
            return soundEffectInstance;
        }

        // Token: 0x0600031E RID: 798 RVA: 0x00012638 File Offset: 0x00010838
        public virtual void playMusic(int resId)
        {
            this.stopMusic();
            Song song = SoundMgr._contentManager.Load<Song>("sounds/" + CTRResourceMgr.XNA_ResName(resId));
            MediaPlayer.IsRepeating = true;
            try
            {
                MediaPlayer.Play(song);
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x0600031F RID: 799 RVA: 0x00012688 File Offset: 0x00010888
        public virtual void stopLoopedSounds()
        {
            SoundMgr.stopList(this.activeLoopedSounds);
            this.activeLoopedSounds.Clear();
        }

        // Token: 0x06000320 RID: 800 RVA: 0x000126A0 File Offset: 0x000108A0
        public virtual void stopAllSounds()
        {
            this.stopLoopedSounds();
        }

        // Token: 0x06000321 RID: 801 RVA: 0x000126A8 File Offset: 0x000108A8
        public virtual void stopMusic()
        {
            try
            {
                MediaPlayer.Stop();
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000322 RID: 802 RVA: 0x000126D0 File Offset: 0x000108D0
        public virtual void suspend()
        {
        }

        // Token: 0x06000323 RID: 803 RVA: 0x000126D2 File Offset: 0x000108D2
        public virtual void resume()
        {
        }

        // Token: 0x06000324 RID: 804 RVA: 0x000126D4 File Offset: 0x000108D4
        public virtual void pause()
        {
            try
            {
                SoundMgr.changeListState(this.activeLoopedSounds, SoundState.Playing, SoundState.Paused);
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
                }
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000325 RID: 805 RVA: 0x00012710 File Offset: 0x00010910
        public virtual void unpause()
        {
            try
            {
                SoundMgr.changeListState(this.activeLoopedSounds, SoundState.Paused, SoundState.Playing);
                if (MediaPlayer.State == MediaState.Paused)
                {
                    MediaPlayer.Resume();
                }
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000326 RID: 806 RVA: 0x0001274C File Offset: 0x0001094C
        private SoundEffectInstance play(int sid, bool l)
        {
            SoundEffectInstance soundEffectInstance = null;
            SoundEffectInstance soundEffectInstance2;
            try
            {
                soundEffectInstance = this.getSound(sid).CreateInstance();
                soundEffectInstance.IsLooped = l;
                soundEffectInstance.Play();
                soundEffectInstance2 = soundEffectInstance;
            }
            catch (Exception)
            {
                soundEffectInstance2 = soundEffectInstance;
            }
            return soundEffectInstance2;
        }

        // Token: 0x06000327 RID: 807 RVA: 0x00012790 File Offset: 0x00010990
        private static void stopList(List<SoundEffectInstance> list)
        {
            foreach (SoundEffectInstance item in list)
            {
                if (item != null)
                {
                    item.Stop();
                }
            }
        }

        // Token: 0x06000328 RID: 808 RVA: 0x000127E0 File Offset: 0x000109E0
        private static void changeListState(List<SoundEffectInstance> list, SoundState fromState, SoundState toState)
        {
            foreach (SoundEffectInstance item in list)
            {
                if (item != null && item.State == fromState)
                {
                    if (toState != SoundState.Playing)
                    {
                        if (toState == SoundState.Paused)
                        {
                            item.Pause();
                        }
                    }
                    else
                    {
                        item.Resume();
                    }
                }
            }
        }

        // Token: 0x04000267 RID: 615
        private static ContentManager _contentManager;

        // Token: 0x04000268 RID: 616
        private Dictionary<int, SoundEffect> LoadedSounds;

        // Token: 0x04000269 RID: 617
        private List<SoundEffectInstance> activeSounds;

        // Token: 0x0400026A RID: 618
        private List<SoundEffectInstance> activeLoopedSounds;
    }
}
