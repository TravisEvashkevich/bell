using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AudioTool.Core;
using Newtonsoft.Json;

namespace AudioTool.Data
{
    public sealed class Folder : NodeWithName
    {
        private ObservableCollection<Folder> _folders;

        [JsonProperty("folders")]
        public ObservableCollection<Folder> Folders
        {
            get { return _folders; }
            set
            {
                Set(ref _folders, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        private ObservableCollection<Cue> _cues;

        [JsonProperty("cues")]
        public ObservableCollection<Cue> Cues
        {
            get { return _cues; }
            set
            {
                Set(ref _cues, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        [JsonIgnore]
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

        public Folder()
        {
            Folders = new ObservableCollection<Folder>();
            Cues = new ObservableCollection<Cue>();
            Children = new ObservableCollection<INode>();
            Children.CollectionChanged += ChildrenOnCollectionChanged;
            Name = "New Folder";
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in notifyCollectionChangedEventArgs.NewItems)
                {
                    if (item is Cue)
                    {
                        Cues.Add(item as Cue);
                    }
                    if (item is Folder)
                    {
                        Folders.Add(item as Folder);
                    }
                }
            }
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in notifyCollectionChangedEventArgs.OldItems)
                {
                    if (item is Cue)
                    {
                        Cues.Remove(item as Cue);
                    }
                    if (item is Folder)
                    {
                        Folders.Remove(item as Folder);
                    }
                }
            }
        }

        [JsonConstructor]
        public Folder(ObservableCollection<Folder> folders, ObservableCollection<Cue> cues)
            : this()
        {
            foreach (var folder in folders)
            {
                AddChild(folder);
            }
            foreach (var cue in cues)
            {
                AddChild(cue);
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

        [JsonIgnore]
        public SmartCommand<object> NewCueCommand { get; private set; }

        public bool CanExecuteNewCueCommand(object o)
        {
            return true;
        }

        public void ExecuteNewCueCommand(object o)
        {
            var cue = new Cue();
            AddChild(cue);
           
        }


        protected override void InitializeCommands()
        {
            NewFolderCommand = new SmartCommand<object>(ExecuteNewFolderCommand, CanExecuteNewFolderCommand);
            NewCueCommand = new SmartCommand<object>(ExecuteNewCueCommand, CanExecuteNewCueCommand);
            base.InitializeCommands();
        }

    }
}
