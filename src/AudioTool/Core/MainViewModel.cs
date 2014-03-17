using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight;

namespace AudioTool.Core
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public abstract class MainViewModel : ViewModelBase
    {
        protected MainViewModel()
        {
            InitializeCommands();
        }

        public virtual void Set<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            Set<T>(name, ref field, value, true);
            CommandManager.InvalidateRequerySuggested();
        }

        protected abstract void InitializeCommands();
    }
}