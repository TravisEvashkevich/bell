using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AudioTool.Core;
using AudioTool.Data;

namespace AudioTool.ViewModel
{
    public class CueViewVM : MainViewModel
    {
        private Cue _cue;

        public Cue Cue
        {
            get { return _cue; }
            set
            {
                Set(ref _cue, value);
            }
        }

        #region CueModes	

        private ObservableCollection<CuePlaybackMode> _cueModes;

        public ObservableCollection<CuePlaybackMode> CueModes
        {
            get { return _cueModes; }
            set
            {
                Set(ref _cueModes, value);
            }
        }

        #endregion

        public CueViewVM()
        {
            CueModes = new ObservableCollection<CuePlaybackMode>((CuePlaybackMode[])Enum.GetValues(typeof(CuePlaybackMode)));
        }

        protected override void InitializeCommands()
        {
            
        }
    }
}
