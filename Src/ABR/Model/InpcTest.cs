using MVVM.Common;
using PropertyChanged;
using System.ComponentModel;
using System.Diagnostics;

namespace ABR.Model
{
    [AddINotifyPropertyChangedInterface] //fody
  public class InpcTestFody { public int Perc { get; set; } }
  public class InpcTestImpl : ViewModelBase { int _perc; public int Perc { get => _perc; set { if (Set(ref _perc, value)) Debug.WriteLine(value); } } }
  public class InpcTestBase : INotifyPropertyChanged { public event PropertyChangedEventHandler PropertyChanged; public int Perc { get; set; } }
  public class InpcTestNone { public int Perc { get; set; } }
}