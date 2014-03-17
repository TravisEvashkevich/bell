using System.Windows;
using AudioTool.Core;

namespace AudioTool.ViewModel
{
    public class PreferencesVM : MainViewModel
    {

        public SmartCommand<object> SaveCommand { get; private set; }

        public bool CanExecuteSaveCommand(object o)
        {
            return true;
        }

        public void ExecuteSaveCommand(object o)
        {
            Properties.Settings.Default.Save();
            MessageBox.Show("Settings has been saved!");
        }

        protected override void InitializeCommands()
        {
            SaveCommand = new SmartCommand<object>(ExecuteSaveCommand, CanExecuteSaveCommand);
        }
    }
}
