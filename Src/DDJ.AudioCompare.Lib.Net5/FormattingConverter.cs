using System;
using System.Windows.Data;

//namespace VideoHistoryView
//{
//  class Converter
//  {
//  }
//}


namespace Framework.FormattingConvert
{
    public class FormattingConverter : IValueConverter
	{
		int usage = 0;
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(parameter is string))
				return value;

			try
			{
				usage++;
				if (usage % 8000 == 0)
					System.Diagnostics.Debug.WriteLine(usage);

				if (value is DateTime && ((DateTime)value).CompareTo(new DateTime()) == 0)
					return "";      //The default value of date time should be blank
				else
					return String.Format(culture, (string)parameter, value);
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (targetType.ToString() != "System.DateTime")
				return null;

			try
			{
				string formattingString = (string)parameter;
				formattingString = formattingString.Remove(0, formattingString.IndexOf(":") + 1);
				formattingString = formattingString.Remove(formattingString.IndexOf("}"));
				string[] formats = new string[] { formattingString, 
					"ddMMMyy",
					"ddMMyy",
					"ddMMyyyy",
					"dd-MM-yy",
					"dd-MM-yyyy",
					"dd-MM-yy",
					"dd-MM-yyyy",
					"ddMMMyy hh:mm",
					"ddMMyy hh:mm",
					"ddMMyyyy hh:mm",
					"dd-MM-yy hh:mm",
					"dd-MM-yyyy hh:mm",
					"dd-MM-yy hh:mm",
					"dd-MM-yyyy hh:mm",
					""};

				DateTime date;
				if (DateTime.TryParseExact((string)value, formats, culture, System.Globalization.DateTimeStyles.AssumeLocal, out date))
					return date;
			}
			catch (Exception ex) //AP: 23Nov2009
			{
				System.Diagnostics.Trace.WriteLine(
				    $"{DateTime.Now.ToString("MMM yyyy HH:mm")}  in  {System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}():\n\t{ex.Message}\n{(ex.InnerException == null ? "" : ex.InnerException.Message)}");
			}

			return DateTime.MaxValue;//returning null is not an option.
		}
	}

	public class LoadedBehaviorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(parameter is string))
				return value;

			try
			{
				if (value is int && 0 < (int)value && (int)value < 11)
					return System.Windows.Controls.MediaState.Play;
				//	else if (value is int && (int)value < 10)					return System.Windows.Controls.MediaState.Pause;
				else
					return System.Windows.Controls.MediaState.Manual;
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

}
