using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AudioTool.Data.Export
{
    [Serializable]
    public class FolderExport
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Cues", Required = Required.AllowNull)]
        public List<CueExport> Cues { get; set; }

        [JsonProperty("Folders", Required = Required.AllowNull)]
        public List<FolderExport> Folders { get; set; }

        public FolderExport(Folder folder)
        {

            Name = folder.Name;

            var cues = folder.Children.Where(p => p is Cue).Cast<Cue>();
            var folders = folder.Children.Where(p => p is Folder).Cast<Folder>();

            Cues = new List<CueExport>();

            foreach (var cue in cues)
            {
                Cues.Add(new CueExport(cue));
            }

            Folders = new List<FolderExport>();

            foreach (var fold in folders)
            {
                Folders.Add(new FolderExport(fold));
            }
        }
    }
}