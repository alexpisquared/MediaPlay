using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
//using TimeTracker; ++

//xmlns:mvvm="clr-namespace:SBNET.MVVM;assembly=SBNET.MVVM"
//<TextBlock TextAlignment="Center" Text="{Binding Path=Unlading, Converter={mvvm:Equals EqualsText='Y', NotEqualsText='N'}, ConverterParameter=1}" />

namespace AsLink
{
  public class StringToColor : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      // if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return Brushes.Red;
      try
      {
        return value is string && !string.IsNullOrEmpty((string)value) && !((string)value).Equals("ColorRGB")
          ? new SolidColorBrush((Color)ColorConverter.ConvertFromString((string)value))
          : Brushes.Teal;
      }
      catch
      {
        return Brushes.Red;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public StringToColor() { }
  }

  public class RowDtlVisMode : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool && (bool)value)
        return DataGridRowDetailsVisibilityMode.VisibleWhenSelected;

      return DataGridRowDetailsVisibilityMode.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
  }
  public class AgeBrush : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var d = value is DateTime ? (byte)(((int)(DateTime.Now - (DateTime)value).TotalDays) % 256) : (byte)0;
      var r = (byte)(220 + (255 - d * 30) % 36);
      var g = (byte)(220 + (255 - d * 20) % 36);
      var b = (byte)(220 + (255 - d * 10) % 36);

      if (targetType == typeof(Brush))
      {
        return new SolidColorBrush(Color.FromRgb(r, g, b));
      }
      else if (targetType == typeof(bool))
      {
        return d == 0;
      }
      else
        return null;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public AgeBrush() { }
  }
  public class IsAfterAppStart : MarkupExtension, IValueConverter
  {
    public static DateTime AppStartAt = DateTime.Now;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
#if DEBUG_
      var rv = (value is DateTime && (DateTime)value > AppStartAt.AddHours(-31));
#else
      var rv = (value is DateTime && (DateTime)value > AppStartAt);
#endif

      if (targetType == typeof(Brush))
      {
        return rv ? Brushes.Black : Brushes.Gray;
      }
      else if (targetType == typeof(bool))
      {
        return rv;
      }
      else if (targetType == typeof(FontWeight))
      {
        return rv ? FontWeights.Bold : FontWeights.Normal;
      }
      else
        return null;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public IsAfterAppStart() { }
  }
  public class IsToday : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var rv = (value is DateTime && (DateTime)value >= DateTime.Today);

      if (targetType == typeof(Brush))
      {
        return rv ? Brushes.DarkOrange : Brushes.Gray;
      }
      else if (targetType == typeof(bool))
      {
        return rv;
      }
      else if (targetType == typeof(FontWeight))
      {
        return rv ? FontWeights.Bold : FontWeights.Normal;
      }
      else
        return null;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public IsToday() { }
  }
  public class PastPresentFuture : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is DateTime)) return new LinearGradientBrush(Colors.Gray, Colors.DarkGray, 45);

      if (Math.Abs((((DateTime)value) - DateTime.UtcNow).TotalHours) < 5) return Brushes.Black;
      return ((DateTime)value) > DateTime.UtcNow ? Brushes.Green : Brushes.Blue;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public PastPresentFuture() { }
  }

  public class MultiplierConverter : MarkupExtension, IValueConverter
  {
    public double MultiplyBy { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch (value)
      {
        case int s: return s * MultiplyBy;
        case long s: return s * MultiplyBy;
        case float s: return s * MultiplyBy;
        case double s: return s * MultiplyBy;
        case decimal s: return s * (decimal)MultiplyBy;
        default: return value;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public MultiplierConverter() => MultiplyBy = .001;
  }

  public class WeekDayToForeColorConverter : MarkupExtension, IValueConverter
  {
    public Brush WeekDay { get; set; } = Brushes.DodgerBlue;
    public Brush WeekEnd { get; set; } = Brushes.Red;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is DateTime) && !(value is DateTimeOffset)) return new LinearGradientBrush(Colors.Gray, Colors.DarkGray, 45);

      var val = value is DateTimeOffset ? ((DateTimeOffset)value).DateTime : (DateTime)value;
      switch (val.DayOfWeek)
      {
        case DayOfWeek.Saturday:
        case DayOfWeek.Sunday: return WeekEnd;
        default:          /**/ return WeekDay;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public WeekDayToForeColorConverter() { }
  }
  public class WeekdaysTo6Colors : MarkupExtension, IValueConverter
  {
    public bool IsGrayScale { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is DateTime) && !(value is DateTimeOffset)) return new LinearGradientBrush(Colors.Gray, Colors.DarkGray, 45);

      var val = value is DateTimeOffset ? ((DateTimeOffset)value).DateTime : (DateTime)value;

      var wd = 1 + (val.DayOfWeek == DayOfWeek.Saturday ? -1 : (int)val.DayOfWeek);
      var dd = 4;

      var b = (byte)(255 - dd * (7 - wd));
      var c = (byte)(255 - dd * (6 - wd)); //      var h = (byte)(225 + ((int)val.DayOfWeek % 2 == 0 ? -5 * (int)val.DayOfWeek : +5 * (int)val.DayOfWeek));

      if (IsGrayScale)
        return new LinearGradientBrush(Color.FromRgb(b, b, b), Color.FromRgb(c, c, c), 90);
      else
      {
        switch (val.DayOfWeek)
        {
          default:
          case DayOfWeek.Monday:     /**/ return new SolidColorBrush(Color.FromRgb(200, 200, 255));
          case DayOfWeek.Tuesday:    /**/ return new SolidColorBrush(Color.FromRgb(255, 200, 200));
          case DayOfWeek.Wednesday:  /**/ return new SolidColorBrush(Color.FromRgb(255, 255, 200));
          case DayOfWeek.Thursday:   /**/ return new SolidColorBrush(Color.FromRgb(255, 200, 255));
          case DayOfWeek.Friday:     /**/ return new SolidColorBrush(Color.FromRgb(200, 255, 255));
          case DayOfWeek.Saturday:   /**/ return new SolidColorBrush(Color.FromRgb(255, 220, 255));
          case DayOfWeek.Sunday:     /**/ return new SolidColorBrush(Color.FromRgb(255, 200, 200));
        }
      }

      //switch (val.DayOfWeek)
      //{
      //  case DayOfWeek.Sunday:    /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0x40, 0x40, 0x40), Color.FromArgb(lower, 0x60, 0x60, 0x60), 90); // return new SolidColorBrush(Color.FromRgb(255, 240, 230));
      //  case DayOfWeek.Monday:    /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0x60, 0x60, 0x60), Color.FromArgb(lower, 0x77, 0x77, 0x77), 90);
      //  case DayOfWeek.Tuesday:   /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0x77, 0x77, 0x77), Color.FromArgb(lower, 0x91, 0x91, 0x91), 90);
      //  case DayOfWeek.Wednesday: /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0x91, 0x91, 0x91), Color.FromArgb(lower, 0xAB, 0xAB, 0xAB), 90);
      //  case DayOfWeek.Thursday:  /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0xAB, 0xAB, 0xAB), Color.FromArgb(lower, 0xC4, 0xC4, 0xC4), 90);
      //  case DayOfWeek.Friday:    /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0xC4, 0xC4, 0xC4), Color.FromArgb(lower, 0xE0, 0xE0, 0xE0), 90);
      //  case DayOfWeek.Saturday:  /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0xE0, 0xE0, 0xE0), Color.FromArgb(lower, 0xff, 0xff, 0xff), 90);
      //}

      //switch (val.DayOfWeek)
      //{
      //  case DayOfWeek.Sunday:    /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0xff, 0x00, 0x00), Color.FromArgb(lower, 0xFF, 0xB9, 0x00), 90); // return new SolidColorBrush(Color.FromRgb(255, 240, 230));
      //  case DayOfWeek.Monday:    /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0xFF, 0xB9, 0x00), Color.FromArgb(lower, 0xF3, 0xFF, 0x00), 90);
      //  case DayOfWeek.Tuesday:   /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0xF3, 0xFF, 0x00), Color.FromArgb(lower, 0x00, 0xFF, 0x00), 90);
      //  case DayOfWeek.Wednesday: /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0x00, 0xFF, 0x00), Color.FromArgb(lower, 0x00, 0xff, 0xff), 90);
      //  case DayOfWeek.Thursday:  /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0x00, 0xff, 0xff), Color.FromArgb(lower, 0x00, 0x00, 0xff), 90);
      //  case DayOfWeek.Friday:    /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0x00, 0x00, 0xff), Color.FromArgb(lower, 0xF3, 0x00, 0xFF), 90);
      //  case DayOfWeek.Saturday:  /**/ return new LinearGradientBrush(Color.FromArgb(upper, 0xF3, 0x00, 0xFF), Color.FromArgb(lower, 0xfF, 0x00, 0x00), 90);
      //}
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public WeekdaysTo6Colors() { }
  }
  public class PcNameToForeColorConverter : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string)
      {
        switch (((string)value))
        {
          case "as": return Brushes.Blue;
          case "nu1": case "nu": return Brushes.Orange;
          case "va1": case "va": return Brushes.MediumVioletRed;
          case "su1": return Brushes.Red;
          case "apw": case "apW": case "ap": return Brushes.Teal;
          case "as1": return Brushes.Green;
          case "as2": return Brushes.DodgerBlue;
          case "asl": return Brushes.DeepPink;
          default: return Brushes.Gray;
        }
      }

      return new LinearGradientBrush(Colors.Gray, Colors.DarkGray, 45);//		return new BrushConverter().ConvertFromString("#ff0000");
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public PcNameToForeColorConverter() { }
  }
  public class EvOfIntToColorConverter : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch (value)
      {
        case 1: return Brushes.DodgerBlue;  // ShutAndSleepDn = 1,  // Off pc
        case 2: return Brushes.LightBlue;   // ScreenSaverrUp = 2,  // Off ss
        case 4: return Brushes.LightPink;   // ScreenSaverrDn = 4,  // On  ss
        case 8: return Brushes.Orange;      // BootAndWakeUps = 8,  // On  pc
        case 16: return Brushes.Magenta;    // Day1stAmbiguos = 16, // PC was on whole night ... but nobody's was there
        case 1024: return Brushes.Red;      // Who_Knows_What = 1024,
        default: return Brushes.Gray;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public EvOfIntToColorConverter() { }
  }
  public class EvOfIntToMsgConverter : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch (value)
      {
        case 1: return "Off pc";
        case 2: return "ss UP ▄█";
        case 4: return "SS dn ▀█";
        case 8: return "On  pc";
        case 16: return "Odd";
        case 32: return "Off ignore";
        case 64: return "On  ignore";
        case 1024: return "1024";
        default: return value.ToString();
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public EvOfIntToMsgConverter() { }
  }


  //<< Copy from EP.TE.App1
  public class HtmlDecodeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        var s = value.ToString();
        s = s.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");
        s = s.Replace("</me1>", "\n").Replace("< /me1>", "\n");
        s = Regex.Replace(s, @"<(.|\n)*?>", string.Empty); //http://www.osherove.com/blog/2003/5/13/strip-html-tags-from-a-string-using-regular-expressions.html
        s = s.Replace("   ", " ").Replace("\r", "");
        s = s.Replace("\n\n", "\n").Replace("\n\n", "\n");
        s = s.Replace("&nbsp;", " ").Replace("&gt;", ">").Replace("&lt;", "<");
        return s;
      }
      catch (Exception ex)
      {
        return ex.ToString();
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
  }
  public class SimpleMiltiplierConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        if ((value is double) && double.TryParse(parameter.ToString(), out var d))
          return d * (double)value;

        if ((value is decimal) && double.TryParse(parameter.ToString(), out d))
          return (d * (double)(decimal)value);

        return 0;
      }
      catch
      {
        return 0;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
  }
  public class UniversalFormatConverter : IValueConverter
  {
    int usage = 0;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(parameter is string))
        return value;

      try
      {
        usage++;
        if (usage % 8000 == 0)
          System.Diagnostics.Trace.WriteLine(usage);

        if (value is DateTime && ((DateTime)value).CompareTo(new DateTime()) == 0)
          return "";      //The default value of date time should bo blank
        else
          return string.Format(culture, (string)parameter, value);
      }
      catch (Exception ex)
      {
        return ex.ToString();
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType.ToString() != "System.DateTime")
        return value;

      try
      {
        var formattingString = (string)parameter;
        formattingString = formattingString.Remove(0, formattingString.IndexOf(":") + 1);
        formattingString = formattingString.Remove(formattingString.IndexOf("}"));
        var formats = new string[] { formattingString,
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

        if (DateTime.TryParseExact((string)value, formats, culture, DateTimeStyles.AssumeLocal, out var date))
          return date;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.WriteLine(
            $"\n***{ex.GetType().Name} in {GetType().Name}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}:\n {ex.Message}\n {(ex.InnerException == null ? "" : ex.InnerException.Message)}\n");
      }

      return DateTime.MaxValue;//returning null is not an option.
    }
  }
  public class hhmm2dec : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var d = (decimal)value;

      var s = $"{Math.Truncate(d):0#}:{60 * (d - Math.Truncate(d)):0#}";

      return s;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var maskedText = value as string;

      var hhmm = maskedText.Replace("_", "").Split(':');
      int h = 0, m = 0;
      if (hhmm.Length == 0) return DependencyProperty.UnsetValue;
      if (hhmm.Length > 0 && int.TryParse(hhmm[0], out var dcm)) { h = dcm; }
      if (hhmm.Length > 1 && int.TryParse(hhmm[1], out dcm)) { m = dcm; }

      return h + m / 60m;
    }
  }
  public class NumericZeroToNullConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is decimal && (decimal)value == 0) return null;
      if (value is double && (double)value == 0) return null;
      if (value is float && (float)value == 0) return null;
      if (value is int && (int)value == 0) return null;

      if (!(parameter is string))
        return value;

      try
      {
        return string.Format(culture, (string)parameter, value);
      }
      catch (Exception ex)
      {
        return ex.ToString();
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
  }

  public class DeletedColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool && ((bool)value))
        return new SolidColorBrush(Colors.LightGray);//		return new BrushConverter().ConvertFromString("#ff0000");
      else
        return Color.FromRgb(255, 0, 0);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }
  public class DateToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is DateTime && ((DateTime)value) < DateTime.Today) // if in the past:
        return new SolidColorBrush(Colors.White);//.Gray);//		return new BrushConverter().ConvertFromString("#ff0000");
      else
        return new SolidColorBrush(Colors.Gray);//		Red);//		Color.FromRgb(255, 0, 0);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }
  public class TaskStatusToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string))
        return new SolidColorBrush(Colors.Gray);

      switch (value.ToString())
      {
        default: return new SolidColorBrush(Colors.SlateBlue);
        case "Hold": return new SolidColorBrush(Colors.DarkBlue);
        case "Estimating": return new SolidColorBrush(Colors.Violet);
        case "Open": return new SolidColorBrush(Colors.Yellow);
        case "In Progress": return new SolidColorBrush(Color.FromRgb(99, 255, 99));
        case "Staging": return new SolidColorBrush(Color.FromRgb(144, 144, 255));
        case "Completed": return new SolidColorBrush(Colors.Red);
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }
  public class StrProgressToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string)
      {
        var pa = ((string)value).Split('/');
        if (pa.Length == 2)
        {
          if (int.TryParse(pa[0], out var progressValue) && int.TryParse(pa[1], out var progressMaximum) && progressMaximum > 0)
          {
            var c = progressMaximum == 0 ? 0 : (255 * progressValue / progressMaximum);
            //c = c > 0xff ? 0xff : c;
            //return new SolidColorBrush(Color.FromRgb((byte)c, (byte)0x80, (byte)(c / 2)));

            if (c < 50) return new SolidColorBrush(Color.FromRgb(0, 150, 0));
            else if (c < 88) return new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x00));
            else if (c < 170) return new SolidColorBrush(Color.FromRgb(0xFF, 0x90, 0x00));
            else if (c < 250) return new SolidColorBrush(Color.FromRgb(0xE0, 0xFF, 0x80));
            else return new SolidColorBrush(Color.FromRgb(0xff, 0x40, 0x00));
          }
        }

        return new SolidColorBrush(Colors.Yellow); //0 max: eternal tasks
      }

      return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }
  public class WeekDayToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is DateTime)// && ((DateTime)value) < DateTime.Now) // if in the past:
      {
        switch (((DateTime)value).DayOfWeek)
        {
          case DayOfWeek.Monday: return new SolidColorBrush(Color.FromRgb(0, 0, 255));
          case DayOfWeek.Tuesday: return new SolidColorBrush(Colors.Magenta);
          case DayOfWeek.Wednesday: return new SolidColorBrush(Colors.Gray);
          case DayOfWeek.Thursday: return new SolidColorBrush(Color.FromRgb(0, 0xb8, 0xb8));
          case DayOfWeek.Friday: return new SolidColorBrush(Color.FromRgb(0, 200, 50));
          case DayOfWeek.Saturday: return new SolidColorBrush(Colors.Red);
          case DayOfWeek.Sunday: return new SolidColorBrush(Colors.Red);//		return new BrushConverter().ConvertFromString("#ff0000");
        }
      }

      return new SolidColorBrush(Colors.Gray);//		return new BrushConverter().ConvertFromString("#ff0000");
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }
  public class WeekdaysTo6Colors_ORG : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is DateTime)// && ((DateTime)value) < DateTime.Now) // if in the past:
      {
        byte upper = 0x19, lower = 0x20;
        switch (((DateTime)value).DayOfWeek)
        {
          case DayOfWeek.Monday: return new LinearGradientBrush(Color.FromArgb(upper, 0x00, 0x00, 0xff), Color.FromArgb(lower, 0x00, 0x00, 0xff), 90);
          case DayOfWeek.Tuesday: return new LinearGradientBrush(Color.FromArgb(upper, 0xFC, 0x00, 0xFF), Color.FromArgb(lower, 0xFC, 0x00, 0xFF), 90);
          case DayOfWeek.Wednesday: return new LinearGradientBrush(Color.FromArgb(upper, 0xFF, 0xA2, 0x00), Color.FromArgb(lower, 0xFF, 0xA2, 0x00), 90);
          case DayOfWeek.Thursday: return new LinearGradientBrush(Color.FromArgb(upper, 0x00, 0xB8, 0xB8), Color.FromArgb(lower, 0x00, 0xB8, 0xB8), 90);
          case DayOfWeek.Friday: return new LinearGradientBrush(Color.FromArgb(upper, 0x00, 0xFF, 0x00), Color.FromArgb(lower, 0x00, 0xFF, 0x00), 90);
          case DayOfWeek.Saturday:
          case DayOfWeek.Sunday: return new LinearGradientBrush(Color.FromArgb(upper, 0xFF, 0x00, 0x00), Color.FromArgb(lower, 0xFF, 0x00, 0x00), 90);
        }
      }

      return new LinearGradientBrush(Colors.Gray, Colors.DarkGray, 45);//		return new BrushConverter().ConvertFromString("#ff0000");
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }
  public class DateToColorMultiValueConverter : IMultiValueConverter
  {
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return new SolidColorBrush(Colors.Beige);

      if (value.Length > 1
        && value[0] is DateTime  // TNC_DateForCompletion
        && value[1] is DateTime) // TNC_CorrectedOn
      {
        if (((DateTime)value[1]) <= ((DateTime)value[0]))
          return new SolidColorBrush(Colors.Green);   // if TNC_CorrectedOn is BEFORE TNC_DateForCompletion:
        else
          return new SolidColorBrush(Colors.Blue);    // if TNC_CorrectedOn is  AFTER TNC_DateForCompletion:
      }
      else if (value.Length > 0
        && value[0] is DateTime) // if not corrected yet
      {
        if (DateTime.Today > ((DateTime)value[0]))
          return new SolidColorBrush(Colors.Red);   // past due 
        else
        {
          var clr0 = Color.FromRgb(192, 192, 192);
          var clr1 = Colors.Purple;
          const int warnPeriod = 3; //days ahead to start changing the color to warn about approaching due date.
          var daysLeft = (int)(((DateTime)value[0]) - DateTime.Today).TotalDays;
          if (daysLeft > warnPeriod)
            return new SolidColorBrush(clr0); // plenty of time
          else
          {
            var proximity = (warnPeriod - daysLeft) / (float)warnPeriod; // closer to due date - closer to 1.
            return new SolidColorBrush(Color.FromRgb(
              (byte)(clr0.R + (clr1.R - clr0.R) * proximity),
              (byte)(clr0.G + (clr1.G - clr0.G) * proximity),
              (byte)(clr0.B + (clr1.B - clr0.B) * proximity))); // approaching due date
          }
        }
      }
      else
        return new SolidColorBrush(Colors.Purple);//		Color.FromRgb(255, 0, 0);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
  }
  public class MultiplyingMultiValueConverter : IMultiValueConverter
  {
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
      decimal rv = 0;

      if (value == null || value.Length < 2)
        return null;

      if (value[0] is int && value[1] is decimal)
        rv = (int)value[0] * ((decimal)value[1]);

      return rv == 0 ? null : rv.ToString("C");
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
  }
  public class WorkedHourCountValueConverter : IMultiValueConverter
  {
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
      decimal rv = 0;

      if (value == null || value.Length < 3)
        return null;

      if (value[3] is bool && (bool)value[3] == true)
      {
        if (value[0] is decimal && value[1] is decimal && value[2] is int)
          rv = (decimal)value[1] - (decimal)value[0] - (decimal)((int)value[2] / 60.0);
      }
      else
      {
        if (value[0] is decimal && value[1] is decimal)
          rv = (decimal)value[1] - (decimal)value[0];
      }

      var i = Math.Truncate(rv);
      var d = rv - i;

      var s = $"{i:0#}:{d * 60m:0#}";
      return s;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
  }

  public class BoolToOpacityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        return (value is bool && ((bool)value)) ? 1 : 0.5;
      }
      catch
      {
        return 1;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class QuestionTypeToVisibilityConverter : IValueConverter
  {
    //const string
    //  csValueTypeText = "Text",
    //  csValueTypeChoice = "SingleChoice",
    //  csValueTypeDateMY = "DateMY",
    //  csValueTypeDateMDY = "DateMDY",
    //  csValueTypeMultiChoice = "MultiChoice";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        if (parameter != null && parameter is string && value != null && value is string)
        {
          var valueType = parameter as string;
          var valueText = value as string;
          var v = string.Compare(valueType, valueText, true) == 0 ?
            Visibility.Visible :
            Visibility.Collapsed;

          return v;
        }

        return (value is string && ((string)value != "Section")) ? Visibility.Visible : Visibility.Collapsed;
      }
      catch
      {
        return Visibility.Visible;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
  public class QuestionTypeToBoolConverter : IValueConverter
  {
    //const string
    //  csValueTypeText = "Text",
    //  csValueTypeChoice = "SingleChoice",
    //  csValueTypeDateMY = "DateMY",
    //  csValueTypeDateMDY = "DateMDY",
    //  csValueTypeMultiChoice = "MultiChoice";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        if (parameter != null && parameter is string && value != null && value is string)
        {
          var valueType = parameter as string;
          var valueText = value as string;
          var v = string.Compare(valueType, valueText, true) == 0 ?
            true :
            false;

          return v;
        }

        return (value is string && ((string)value != "Section")) ? true : false;
      }
      catch
      {
        return true;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class SectionVsQuestionTypeToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        return (value is string && ((string)value != "Section")) ? Visibility.Visible : Visibility.Collapsed;
      }
      catch
      {
        return Visibility.Visible;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
  public class SectionVsQuestionTypeToFontStyleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        return (value is string && ((string)value == "Section")) ? FontStyles.Italic : FontStyles.Normal;
      }
      catch
      {
        return Visibility.Visible;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
  public class SectionVsQuestionTypeToFontWeightConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        return (value is string && ((string)value == "Section")) ? FontWeights.ExtraBold : FontWeights.Normal;
      }
      catch
      {
        return Visibility.Visible;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
  public class SectionVsQuestionTypeToBoolConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        return (value is string && ((string)value == "Section"));
      }
      catch
      {
        return false;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
  }

  public class UserLanguageEnToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        return (value is string && (((string)value).Contains("en"))) ? Visibility.Visible : Visibility.Collapsed;
      }
      catch
      {
        return Visibility.Visible;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
  public class UserLanguageFrToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        return (value is string && (((string)value).Contains("fr"))) ? Visibility.Visible : Visibility.Collapsed;
      }
      catch
      {
        return Visibility.Visible;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class DGridHeightConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) //todo: use parameter set from the form.
    {
      try
      {
        if (value is double)
        {
          return (double)value > 400 ? (double)value - 340 : 60;
        }
      }
      catch
      {
        return 111;
      }

      return 44;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }


  public class BoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var visibility = (bool)value;
      return visibility ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var visibility = (Visibility)value;
      return (visibility == Visibility.Visible);
    }
  }
  public class BoolToVisibilityConverter_Reverse : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var visibility = !(bool)value;
      return visibility ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var visibility = (Visibility)value;
      return (visibility == Visibility.Visible);
    }
  }


  //<Button x:Name="DisabledButton" Click="RemovebtnInvoicePreview_Click" Content="Disabled Button" 
  //IsEnabled="{Binding ItemsSource.Count, ElementName=ListBox,Converter={StaticResource IsGreaterThanZero}}"  />
  public class IsGreaterThanZero : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => (value is int) && (int)value > 0;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
  }
  public class NoLessThanZero : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => (value is int) && (int)value >= 0;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
  }
  //>>


  public class Clr4 : MarkupExtension, IValueConverter
  {
    public Clr4() { }
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value == null ? Colors.Red : Colors.Green;
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
  }
  public class Equals3 : MarkupExtension, IValueConverter
  {
    public Equals3() { }
    bool _InvertValue = false; public bool InvertValue { get => _InvertValue; set => _InvertValue = value; }
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => _InvertValue ? value == null : value != null;
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
  }

  public class Equals : MarkupExtension, IValueConverter
  {
    public Equals() { }

    bool _InvertValue = false; public bool InvertValue { get => _InvertValue; set => _InvertValue = value; }

    public string EqualsText { get; set; }
    public string NotEqualsText { get; set; }
    public Brush BrushTrue { get; set; }
    public Brush BrushFalse { get; set; }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool result;

      if (value != null)
        result = InvertValue ? !value.Equals(parameter) : value.Equals(parameter);
      else
      {
        result = value == null && parameter == null;
        result = InvertValue ? !result : result;
      }

      if (targetType == typeof(Visibility))
        return result ? Visibility.Visible : Visibility.Collapsed;

      if (targetType == typeof(string))
        return result ? EqualsText : NotEqualsText;

      if (targetType == typeof(Brush))
        return result ? BrushTrue : BrushFalse;

      return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
  }

  public class Equals2 : MarkupExtension, IValueConverter
  {
    //public bool InvertValue { get; set; } // 
    public bool InvertValue { get => _InvertValue; set => _InvertValue = value; }
    bool _InvertValue = false;
    public string EqualsText { get; set; }
    public string NotEqualsText { get; set; }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool result;

      if (value != null) result = InvertValue ? !value.Equals(parameter) : value.Equals(parameter);
      else
      {
        result = value == null && parameter == null;
        result = InvertValue ? !result : result;
      }

      if (targetType == typeof(Visibility)) return result ? Visibility.Visible : Visibility.Collapsed;
      if (targetType == typeof(string)) return result ? EqualsText : NotEqualsText;

      return result;
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
  }
  public class Bool : MarkupExtension, IValueConverter
  {
    public bool InvertValue { get; set; } // public bool InvertValue		{			get { return _InvertValue; }			set { _InvertValue = value; }		}		 bool _InvertValue = false;

    public string TrueText { get; set; }
    public string FalseText { get; set; }

    public SolidColorBrush TrueColor { get; set; }
    public SolidColorBrush FalseColor { get; set; }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (!(value is bool)) throw new InvalidCastException("Boolean converter expects to convert boolean.");

      var result = InvertValue ? !(bool)value : (bool)value;

      if (targetType == typeof(Visibility)) return result ? Visibility.Visible : Visibility.Collapsed;
      if (targetType == typeof(string)) return result ? TrueText : FalseText;
      if (targetType == typeof(Brush)) return result ? TrueColor ?? Brushes.SpringGreen : FalseColor ?? Brushes.Red; // : ;

      return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
  }

  public class VisibilityConverter : MarkupExtension, IValueConverter
  {
    public bool InvertValue { get; set; } // public bool InvertValue		{			get { return _InvertValue; }			set { _InvertValue = value; }		}		 bool _InvertValue = false;

    public string EqualsText { get; set; }
    public string NotEqualsText { get; set; }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool result;

      if (parameter == null && value is bool)
        result = InvertValue ? !(bool)value : (bool)value;
      else if (parameter == null || (parameter is string && string.CompareOrdinal((string)parameter, "NullIsInvisible") == 0))
      {
        //if (string.IsNullOrEmpty(EqualsText))
        //	return InvertValue ?
        //		(value == null ? Visibility.Visible : Visibility.Collapsed) :
        //		(value != null ? Visibility.Visible : Visibility.Collapsed);
        //else
        return InvertValue ?
          (value == null ? Visibility.Visible : Visibility.Collapsed) :
          (value != null && (
          (string.IsNullOrEmpty(EqualsText) || value.ToString() == EqualsText) &&
          (string.IsNullOrEmpty(NotEqualsText) || value.ToString() != NotEqualsText)) ? Visibility.Visible : Visibility.Collapsed);
      }
      else      //if (value != null)        result = InvertValue ? !value.ToString().Equals(parameter) : value.ToString().Equals(parameter);
      {
        result = value == null && parameter == null;
        result = InvertValue ? !result : result;
      }

      if (targetType == typeof(Visibility)) return result ? Visibility.Visible : Visibility.Collapsed;
      return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
  }
  public class MultiBoolean : MarkupExtension, IMultiValueConverter
  {
    public bool UseOrInsteadOfAnd { get; set; }
    public bool InvertValue { get; set; }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool? result = null;

      for (var i = 0; i < values.Length; i++)
      {
        if (values[i] == DependencyProperty.UnsetValue)
          return false;

        if (result == null) result = (bool)values[i];
        result = UseOrInsteadOfAnd ? result.Value || (bool)values[i] : result.Value && (bool)values[i];
      }

      if (InvertValue) result = !result;
      if (targetType == typeof(Visibility)) return result.Value ? Visibility.Visible : Visibility.Collapsed;
      return result.Value;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
  }
}
