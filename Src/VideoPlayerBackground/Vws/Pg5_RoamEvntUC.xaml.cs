using AsLink;
using System;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VideoPlayerBackground.Vws
{
  public sealed partial class Pg5_RoamEvntUC : UserControl
  {
    public Pg5_RoamEvntUC()
    {
      this.InitializeComponent();
      ApplicationData.Current.DataChanged += onDataChanged;
      tbRoamDataChd.Text = $"{DateTime.Now:ddd HH:mm:ss}  Start << \r\n" + tbRoamDataChd.Text;
    }

    async void onDataChanged(Windows.Storage.ApplicationData sender, object args) => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => tbRoamDataChd.Text = $"{DateTime.Now:ddd HH:mm:ss}  {DevOp.MachineName}  {args}\r\n" + tbRoamDataChd.Text);

    MainPageAbr mp; public MainPageAbr MP { get => mp; set { mp = value; } }

    void onClearT(object sender, RoutedEventArgs e) { tbRoamDataChd.Text = ""; }
    void onUnhook(object sender, RoutedEventArgs e) { ApplicationData.Current.DataChanged -= onDataChanged; }
    void onSignal(object sender, RoutedEventArgs e) { ApplicationData.Current.SignalDataChanged(); }
  }
}
