using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using AudioTool.Core;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;

namespace AudioTool.Data
{
    public interface INode : IName
    {
        ObservableCollection<INode> Children { get; set; }
        INode Parent { get; set; }
        void Initialize();
    }

    public abstract class NodeWithName : MainViewModel, INode
    {
        protected string _name;

        
        private bool _isVisible = true;
        [JsonIgnore]
        public virtual bool IsVisible { get { return _isVisible; } set { Set(ref _isVisible, value); } }

        private bool _expanded;
        [JsonIgnore]
        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                Set(ref _expanded, value);
            }
        }

        private bool _approved;
        [JsonIgnore]
        public bool Approved
        {
            get { return _approved; }
            set { Set(ref _approved, value); }
        }

        [JsonProperty("name")]
        public virtual string Name
        {
            get { return _name; }
            set
            {
                Set(ref _name, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        protected override void InitializeCommands()
        {
            RemoveCommand = new SmartCommand<object>(ExecuteRemoveCommand, CanExecuteRemoveCommand);
        }

        protected ObservableCollection<INode> _children;

        public virtual ObservableCollection<INode> Children
        {
            get { return _children; }
            set
            {
                Set(ref _children, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        public INode Parent { get; set; }
        public void Initialize()
        {
            InitializeCommands();
        }

        public virtual void Remove()
        {
            if (Parent != null)
            {
                Parent.Children.Remove(this);
                Glue.Instance.DocumentIsSaved = false;
            }

        }

        public void AddChild(INode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        [JsonIgnore]
        public SmartCommand<object> RemoveCommand { get; private set; }

        public bool CanExecuteRemoveCommand(object o)
        {
            return true;
        }

        public void ExecuteRemoveCommand(object o)
        {
            Remove();
        }

    }

}
