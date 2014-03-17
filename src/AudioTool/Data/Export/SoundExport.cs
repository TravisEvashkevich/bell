using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace AudioTool.Data.Export
{
    [Serializable]
    public class SoundExport
    {
        [JsonProperty("Pan")]
        public float? Pan { get; set; }
        [JsonProperty("Volume")]
        public float? Volume { get; set; }
        [JsonProperty("Pitch")]
        public float? Pitch { get; set; }
        [JsonProperty("Looped")]
        public bool Looped { get; set; }
        [JsonProperty("Instances")]
        public int? Instances { get; set; }
         [JsonProperty("Name")]
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