using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using AudioTool.Core;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace AudioTool.Data
{
    public sealed class Document : NodeWithName
    {
        private static readonly FileFormat Format = new BinaryFileFormat();

        public void Save(bool forceNewName)
        {
            if (forceNewName || Filename == "Not Saved")
            {
                var Dialog = new SaveFileDialog();
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

            Format.Save(Filename, this);
                
            Glue.Instance.DocumentIsSaved = true;
        }

        public static Document Open()
        {
            string fileName;

            var dialog = new OpenFileDialog {Filter = "Audio Tool File (*.auf)|*.auf"};

            var result = dialog.ShowDialog();
            if (result.Value)
            {
                fileName = dialog.FileName;
            }
            else
            {
                return null;
            }

            var document = Format.Load(fileName);
            return document;
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
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        private string _filename;

        public string Filename
        {
            get { return _filename; }
            set
            {
                Set(ref _filename, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        public Document()
        {
            Name = "New Document";
            Filename = "Not Saved";
            Children = new ObservableCollection<INode>();
        }

         [JsonConstructor]
        public Document(IEnumerable<Folder> folders)
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

        public void Initialize()
        {
            InitializeCommands();
        }
    }
}
