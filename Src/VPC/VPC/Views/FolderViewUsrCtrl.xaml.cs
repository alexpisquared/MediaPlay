using System.Globalization;
//using System.Speech.Synthesis;
using System.Windows.Data;

namespace VPC.Views;

public partial class FolderViewUsrCtrl : UserControl
{
  public static readonly DependencyProperty MUsProperty = DependencyProperty.Register("MUs", typeof(ObservableCollection<MediaUnit>), typeof(FolderViewUsrCtrl), new PropertyMetadata(null)); public ObservableCollection<MediaUnit> MUs { get => (ObservableCollection<MediaUnit>)GetValue(MUsProperty); set => SetValue(MUsProperty, value); }
  public static readonly DependencyProperty CurMediaFile2Property = DependencyProperty.Register("CurMediaFile2", typeof(string), typeof(FolderViewUsrCtrl), new PropertyMetadata(null)); public string CurMediaFile2 { get => (string)GetValue(CurMediaFile2Property); set => SetValue(CurMediaFile2Property, value); }
  public static readonly DependencyProperty SrchProperty = DependencyProperty.Register("Srch", typeof(string), typeof(FolderViewUsrCtrl), new PropertyMetadata(null)); public string Srch { get => (string)GetValue(SrchProperty); set => SetValue(SrchProperty, value); }
  public static readonly DependencyProperty CurMu9Property = DependencyProperty.Register("CurMu9", typeof(MediaUnit), typeof(FolderViewUsrCtrl), new PropertyMetadata(null)); public MediaUnit CurMu9 { get => (MediaUnit)GetValue(CurMu9Property); set => SetValue(CurMu9Property, value); }
  //SpeechSynthesizer _synth; public SpeechSynthesizer Synth { get { if (_synth == null) _synth = new SpeechSynthesizer(); return _synth; } }

  bool _inclSubDirs = false;
  //ObservableCollection<MediaUnit> _flt;
  internal enum Enm { Folder, Histry }; Enm _fMode;
  public FolderViewUsrCtrl() { InitializeComponent(); DataContext = this; }

  public void LoadOneDrCash(bool inclSubDirs) { loadAll(Enm.Histry, inclSubDirs); /*dgMUnits.DataContext = MUs; */findHighLightCurrent(); }
  public void LoadCurFolder(bool inclSubDirs) { loadAll(Enm.Folder, inclSubDirs); /*dgMUnits.DataContext = MUs; */findHighLightCurrent(); }

  void loadAll(Enm fmode, bool inclSubDirs)
  {
    _fMode = fmode;
    _inclSubDirs = inclSubDirs;
    var fvm = new FolderViewModel { CurFile = CurMediaFile2 };

    if (_fMode == Enm.Folder)
      fvm.LoadDirFromFile(CurMediaFile2, inclSubDirs);
    else
      fvm.PopulateFromSkyCache(inclSubDirs);

    MUs = fvm.MediaUnits; // MUs = new ObservableCollection<CrntMU>(fvm.MediaUnits);
  }

  internal void DoSearch(string srch, bool inclSubDirs)
  {
    Srch = srch;

    if (_inclSubDirs != inclSubDirs)
    {
      _inclSubDirs = inclSubDirs;

      loadAll(_fMode, inclSubDirs);
    }

    //MUs = fvm.MediaUnits.Where(r => r.FileName.ToLower().Contains(srch.ToLower())); // 
    MUs = new ObservableCollection<MediaUnit>(MUs.Where(r => r.FileName.ToLower().Contains(srch.ToLower())));
  }

  void findHighLightCurrent()
  {
    var i = 0;
    foreach (var mu in MUs) { if (mu.PathFileCur == CurMediaFile2) break; i++; }

    dgMUnits.SelectedIndex = i;
    if (dgMUnits.SelectedItem != null)
      dgMUnits.ScrollIntoView(dgMUnits.SelectedItem);
  } //dg1.Items.MoveCurrentToPosition(i);
  public void MovCurrentToPrev()
  {
    if (dgMUnits.SelectedIndex > 0) dgMUnits.SelectedIndex--;

    _ = dgMUnits.Items.MoveCurrentToPrevious();

    if (dgMUnits.SelectedItem == null) return;

    CurMediaFile2 = ((VPC.Models.MediaUnit)dgMUnits.SelectedItem).PathFileCur;  //j2016
    dgMUnits.ScrollIntoView(dgMUnits.SelectedItem);
  }
  public void MovCurrentToNext()
  {
    if (dgMUnits.SelectedIndex < dgMUnits.Items.Count - 1) dgMUnits.SelectedIndex++; else dgMUnits.SelectedIndex = 0; // else part added on 2020-11-21.

    _ = dgMUnits.Items.MoveCurrentToNext();

    if (dgMUnits.SelectedItem == null || !File.Exists(((MediaUnit)dgMUnits.SelectedItem).PathFileCur))
    {
      //Synth.SpeakAsync("File does not exists on this PC.");
      return; //f2017
    }

    CurMediaFile2 = ((MediaUnit)dgMUnits.SelectedItem).PathFileCur;  //j2016
    dgMUnits.ScrollIntoView(dgMUnits.SelectedItem);
  }

  void mediaUnitsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (e.AddedItems.Count < 1) return;

    CurMu9 = (MediaUnit)((object[])e.AddedItems)[0];
    if (File.Exists(CurMu9.PathFileCur))
      CurMediaFile2 = CurMu9.PathFileCur;
  }
  void mediaUnitsDataGrid_MouseDoubleClick_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
  {
    if (dgMUnits.SelectedItems.Count < 1) return;

    dynamic row = dgMUnits.SelectedItems[0];

    CurMediaFile2 = row.PathFileCur;
  }
}

public class EmptyOnZero : IValueConverter
{
  public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
  {
    if (value == null) return value;

    if (value is TimeSpan && ((TimeSpan)value).TotalSeconds == 0) return null;
    if (value is int && (int)value == 0) return null;
    return value is long && (long)value == 0 ? null : value;
  }
  public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new System.NotImplementedException();
}
public class WeekdaysTo2Colors : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is DateTime)
    {
      return ((DateTime)value).DayOfWeek switch
      {
        DayOfWeek.Saturday or DayOfWeek.Sunday => new SolidColorBrush(Color.FromRgb(0xa0, 0x01, 0x01)),
        _ => new SolidColorBrush(Color.FromRgb(0x01, 0x0001, 0xa0)),
      };
    }

    return new LinearGradientBrush(Colors.Gray, Colors.DarkGray, 45);//		return new BrushConverter().ConvertFromString("#ff0000");
  }
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}
