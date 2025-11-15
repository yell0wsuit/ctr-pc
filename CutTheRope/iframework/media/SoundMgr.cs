using System;
using System.Collections.Generic;

using CutTheRope.game;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace CutTheRope.iframework.media
{
    internal class SoundMgr : FrameworkTypes
    {
        public SoundMgr()
        {
            LoadedSounds = [];
            activeSounds = [];
            activeLoopedSounds = [];
        }

        public static void SetContentManager(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public void FreeSound(int resId)
        {
            _ = LoadedSounds.Remove(resId);
        }

        public SoundEffect GetSound(int resId)
        {
            if (resId is >= 145 and <= 148)
            {
                return null;
            }
            if (LoadedSounds.TryGetValue(resId, out SoundEffect value))
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

        public virtual void PlaySound(int sid)
        {
            ClearStopped();
            activeSounds.Add(Play(sid, false));
        }

        public virtual SoundEffectInstance PlaySoundLooped(int sid)
        {
            ClearStopped();
            SoundEffectInstance soundEffectInstance = Play(sid, true);
            activeLoopedSounds.Add(soundEffectInstance);
            return soundEffectInstance;
        }

        public virtual void PlayMusic(int resId)
        {
            StopMusic();
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

        public virtual void StopLoopedSounds()
        {
            StopList(activeLoopedSounds);
            activeLoopedSounds.Clear();
        }

        public virtual void StopAllSounds()
        {
            StopLoopedSounds();
        }

        public virtual void StopMusic()
        {
            try
            {
                MediaPlayer.Stop();
            }
            catch (Exception)
            {
            }
        }

        public virtual void Suspend()
        {
        }

        public virtual void Resume()
        {
        }

        public virtual void Pause()
        {
            try
            {
                ChangeListState(activeLoopedSounds, SoundState.Playing, SoundState.Paused);
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
                }
            }
            catch (Exception)
            {
            }
        }

        public virtual void Unpause()
        {
            try
            {
                ChangeListState(activeLoopedSounds, SoundState.Paused, SoundState.Playing);
                if (MediaPlayer.State == MediaState.Paused)
                {
                    MediaPlayer.Resume();
                }
            }
            catch (Exception)
            {
            }
        }

        private SoundEffectInstance Play(int sid, bool l)
        {
            SoundEffectInstance soundEffectInstance = null;
            SoundEffectInstance soundEffectInstance2;
            try
            {
                soundEffectInstance = GetSound(sid).CreateInstance();
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

        private static void StopList(List<SoundEffectInstance> list)
        {
            foreach (SoundEffectInstance item in list)
            {
                item?.Stop();
            }
        }

        private static void ChangeListState(List<SoundEffectInstance> list, SoundState fromState, SoundState toState)
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

        private readonly Dictionary<int, SoundEffect> LoadedSounds;

        private List<SoundEffectInstance> activeSounds;

        private readonly List<SoundEffectInstance> activeLoopedSounds;
    }
}
