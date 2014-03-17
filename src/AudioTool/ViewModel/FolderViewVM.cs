using AudioTool.Core;
using AudioTool.Data;

namespace AudioTool.ViewModel
{
    public class FolderViewVM : MainViewModel
    {
        private Folder _folder;

        public Folder Folder
        {
            get { return _folder; }
            set { Set(ref _folder, value); }
        }

        protected override void InitializeCommands()
        {

        }
    }
}
