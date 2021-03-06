﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AudioTool.Core;
using AudioTool.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;

namespace AudioTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;

            Messenger.Default.Register<CloseMainWindowMessage>(this, p => Close());

            Icon = new BitmapImage(new Uri("icon@2x.png", UriKind.Relative));

            TreeView.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));

            //-----------HOTKEYS------------//
            //Get the MainWindowViewModel as it has all the menu related commands
            var instance = ServiceLocator.Current.GetInstance<MainWindowVM>();
            InputBindings.Add(new KeyBinding(instance.OpenDocumentCommand, new KeyGesture(Key.O, ModifierKeys.Control)));
            InputBindings.Add(new KeyBinding(instance.NewDocumentCommand, new KeyGesture(Key.N, ModifierKeys.Control)));
            InputBindings.Add(new KeyBinding(instance.SaveDocumentCommand, new KeyGesture(Key.S, ModifierKeys.Control)));
            InputBindings.Add(new KeyBinding(instance.SaveAsCommand, new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)));
            InputBindings.Add(new KeyBinding(instance.CloseCommand, new KeyGesture(Key.Q, ModifierKeys.Control)));
            InputBindings.Add(new KeyBinding(instance.ExportCommand, new KeyGesture(Key.E, ModifierKeys.Control)));
            InputBindings.Add(new KeyBinding(instance.RemoveCommand, new KeyGesture(Key.Delete)));
            InputBindings.Add(new KeyBinding(instance.ReImportSelectedSoundCommand, new KeyGesture(Key.R, ModifierKeys.Control)));
            InputBindings.Add(new KeyBinding(instance.ReImportFromNewPathCommand, new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift)));
            InputBindings.Add(new KeyBinding(instance.ReimportCueCommand, new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift)));
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!Glue.Instance.DocumentIsSaved)
            {
                if (MessageBox.Show("You have Un-Saved Changes. Would you like to save them now?", "Un-Saved Changes",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //If they want to save then we save else whatever.
                    Glue.Instance.Document.Save(false);
                }
            }

        }
    }
}
