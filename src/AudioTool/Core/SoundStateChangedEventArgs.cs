using System;
using Microsoft.Xna.Framework.Audio;

namespace AudioTool.Core
{
    public class SoundStateChangedEventArgs : EventArgs
    {
        public SoundEffectInstance Instance { get; set; }
        public SoundState OldState { get; set; }
    }
}