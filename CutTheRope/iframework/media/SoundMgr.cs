using CutTheRope.game;
using CutTheRope.ios;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.media
{
    internal class SoundMgr : NSObject
    {
        public new SoundMgr init()
        {
            LoadedSounds = [];
            activeSounds = [];
            activeLoopedSounds = [];
            return this;
        }

        public static void SetContentManager(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public void freeSound(int resId)
        {
            LoadedSounds.Remove(resId);
        }

        public SoundEffect getSound(int resId)
        {
            if (resId is >= 145 and <= 148)
            {
                return null;
            }
            SoundEffect value;
            if (LoadedSounds.TryGetValue(resId, out value))
            {
                return value;
            }
            SoundEffect soundEffect;
            try
            {
                value = _contentManager.Load<SoundEffect>("sounds/" + CTRResourceMgr.XNA_ResName(resId));
                LoadedSounds.Add(resId, value);
                soundEffect = value;
            }
            catch (Exception)
            {
                soundEffect = value;
            }
            return soundEffect;
        }

        private void ClearStopped()
        {
            List<SoundEffectInstance> list = [];
            foreach (SoundEffectInstance activeSound in activeSounds)
            {
                if (activeSound != null && activeSound.State != SoundState.Stopped)
                {
                    list.Add(activeSound);
                }
            }
            activeSounds.Clear();
            activeSounds = list;
        }

        public virtual void playSound(int sid)
        {
            ClearStopped();
            activeSounds.Add(play(sid, false));
        }

        public virtual SoundEffectInstance playSoundLooped(int sid)
        {
            ClearStopped();
            SoundEffectInstance soundEffectInstance = play(sid, true);
            activeLoopedSounds.Add(soundEffectInstance);
            return soundEffectInstance;
        }

        public virtual void playMusic(int resId)
        {
            stopMusic();
            Song song = _contentManager.Load<Song>("sounds/" + CTRResourceMgr.XNA_ResName(resId));
            MediaPlayer.IsRepeating = true;
            try
            {
                MediaPlayer.Play(song);
            }
            catch (Exception)
            {
            }
        }

        public virtual void stopLoopedSounds()
        {
            stopList(activeLoopedSounds);
            activeLoopedSounds.Clear();
        }

        public virtual void stopAllSounds()
        {
            stopLoopedSounds();
        }

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

        public virtual void suspend()
        {
        }

        public virtual void resume()
        {
        }

        public virtual void pause()
        {
            try
            {
                changeListState(activeLoopedSounds, SoundState.Playing, SoundState.Paused);
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
                }
            }
            catch (Exception)
            {
            }
        }

        public virtual void unpause()
        {
            try
            {
                changeListState(activeLoopedSounds, SoundState.Paused, SoundState.Playing);
                if (MediaPlayer.State == MediaState.Paused)
                {
                    MediaPlayer.Resume();
                }
            }
            catch (Exception)
            {
            }
        }

        private SoundEffectInstance play(int sid, bool l)
        {
            SoundEffectInstance soundEffectInstance = null;
            SoundEffectInstance soundEffectInstance2;
            try
            {
                soundEffectInstance = getSound(sid).CreateInstance();
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

        private static void stopList(List<SoundEffectInstance> list)
        {
            foreach (SoundEffectInstance item in list)
            {
                item?.Stop();
            }
        }

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

        private static ContentManager _contentManager;

        private Dictionary<int, SoundEffect> LoadedSounds;

        private List<SoundEffectInstance> activeSounds;

        private List<SoundEffectInstance> activeLoopedSounds;
    }
}
