using ABR.VMs;
using MVVM.Common;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws
{
  public sealed partial class DTPLtItemUC : UserControl
  {
    readonly AbrVM _abrVM;
    public DTPLtItemUC()
    {
      InitializeComponent();
      _abrVM = ViewModelDispatcher.AbrVM;
      DataContextChanged += (s, e) => Bindings.Update();
    }
    public VpxCmn.Model.MediaInfoDto Mid => DataContext as VpxCmn.Model.MediaInfoDto;
  }
}