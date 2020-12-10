using Windows.UI.Xaml.Controls;
namespace VideoPlayerBackground.Vws
{
  public sealed partial class MruItemUC : UserControl
  {
    public MruItemUC() { InitializeComponent(); DataContextChanged += (s, e) => Bindings.Update(); }
    public VpxCmn.Model.MediaInfoDto Mid => DataContext as VpxCmn.Model.MediaInfoDto; /*new TextBlock().Foreground;*/
  }
}