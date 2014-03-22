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
                _viewModelLocator.DocumentView.Document = e.NewValue as Document;
                CurrentView = _viewModelLocator.DocumentView;
            }
            else if (e.NewValue is Folder)
            {
                _viewModelLocator.FolderView.Folder = e.NewValue as Folder;
                CurrentView = _viewModelLocator.FolderView;
            }
            else if (e.NewValue is Cue)
            {
                _viewModelLocator.CueView.Cue = e.NewValue as Cue;
                CurrentView = _viewModelLocator.CueView;
            }
            else if (e.NewValue is Sound)
            {
                _viewModelLocator.SoundView.Sound = e.NewValue as Sound;
                CurrentView = _viewModelLocator.SoundView;
            }
            else
            {
                CurrentView = null;
            }
        }

        public SmartCommand<object> NewDocumentCommand { get; private set; }

        public bool CanExecuteNewDocumentCommand(object o)
        {
            return true;
        }

        public void ExecuteNewDocumentCommand(object o)
        {
            if (Glue.Document != null && !Glue.DocumentIsSaved)
            {
                var result = MessageBox.Show("Would you like to save current document before creating another one?", "Save file", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Glue.Document.Save(false);
                }
            }
            Glue.Document = new Document();
            Glue.DocumentIsSaved = true;
            Glue.DocumentIsSaved = false;
            Documents.Clear();
            Documents.Add(Glue.Document);
        }

        public SmartCommand<object> OpenDocumentCommand { get; private set; }

        public bool CanExecuteOpenDocumentCommand(object o)
        {
            return true;
        }

        public void ExecuteOpenDocumentCommand(object o)
        {
            if (Glue.Document != null && !Glue.DocumentIsSaved)
            {
                var result = MessageBox.Show("Would you like to save current document before opening another one?", "Save file", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Glue.Document.Save(false);
                }
            }
            Glue.Document = Document.Open();
            Glue.DocumentIsSaved = true;
            Documents.Clear();
            Documents.Add(Glue.Document);
        }

        public SmartCommand<object> SaveDocumentCommand { get; private set; }

        public bool CanExecuteSaveDocumentCommand(object o)
        {
            return Glue.Document != null && !Glue.DocumentIsSaved;
        }

        public void ExecuteSaveDocumentCommand(object o)
        {
            Glue.Document.Save(false);
        }

        public SmartCommand<object> OpenPreferencesWindowCommand { get; private set; }

        public bool CanExecuteOpenPreferencesWindowCommand(object o)
        {
            return true;
        }

        public void ExecuteOpenPreferencesWindowCommand(object o)
        {
            CurrentView = _viewModelLocator.Preferences;
        }

        public SmartCommand<object> SaveAsCommand { get; private set; }

        public bool CanExecuteSaveAsCommand(object o)
        {
            return Glue.Document != null;
        }

        public void ExecuteSaveAsCommand(object o)
        {
            Glue.Document.Save(true);
        }

        public SmartCommand<object> CloseCommand { get; private set; }

        public bool CanExecuteCloseCommand(object o)
        {
            return true;
        }

        public void ExecuteCloseCommand(object o)
        {
            Messenger.Default.Send(new CloseMainWindowMessage());
        }

        public SmartCommand<object> ExportCommand { get; private set; }

        public bool CanExecuteExportCommand(object o)
        {
            return Glue.Document != null;
        }

        public void ExecuteExportCommand(object o)
        {
            var dialog = new SaveFileDialog { Filter = "JSON (*.json)|*.json" };
            if (dialog.ShowDialog().Value)
            {
                var export = new DocumentExport(Glue.Document);
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

        #region ClosingCommand

        public SmartCommand<object> ClosingCommand { get; private set; }

        public bool CanExecuteClosingCommand(object o)
        {
            return true;
        }

        public async void ExecuteClosingCommand(object o)
        {
            if (Glue.Document != null && !Glue.DocumentIsSaved)
            {
                var result = MessageBox.Show("Would you like to save current document before close application?", "Save file", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Glue.Document.Save(false);
                }
            }
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
        }
    }
}