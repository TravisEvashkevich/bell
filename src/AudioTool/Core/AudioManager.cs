using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace AudioTool.Core
{
    public static class AudioManager
    {
        public static Dictionary<SoundEffectInstance, SoundState> States;
        public static event EventHandler<SoundStateChangedEventArgs> SoundStateChanged;

        private static readonly object Sync = new object();
        private static Timer _timer;

        static AudioManager()
        {
            States = new Dictionary<SoundEffectInstance, SoundState>();
            _timer = new Timer(TimerCallback, null, 0, 20);
        }

        private static void TimerCallback(object state)
        {
            lock (Sync)
            {
                CheckSoundEffectInstanceState();
            }
        }

        private static void CheckSoundEffectInstanceState()
        {
            foreach (var state in States.ToList())
            {
                    if (state.Key != null && !state.Key.IsDisposed)
                    {
                        if (state.Value != state.Key.State)
                        {
                            var e = new SoundStateChangedEventArgs()
                            {
                                Instance = state.Key,
                                OldState = state.Value
                            };

                            States[state.Key] = state.Key.State;

                            if (SoundStateChanged != null)
                            {
                                SoundStateChanged(null, e);
                            }
                        }
                    }
            }
        }

        public static void AddSoundInstance(SoundEffectInstance instance)
        {
            lock (Sync)
            {
                States.Add(instance, instance.State);
            }
        }

        public static void RemoveSoundInstance(SoundEffectInstance instance)
        {
            lock (Sync)
            {
                States.Remove(instance);
            }
        }
    }
}
