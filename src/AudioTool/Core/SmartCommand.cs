using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using CommandManager = AudioTool.Core.CommandManager;

namespace AudioTool.Core
{
    public interface ISmartCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }

    public class SmartCommand<T> : RelayCommand<T>, ISmartCommand, IDisposable
    {
        public SmartCommand(Action<T> executeMethod)
            : base(executeMethod)
        {
            CommandManager.Register(this);
        }

        public SmartCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
            if (canExecuteMethod != null)
                CommandManager.Register(this);
        }

        public void Dispose()
        {
            CommandManager.Unregister(this);
        }
    }
}
