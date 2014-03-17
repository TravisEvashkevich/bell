﻿using AudioTool.Core;
using AudioTool.Data;

namespace AudioTool.ViewModel
{
    public class DocumentViewVM : MainViewModel
    {
        private Document _document;

        public Document Document
        {
            get { return _document; }
            set { Set(ref _document, value); }
        }

        protected override void InitializeCommands()
        {
            
        }
    }
}
