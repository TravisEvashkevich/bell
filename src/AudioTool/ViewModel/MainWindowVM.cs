using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using AudioTool.Core;
using AudioTool.Data;
using AudioTool.Data.Export;
using AudioTool.Properties;
using AudioTool.Views;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace AudioTool.ViewModel
{

    public class CloseMainWindowMessage
    {
        
    }
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
    public class MainWindowVM : MainViewModel
    {
        public Glue Glue
        {
            get { return ServiceLocator.Current.GetInstance<Glue>(); }
        }

        private ViewModelLocator _viewModelLocator;

        private MainViewModel _currentView;

        public MainViewModel CurrentView
        {
            get { return _currentView; }
            set { Set(ref _currentView, value); }
        }

        private ObservableCollection<Document> _documents;

        public ObservableCollection<Document> Documents
        {
            get { return _documents; }
            set { Set(ref _documents, value); }
        }

        private bool _isDocumentViewVisible;

        public bool IsDocumentViewVisible
        {
            get { return _isDocumentViewVisible; }
            set { Set(ref _isDocumentViewVisible, value); }
        }

        private bool _isImageViewerViewVisible;
        private NodeWithName _currentSelectedNode;

        public bool IsImageViewerViewVisible
        {
            get { return _isImageViewerViewVisible; }
            set { Set(ref _isImageViewerViewVisible, value); }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainWindowVM()
        {
            Documents = new ObservableCollection<Document>();

            IsDocumentViewVisible = true;

            _viewModelLocator = new ViewModelLocator();

            if (IsInDesignMode)
            {
                CurrentView = _viewModelLocator.DocumentView;
            }
        }

        #region Commands

        #region Selected Item Changed Command
        public SmartCommand<object> SelectedItemChangedCommand { get; private set; }

        public bool CanExecuteSelectedItemChangedCommand(object o)
        {
            return true;
        }

        public void ExecuteSelectedItemChangedCommand(object o)
        {
            var e = o as RoutedPropertyChangedEventArgs<object>;

            if (e.NewValue is Document)
            {
                _currentSelectedNode = _viewModelLocator.DocumentView.Document = e.NewValue as Document;
                CurrentView = _viewModelLocator.DocumentView;
            }
            else if (e.NewValue is Folder)
            {
                _currentSelectedNode = _viewModelLocator.FolderView.Folder = e.NewValue as Folder;
                CurrentView = _viewModelLocator.FolderView;
            }
            else if (e.NewValue is Cue)
            {
                _currentSelectedNode = _viewModelLocator.CueView.Cue = e.NewValue as Cue;
                CurrentView = _viewModelLocator.CueView;
            }
            else if (e.NewValue is Sound)
            {
                _currentSelectedNode = _viewModelLocator.SoundView.Sound = e.NewValue as Sound;
                CurrentView = _viewModelLocator.SoundView;
            }
            else
            {
                CurrentView = null;
            }
        }
        #endregion

        #region New Document Command
        public SmartCommand<object> NewDocumentCommand { get; private set; }

        public bool CanExecuteNewDocumentCommand(object o)
        {
            return true;
        }

        public void ExecuteNewDocumentCommand(object o)
        {
            if (Glue.Instance.Document != null && !Glue.Instance.DocumentIsSaved)
            {
                var result = MessageBox.Show("Would you like to save current document before creating another one?", "Save file", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Glue.Instance.Document.Save(false);
                }
            }
            Glue.Instance.Document = new Document();
            Glue.Instance.DocumentIsSaved = true;
            Glue.Instance.DocumentIsSaved = false;
            Documents.Clear();
            Documents.Add(Glue.Instance.Document);
        }
        #endregion

        #region Open Document
        public SmartCommand<object> OpenDocumentCommand { get; private set; }

        public bool CanExecuteOpenDocumentCommand(object o)
        {
            return true;
        }

        public void ExecuteOpenDocumentCommand(object o)
        {
            if (Glue.Instance.Document != null && !Glue.Instance.DocumentIsSaved)
            {
                var result = MessageBox.Show("Would you like to save current document before opening another one?", "Save file", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Glue.Instance.Document.Save(false);
                }
            }
            Glue.Instance.Document = Document.Open();
            Glue.Instance.DocumentIsSaved = true;
            Documents.Clear();
            Documents.Add(Glue.Instance.Document);
        }
        #endregion

        #region Save Document Command
        public SmartCommand<object> SaveDocumentCommand { get; private set; }

        public bool CanExecuteSaveDocumentCommand(object o)
        {
            return Glue.Instance.Document != null && !Glue.Instance.DocumentIsSaved;
        }

        public void ExecuteSaveDocumentCommand(object o)
        {
            Glue.Instance.Document.Save(false);
        }
        #endregion

        #region Open Preferences Window Command
        public SmartCommand<object> OpenPreferencesWindowCommand { get; private set; }

        public bool CanExecuteOpenPreferencesWindowCommand(object o)
        {
            return true;
        }

        public void ExecuteOpenPreferencesWindowCommand(object o)
        {
            CurrentView = _viewModelLocator.Preferences;
        }
        #endregion

        #region Save As Command
        public SmartCommand<object> SaveAsCommand { get; private set; }

        public bool CanExecuteSaveAsCommand(object o)
        {
            return Glue.Instance.Document != null;
        }

        public void ExecuteSaveAsCommand(object o)
        {
            Glue.Instance.Document.Save(true);
        }
        #endregion

        #region Close Command
        public SmartCommand<object> CloseCommand { get; private set; }

        public bool CanExecuteCloseCommand(object o)
        {
            return true;
        }

        public void ExecuteCloseCommand(object o)
        {
            Messenger.Default.Send(new CloseMainWindowMessage());
        }
        #endregion

        #region Export Command
        public SmartCommand<object> ExportCommand { get; private set; }

        public bool CanExecuteExportCommand(object o)
        {
            return Glue.Instance.Document != null;
        }

        public void ExecuteExportCommand(object o)
        {
            var dialog = new SaveFileDialog { Filter = "JSON (*.json)|*.json" };
            if (dialog.ShowDialog().Value)
            {
                var export = new DocumentExport(Glue.Instance.Document);
                var json = JsonConvert.SerializeObject(export, new JsonSerializerSettings()
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Formatting = Formatting.Indented,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                File.WriteAllText(dialog.FileName, json);
                MessageBox.Show("Json data has been exported!");
            }
        }
        #endregion

        #region ClosingCommand

        public SmartCommand<object> ClosingCommand { get; private set; }

        public bool CanExecuteClosingCommand(object o)
        {
            return true;
        }

        public async void ExecuteClosingCommand(object o)
        {
            if (Glue.Instance.Document != null && !Glue.Instance.DocumentIsSaved)
            {
                var result = MessageBox.Show("Would you like to save current document before close application?", "Save file", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Glue.Instance.Document.Save(false);
                }
            }
        }
        #endregion

        #region Remove Command

        public SmartCommand<object> RemoveCommand { get; private set; }

        private bool CanExecuteRemoveCommand(object arg)
        {
            return _currentSelectedNode != null;
        }
        private void ExecuteRemoveCommand(object obj)
        {
            _currentSelectedNode.Remove();
        }
        #endregion

        #region ReImportSounds Command

        public SmartCommand<object> ReImportSoundCommand { get; private set; }

        public void ExecuteReImportSoundCommand(object obj)
        {
            //Call stuff on Documents
        }

        #endregion

        protected override void InitializeCommands()
        {
            ClosingCommand = new SmartCommand<object>(ExecuteClosingCommand, CanExecuteClosingCommand);  
            NewDocumentCommand = new SmartCommand<object>(ExecuteNewDocumentCommand, CanExecuteNewDocumentCommand);
            SaveDocumentCommand = new SmartCommand<object>(ExecuteSaveDocumentCommand, CanExecuteSaveDocumentCommand);
            OpenDocumentCommand = new SmartCommand<object>(ExecuteOpenDocumentCommand, CanExecuteOpenDocumentCommand);
            SelectedItemChangedCommand = new SmartCommand<object>(ExecuteSelectedItemChangedCommand, CanExecuteSelectedItemChangedCommand);
            OpenPreferencesWindowCommand = new SmartCommand<object>(ExecuteOpenPreferencesWindowCommand, CanExecuteOpenPreferencesWindowCommand);
            SaveAsCommand = new SmartCommand<object>(ExecuteSaveAsCommand, CanExecuteSaveAsCommand);
            CloseCommand = new SmartCommand<object>(ExecuteCloseCommand, CanExecuteCloseCommand);
            ExportCommand = new SmartCommand<object>(ExecuteExportCommand, CanExecuteExportCommand);
            RemoveCommand = new SmartCommand<object>(ExecuteRemoveCommand, CanExecuteRemoveCommand);
            ReImportSoundCommand = new SmartCommand<object>(ExecuteReImportSoundCommand);
        }
        #endregion

    }
}