using AsLink;
using ApxCmn;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace xPocBits.VWs
{
	public sealed partial class GhsAudioPlayerVw : Page
	{
		////MainPage rootPage;
		public GhsAudioPlayerVw()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;      // Always use the cached page
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			////rootPage = MainPage.Current;
			var rootFrame = Window.Current.Content as Frame; if (rootFrame.CanGoBack) Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;

			p1.PlaylistView.ItemClick += p1.PlaylistView_ItemClick;

			Application.Current.Suspending += p1.ForegroundApp_Suspending; // Adding App suspension handlers here so that we can unsubscribe handlers that access BackgroundMediaPlayer events
			Application.Current.Resuming += p1.ForegroundApp_Resuming;
			AppSettingsHelper.SaveVal(ApplicationSettingsConstants.AppState, AppState.Active.ToString());
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			if (p1.IsMyBackgroundTaskRunning)
			{
				p1.RemoveMediaPlayerEventHandlers();
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());
			}

			base.OnNavigatedFrom(e);
		}
	}
}
