using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AudioTool.Data.Export
{
    [Serializable]
    public class CueExport
    {
        [JsonProperty("Pan")]
        public float Pan { get; set; }
        [JsonProperty("Volume")]
        public float Volume { get; set; }
        [JsonProperty("Pitch")]
        public float Pitch { get; set; }
        [JsonProperty("Looped")]
        public bool Looped { get; set; }
        [JsonProperty("Instances")]
        public int Instances { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Playback")]
        public CuePlaybackMode PlaybackMode { get; set; }

        [JsonProperty("Sounds")]
        public List<SoundExport> Sounds { get; set; }

        public CueExport(Cue cue)
        {
            Name = cue.Name;
            Pan = cue.Pan;
            Volume = cue.Volume;
            Pitch = cue.Pitch;
            Looped = cue.Looped;
            Instances = cue.Instances;
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