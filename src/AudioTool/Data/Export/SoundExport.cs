using System;
using Newtonsoft.Json;

namespace AudioTool.Data.Export
{
    [Serializable]
    public class SoundExport
    {
        [JsonProperty("pan")]
        public float? Pan { get; set; }

        [JsonProperty("volume")]
        public float? Volume { get; set; }

        [JsonProperty("pitch")]
        public float? Pitch { get; set; }

        [JsonProperty("looped")]
        public bool Looped { get; set; }

        [JsonProperty("instances")]
        public int? Instances { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }     

        public SoundExport(Sound sound)
        {
            Pan = sound.Pan;
            Volume = sound.Volume;
            Pitch = sound.Pitch;
            Looped = sound.Looped;
            Instances = sound.Instances;
            Name = sound.Name;
        }
    }
}