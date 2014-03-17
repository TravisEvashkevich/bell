using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AudioTool.Core;
using AudioTool.Data;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Audio;

namespace AudioTool.Converters
{
    public class DocumentHeaderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            string header = "";

            if (values[0] is string)
            {
                string name = values[0] as string;

                int foldersCount = (int)values[1];

                header = name + Environment.NewLine
                    + "Folders: " + foldersCount;
            }

            return header;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CollapseIfTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return (bool?)value == true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }

    public class CueChildrenCountToHeader : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is int)
            {
                return (int) value > 1 ? "Play parallel" : "Play";
            }

            return "FATAL ERROR";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }

    public class CueChildrenCountToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is int)
            {
                return (int)value > 1 ? Visibility.Visible : Visibility.Collapsed;
            }

            return "FATAL ERROR";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }

    public class IsOpenToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is bool)
            {
                var result = (bool) value;
                if (result)
                {
                    return "Mark as closed";
                }
                else
                {
                    return "Mark as open";
                }
            }

            return "FATAL ERROR ;c";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }

    public class IsDocumentSavedToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is bool)
            {
                var glue = ServiceLocator.Current.GetInstance<Glue>();
                if (glue.Document != null)
                {
                    if (glue.DocumentIsSaved)
                    {
                        return "Sprite Utility [" + glue.Document.Filename + "]";
                    }
                    else
                    {
                        return "Sprite Utility [" + glue.Document.Filename + "*]";
                    }
                }
                else
                {
                    return "Sprite Utility";
                }
            }

            return "FATAL ERROR ;c";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowIfTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return (bool?)value == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }

    public class SoundStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is SoundState && parameter is string)
            {
                var state = (SoundState) value;
                var button = parameter as string;

                if (button == "Play")
                {
                    if (state != SoundState.Playing)
                        return Visibility.Visible;
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
                else if (button == "Stop")
                {
                    if (state != SoundState.Stopped)
                        return Visibility.Visible;
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
                else if (button == "Pause")
                {
                    if (state == SoundState.Playing)
                        return Visibility.Visible;
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }

    public class OrderNodesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is ObservableCollection<INode>)
            {
                var collection = value as ObservableCollection<INode>;

                AutoRefreshCollectionViewSource view = new AutoRefreshCollectionViewSource();
                view.Source = collection;
                SortDescription sort = new SortDescription("Name", ListSortDirection.Ascending);
                view.SortDescriptions.Add(sort);

                return view.View;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToBrushConterver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is Color)
            {
                return new SolidColorBrush((Color)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
