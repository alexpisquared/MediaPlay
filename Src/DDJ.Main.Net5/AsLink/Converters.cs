using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AAV.Common
{
    //[ValueConversion(typeof(TimeSpan), typeof(double))]
	public class TimeSpanToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((TimeSpan)value).TotalSeconds;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return 0;
		}
	}

    //[ValueConversion(typeof(bool), typeof(Brush))]
	public class BoolToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new SolidColorBrush(((bool)value) ? Colors.Red : Colors.Green);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return 0;
		}
	}

    //[ValueConversion(typeof(bool), typeof(double))]
	public class BoolToOpacityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return (value is bool && ((bool)value)) ? 1 : 0.3;
			}
			catch
			{
				return 1;
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class BoolToVisbilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return (value is bool && ((bool)value)) ? Visibility.Visible : Visibility.Collapsed;
			}
			catch
			{
				return 1;
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class BoolToVisbilityReverseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return (value is bool && ((bool)value)) ? Visibility.Collapsed : Visibility.Visible;
			}
			catch
			{
				return 1;
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
