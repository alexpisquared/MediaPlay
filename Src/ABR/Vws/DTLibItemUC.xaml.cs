using ABR.VMs;
using MVVM.Common;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws
{
  public sealed partial class DTLibItemUC : UserControl
  {
    readonly AbrVM _abrVM;
    public DTLibItemUC()
    {
      InitializeComponent();
      _abrVM = ViewModelDispatcher.AbrVM;
      DataContextChanged += (s, e) => Bindings.Update();
    }
    public VpxCmn.Model.MediaInfoDto Mid => this?.DataContext as VpxCmn.Model.MediaInfoDto;
  }
}