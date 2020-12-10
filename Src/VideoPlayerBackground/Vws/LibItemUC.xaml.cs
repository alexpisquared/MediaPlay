using Windows.UI.Xaml.Controls;
namespace VideoPlayerBackground.Vws
{
  public sealed partial class LibItemUC : UserControl
  {
    public LibItemUC() { InitializeComponent(); DataContextChanged += (s, e) => Bindings.Update(); }
    public VpxCmn.Model.MediaInfoDto Mid => DataContext as VpxCmn.Model.MediaInfoDto;
  }
}