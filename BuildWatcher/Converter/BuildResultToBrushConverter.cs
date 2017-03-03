using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildWatcher.Converter
{
    public class BuildResultToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is BuildResult))
            {
                return Brushes.Gray;
            }

            var result = (BuildResult) value;

            switch (result)
            {
                case BuildResult.Succeeded:
                    return Brushes.Green;
                case BuildResult.PartiallySucceeded:
                    return Brushes.GreenYellow;
                case BuildResult.Failed:
                    return Brushes.Red;
                case BuildResult.Canceled:
                    return Brushes.Yellow;
                default:
                    return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}