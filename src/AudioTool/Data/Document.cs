using System.Collections.ObjectModel;
using System.IO;
using AudioTool.Core;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace AudioTool.Data
{
    public sealed class Document : NodeWithName
    {
        public void Save(bool forceNewName)
        {
            if (forceNewName || Filename == "Not Saved")
            {
                SaveFileDialog Dialog = new SaveFileDialog();
                Dialog.Filter = "Audio Tool File (*.auf)|*.auf";
                var result = Dialog.ShowDialog();
                if (result.Value)
                {
                    Filename = Dialog.FileName;
                }
                else
                {
                    return;
                }
            }

            var json = Core.JsonSerializer.Serialize(this);
            File.WriteAllText(Filename, json);

            Glue.DocumentIsSaved = true;
        }

        public static Document Open()
        {
            string fileName;

            var dialog = new OpenFileDialog();
            dialog.Filter = "Audio Tool File (*.auf)|*.auf";
            
            var result = dialog.ShowDialog();
            if (result.Value)
                fileName = dialog.FileName;
            else
                return null;

            var json = File.ReadAllText(fileName);
            var deserialized = Core.JsonSerializer.Deserialize<Document>(json);

            deserialized.Filename = fileName;
           // SetParentRecursivly(deserialized);

            return deserialized;
        }

        public static void SetParentRecursivly(INode node)
        {
            foreach (var child in node.Children)
            {
                child.Parent = node;
                SetParentRecursivly(child);
            }
        }

        [JsonProperty("folders")]
        public override ObservableCollection<INode> Children
        {
            get
            {
                return _children;
            }
            set
            {
                Set(ref _children, value);
                Glue.DocumentIsSaved = false;
            }
        }

        private string _filename;

        public string Filename
        {
            get { return _filename; }
            set
            {
                Set(ref _filename, value);
                Glue.DocumentIsSaved = false;
            }
        }

        public Document()
        {
            Name = "New Document";
            Filename = "Not Saved";
            Children = new ObservableCollection<INode>();
        }

         [JsonConstructor]
        public Document(ObservableCollection<Folder> folders)
            : this()
        {
            foreach (var folder in folders)
            {
                AddChild(folder);
            }
        }

        [JsonIgnore]
        public SmartCommand<object> NewFolderCommand { get; private set; }

        public bool CanExecuteNewFolderCommand(object o)
        {
            return true;
        }

        public void ExecuteNewFolderCommand(object o)
        {
            var folder = new Folder();
            AddChild(folder);
        }

        protected override void InitializeCommands()
        {
            NewFolderCommand = new SmartCommand<object>(ExecuteNewFolderCommand, CanExecuteNewFolderCommand);
            base.InitializeCommands();
        }
    }
}
