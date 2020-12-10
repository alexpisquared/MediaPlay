using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using xPocBits.Views;
using xPocBits.VMs;

namespace xPocBits.UCs
{
	public sealed partial class MediaExplorerUC : UserControl
  {
    MainVM _vm = null;

    public MediaExplorerUC()
    {
      this.InitializeComponent();

      _vm = MainVM.Instance;
      DataContext = _vm;
    }

    public MainVM VM { get { return _vm; } }


    void onGoToPrivPlcy(object sender, RoutedEventArgs e) { (Window.Current.Content as Frame).Navigate(typeof(PrivacyPolicy)); }
    void onGoToMainPage(object sender, RoutedEventArgs e) { (Window.Current.Content as Frame).Navigate(typeof(MainPage)); }
  }
}
