using System.Windows.Input;

namespace AudioTool.Core
{
    public interface ISmartCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}