using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace xPocBits.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileMngr : Page
	{
		public FileMngr()
		{
			this.InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			var rootFrame = Window.Current.Content as Frame;
			if (rootFrame.CanGoBack)
			{
				Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
			}
		}
	}
}
