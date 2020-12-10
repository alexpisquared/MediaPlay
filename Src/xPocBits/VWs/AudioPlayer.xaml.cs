using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace xPocBits.Views
{
    public sealed partial class AudioPlayer : Page
	{
		public AudioPlayer()
		{
			this.InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			var rootFrame = Window.Current.Content as Frame; if (rootFrame.CanGoBack) Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
		}
	}
}
