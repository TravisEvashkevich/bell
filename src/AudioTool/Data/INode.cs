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
    }

    public abstract class NodeWithName : MainViewModel, INode
    {
        protected Glue Glue = ServiceLocator.Current.GetInstance<Glue>();

        protected string _name;

        [JsonProperty("name")]
        public virtual string Name
        {
            get { return _name; }
            set
            {
                Set(ref _name, value);
                Glue.DocumentIsSaved = false;
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
                Glue.DocumentIsSaved = false;
            }
        }

        public INode Parent { get; set; }

        public virtual void Remove()
        {
            if (Parent != null)
                Parent.Children.Remove(this);
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
