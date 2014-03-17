using AudioTool.Core;
using AudioTool.Data;

namespace AudioTool.ViewModel
{
    public class SoundViewVM : MainViewModel
    {
        #region Sound	

        private Sound _sound;

        public Sound Sound
        {
            get { return _sound; }
            set
            {
                Set(ref _sound, value);
            }
        }

        #endregion

        protected override void InitializeCommands()
        {
        }
    }
}