/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Boxer"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using AudioTool.ViewModel;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace AudioTool.Core
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainWindowVM>();
            SimpleIoc.Default.Register<PreferencesVM>();
            SimpleIoc.Default.Register<DocumentViewVM>();
            SimpleIoc.Default.Register<FolderViewVM>();
            SimpleIoc.Default.Register<SoundViewVM>();
            SimpleIoc.Default.Register<CueViewVM>();
            SimpleIoc.Default.Register<Glue>();
        }


        public MainWindowVM MainWindow
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainWindowVM>();
            }
        }

        public PreferencesVM Preferences
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PreferencesVM>();
            }
        }

        public DocumentViewVM DocumentView
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DocumentViewVM>();
            }
        }

        public FolderViewVM FolderView
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FolderViewVM>();
            }
        }

        public SoundViewVM SoundView
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SoundViewVM>();
            }
        }

        public CueViewVM CueView
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CueViewVM>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}