using ABR.VMs;
using MVVM.Common;
using Windows.UI.Xaml.Controls;

namespace ABR.Vws
{
  public sealed partial class Pg4_CurDtl : UserControl
  {
    AbrVM _abrVM; public Pg4_CurDtl() { this.InitializeComponent(); if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return; _abrVM = ViewModelDispatcher.AbrVM; }
  }
}
