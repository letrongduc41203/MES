using MES.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MES.Helpers
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MachineStatus status)
            {
                return status switch
                {
                    MachineStatus.Idle => new SolidColorBrush(Colors.Gray),
                    MachineStatus.Running => new SolidColorBrush(Colors.Green),
                    MachineStatus.Maintenance => new SolidColorBrush(Colors.Orange),
                    MachineStatus.Error => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 