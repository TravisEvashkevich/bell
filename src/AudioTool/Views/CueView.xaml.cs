using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AudioTool.Data;
using AudioTool.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace AudioTool.Views
{
    /// <summary>
    /// Interaction logic for DocumentWindow.xaml
    /// </summary>
    public partial class CueView : UserControl
    {

        private CueViewVM _instance = ServiceLocator.Current.GetInstance<CueViewVM>();
        public CueView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var instance = ServiceLocator.Current.GetInstance<CueViewVM>();
                double x,y = 0;
                Double.TryParse(CenterXTextBox.Text, out x);
                Double.TryParse(CenterYTextBox.Text, out y);
                instance.Cue.DefinedCenter = new Point(x, y);
            }
        }

        private void CenterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(CenterXTextBox == null || CenterYTextBox == null)
                return;
            if(CenterXTextBox.Text != null && CenterYTextBox.Text != null)
            {
                double x, y = 0;
                Double.TryParse(CenterXTextBox.Text, out x);
                Double.TryParse(CenterYTextBox.Text, out y);
                _instance.Cue.DefinedCenter = new Point(x, y);
            }
        }
    }
}
