using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = false;
            
            if (value is bool b)
                boolValue = b;
            else if (value is string s)
                boolValue = !string.IsNullOrEmpty(s);
            else if (value != null)
                boolValue = true;

            if (parameter?.ToString() == "Inverse")
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViewVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentView && parameter is string targetView)
            {
                return currentView == targetView ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrEmpty(s))
                return s[0].ToString().ToUpper();
            return "U";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ResultBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value?.ToString() ?? "";
            return result switch
            {
                "Pass" => new SolidColorBrush(Color.FromRgb(220, 252, 231)), // #DCFCE7
                "Warning" => new SolidColorBrush(Color.FromRgb(254, 243, 199)), // #FEF3C7
                "Fail" => new SolidColorBrush(Color.FromRgb(254, 226, 226)), // #FEE2E2
                _ => new SolidColorBrush(Color.FromRgb(241, 245, 249)) // #F1F5F9
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ResultForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value?.ToString() ?? "";
            return result switch
            {
                "Pass" => new SolidColorBrush(Color.FromRgb(21, 128, 61)), // #15803D
                "Warning" => new SolidColorBrush(Color.FromRgb(180, 83, 9)), // #B45309
                "Fail" => new SolidColorBrush(Color.FromRgb(220, 38, 38)), // #DC2626
                _ => new SolidColorBrush(Color.FromRgb(100, 116, 139)) // #64748B
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DeviceStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceConnectionState state)
            {
                return state switch
                {
                    DeviceConnectionState.Connected or 
                    DeviceConnectionState.ImportComplete => new SolidColorBrush(Color.FromRgb(220, 252, 231)),
                    DeviceConnectionState.Detecting or 
                    DeviceConnectionState.Connecting or 
                    DeviceConnectionState.ReadingData or 
                    DeviceConnectionState.Importing => new SolidColorBrush(Color.FromRgb(219, 234, 254)),
                    DeviceConnectionState.Found => new SolidColorBrush(Color.FromRgb(254, 243, 199)),
                    DeviceConnectionState.Error => new SolidColorBrush(Color.FromRgb(254, 226, 226)),
                    _ => new SolidColorBrush(Color.FromRgb(241, 245, 249))
                };
            }
            return new SolidColorBrush(Color.FromRgb(241, 245, 249));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DeviceStateDotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceConnectionState state)
            {
                return state switch
                {
                    DeviceConnectionState.Connected or 
                    DeviceConnectionState.ImportComplete => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                    DeviceConnectionState.Detecting or 
                    DeviceConnectionState.Connecting or 
                    DeviceConnectionState.ReadingData or 
                    DeviceConnectionState.Importing => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                    DeviceConnectionState.Found => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                    DeviceConnectionState.Error => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                    _ => new SolidColorBrush(Color.FromRgb(148, 163, 184))
                };
            }
            return new SolidColorBrush(Color.FromRgb(148, 163, 184));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DeviceStateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceConnectionState state)
            {
                return state switch
                {
                    DeviceConnectionState.Disconnected => "No Device",
                    DeviceConnectionState.Detecting => "Searching...",
                    DeviceConnectionState.Found => "Device Found",
                    DeviceConnectionState.Connecting => "Connecting...",
                    DeviceConnectionState.Connected => "Connected",
                    DeviceConnectionState.ReadingData => "Reading Data...",
                    DeviceConnectionState.Importing => "Importing...",
                    DeviceConnectionState.ImportComplete => "Import Complete",
                    DeviceConnectionState.Error => "Error",
                    _ => "Unknown"
                };
            }
            return "No Device";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DeviceIconBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceConnectionState state)
            {
                return state switch
                {
                    DeviceConnectionState.Connected or DeviceConnectionState.ImportComplete => 
                        new SolidColorBrush(Color.FromRgb(220, 252, 231)),
                    DeviceConnectionState.Found => 
                        new SolidColorBrush(Color.FromRgb(254, 243, 199)),
                    DeviceConnectionState.Error => 
                        new SolidColorBrush(Color.FromRgb(254, 226, 226)),
                    _ => new SolidColorBrush(Color.FromRgb(241, 245, 249))
                };
            }
            return new SolidColorBrush(Color.FromRgb(241, 245, 249));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProgressVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int progress)
            {
                return progress > 0 && progress < 100 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProgressWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int progress)
            {
                // Return a proportional width based on progress percentage
                return progress * 3.5; // Approximate width scaling
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StepBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceConnectionState state && parameter is string stepStr && int.TryParse(stepStr, out int step))
            {
                int currentStep = GetCurrentStep(state);
                
                if (currentStep >= step)
                    return new SolidColorBrush(Color.FromRgb(37, 99, 235)); // Primary blue
                else
                    return new SolidColorBrush(Color.FromRgb(226, 232, 240)); // Gray
            }
            return new SolidColorBrush(Color.FromRgb(226, 232, 240));
        }

        private int GetCurrentStep(DeviceConnectionState state)
        {
            return state switch
            {
                DeviceConnectionState.Found => 1,
                DeviceConnectionState.Connecting or DeviceConnectionState.Connected => 2,
                DeviceConnectionState.ReadingData or DeviceConnectionState.Importing => 3,
                DeviceConnectionState.ImportComplete => 4,
                _ => 0
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StepForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceConnectionState state && parameter is string stepStr && int.TryParse(stepStr, out int step))
            {
                int currentStep = GetCurrentStep(state);
                
                if (currentStep >= step)
                    return new SolidColorBrush(Colors.White);
                else
                    return new SolidColorBrush(Color.FromRgb(100, 116, 139)); // Gray text
            }
            return new SolidColorBrush(Color.FromRgb(100, 116, 139));
        }

        private int GetCurrentStep(DeviceConnectionState state)
        {
            return state switch
            {
                DeviceConnectionState.Found => 1,
                DeviceConnectionState.Connecting or DeviceConnectionState.Connected => 2,
                DeviceConnectionState.ReadingData or DeviceConnectionState.Importing => 3,
                DeviceConnectionState.ImportComplete => 4,
                _ => 0
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyStateVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
