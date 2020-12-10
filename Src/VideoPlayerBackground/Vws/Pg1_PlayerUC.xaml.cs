using Windows.UI.Xaml.Controls;
namespace VideoPlayerBackground.Vws
{
    public sealed partial class Pg1_PlayerUC : UserControl
  {
    public Pg1_PlayerUC()
    {
      this.InitializeComponent();
    }
    MainPageAbr mp; public MainPageAbr MP { get => mp; set { mp = value; } }
  }
}
