using Windows.UI.Xaml.Controls;
namespace ABR.Vws
{
  public sealed partial class DTMruItemUC : UserControl
  {
    public DTMruItemUC()
    {
      InitializeComponent(); DataContextChanged += (s, e) => Bindings.Update();
    }
    public VpxCmn.Model.MediaInfoDto Mid => DataContext as VpxCmn.Model.MediaInfoDto;
  }
}