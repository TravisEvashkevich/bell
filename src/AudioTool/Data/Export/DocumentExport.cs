using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AudioTool.Data.Export
{
    [Serializable]
    public class DocumentExport
    {
        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("folders")]
        public List<FolderExport> Folders { get; set; }

        public DocumentExport(INode document)
        {
            ProjectName = document.Name;
            Folders = new List<FolderExport>();
            foreach (var folder in document.Children.Cast<Folder>())
            {
                Folders.Add(new FolderExport(folder));
            }
        }
    }
}