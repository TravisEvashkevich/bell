using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AudioTool.Data.Export
{
    [Serializable]
    public class CueExport
    {
        [JsonProperty("pan")]
        public float Pan { get; set; }

        [JsonProperty("volume")]
        public float Volume { get; set; }

        [JsonProperty("pitch")]
        public float Pitch { get; set; }

        [JsonProperty("looped")]
        public bool Looped { get; set; }

        [JsonProperty("instances")]
        public int Instances { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("playback")]
        public CuePlaybackMode PlaybackMode { get; set; }

        [JsonProperty("sounds")]
        public List<SoundExport> Sounds { get; set; }

        public CueExport(Cue cue)
        {
            Name = cue.Name;
            Pan = cue.Pan;
            Volume = cue.Volume;
            Pitch = cue.Pitch;
            Looped = cue.Looped;
            Name = cue.Name;
            PlaybackMode = cue.CuePlaybackMode;
            Sounds = new List<SoundExport>(cue.Children.Count);
            foreach (Sound sound in cue.Children)
            {
                Sounds.Add(new SoundExport(sound));
            }
        }
    }
}