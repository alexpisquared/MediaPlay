using ApxCmn;
using AsLink;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VideoPlayerBackground.Vws
{
  public sealed partial class Pg3_DevDbgUC : UserControl
  {
    public Pg3_DevDbgUC()
    {
      this.InitializeComponent();
    }
    MainPageAbr mp; public MainPageAbr MP { get => mp; set { mp = value; } }

    async void onRoamInfo(object sender, RoutedEventArgs e) { tbx.Text = $"{ApplicationData.Current.RoamingFolder.Path}"; tbRoamT.Text = $"Roaming Quota: Total {ApplicationData.Current.RoamingStorageQuota} KB,  Used {await IsoStorePoc.GetRoamingFolderSizeKbFromFiles():N0} kB by files, {((string)AppSettingsHelper.ReadVal(AppSetConst.Mru4Roam))?.Length} by data."; }
    async void onSpeakTest(object sender, RoutedEventArgs e) => await MP.Speak("Just kidding");
  }
}
