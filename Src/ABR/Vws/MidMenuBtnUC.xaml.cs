using ABR.VMs;
using MVVM.Common;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws
{
  public sealed partial class MidMenuBtnUC : UserControl
  {
    readonly AbrVM _abrVM;
    public MidMenuBtnUC()
    {
      InitializeComponent();
      _abrVM = ViewModelDispatcher.AbrVM;
      DataContextChanged += (s, e) => Bindings.Update();
    }
    public VpxCmn.Model.MediaInfoDto Mid => DataContext as VpxCmn.Model.MediaInfoDto;
  }
}
