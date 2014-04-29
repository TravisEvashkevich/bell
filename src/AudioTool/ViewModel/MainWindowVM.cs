using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            //if you exit an open before actually selecting something, you get null and it actually
            //opens up reimporting without a document since you add a null into the documents
            if(Glue.Instance.Document != null)
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

        #region ReImport from New Location Command

        public SmartCommand<object> ReImportFromNewPathCommand { get; private set; }

        public bool CanExecuteReimportCommand(object arg)
        {
            return Documents.Count != 0 && Documents != null;
        }

        public void ExecuteReImportFromNewPathCommand(object obj)
        {
            if(_currentSelectedNode == null || _currentSelectedNode.GetType() != typeof(Sound))
                return;

            var sound = _currentSelectedNode as Sound;
            //The thing is, the filename could have technically changed or have a different syntax or something
            var dialog = new OpenFileDialog();
            dialog.Filter = ".Wav (*.Wav)|*.Wav";
            
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                //we'll check the filename they selected against the filename in the sound and ask if they want to overwrite
                //if they are different
                //get the path
                string fileName = Path.GetFileNameWithoutExtension(dialog.FileName);

                if (fileName != sound.Name)
                {
                    if (MessageBox.Show(
                                String.Format("The selected file \"{0}\" has a different name than what you are trying to overwrite ({1}). Would you like to proceed anyway?", fileName, sound.Name),
                                "FileName doesn't match", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        //pass the filename from the dialog to the Reimport new version so it will Create and overwrite
                        //the sound thus updating in the program.
                        sound.ExecuteReImport(dialog.FileName);
                    }
                }
            }
        }

        #endregion

        #region ReImportSelectedSound Command

        public SmartCommand<object> ReImportSelectedSoundCommand { get; private set; }

        public void ExecuteReImportSelectedSoundCommand(object obj)
        {
            //Reimport called on the current Selected sound (the one you clicked etc)
            if (_currentSelectedNode is Sound)
            {
                var sound = _currentSelectedNode as Sound;
                sound.ExecuteReImport(null);
            }
        }

        #endregion

        #region Reimport Arbitrary Command

        public SmartCommand<object> ReimportArbitraryCommand { get; private set; }

        public void ExecuteReimportArbitraryCommand(object o)
        {
            if(Documents.Count != 0)
            {
                var dlg = new OpenFileDialog();

                dlg.Filter = ".Wav (*.Wav)|*.Wav";
                dlg.Multiselect = true;
                var result = dlg.ShowDialog();

                if (result == true)
                {
                    string[] names = dlg.FileNames;

                    for (int i = 0;  i < names.Count(); i++)
                    {
                        var name = Path.GetFileNameWithoutExtension(names[i]);
                        FindMatches(name, names[i], new Stack<NodeWithName>(), Documents[0]);
                    }
                }
            }
        }

        public void FindMatches(string criteria, string fullPath, Stack<NodeWithName> ancestors, NodeWithName startPoint)
        {
            if (IsCriteriaMatched(criteria, startPoint))
            {
                (startPoint as Sound).ReimportSoundFile(fullPath);

                MessageBox.Show(String.Format("We reimported {0} successfully.", Path.GetFileNameWithoutExtension(criteria)));
            }

            ancestors.Push(startPoint);
            if (startPoint.Children != null && startPoint.Children.Count > 0)
            {
                foreach (var child in startPoint.Children)
                    FindMatches(criteria, fullPath, ancestors, child as NodeWithName);
            }

            ancestors.Pop();
        }

        private bool IsCriteriaMatched(string criteria, NodeWithName check)
        {
            return String.IsNullOrEmpty(criteria) || check.Name.ToLower().Contains(criteria.ToLower());
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
            ReImportSelectedSoundCommand = new SmartCommand<object>(ExecuteReImportSelectedSoundCommand, CanExecuteReimportCommand);
            ReImportFromNewPathCommand = new SmartCommand<object>(ExecuteReImportFromNewPathCommand, CanExecuteReimportCommand);
            ReimportArbitraryCommand = new SmartCommand<object>(ExecuteReimportArbitraryCommand, CanExecuteReimportCommand);
        }
        #endregion

    }
}