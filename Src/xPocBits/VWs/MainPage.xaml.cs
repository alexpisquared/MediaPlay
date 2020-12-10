using AsLink;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using xPocBits.Views;
using xPocBits.VMs;
using xPocBits.VWs;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace xPocBits
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
  {
    MainPage rootPage;               //C:\gh\Windows-universal-samples\Samples\BackButton\cs\Scenario1.xaml.cs
    public static MainPage Current;  //C:\gh\Windows-universal-samples\Samples\BackButton\cs\Scenario1.xaml.cs
    MainVM _vm = null;

    public MainPage()
    {
      this.InitializeComponent();

      _vm = MainVM.Instance;
      DataContext = _vm;
			_vm.MediaElement = pl1;

      var bt = DevOp.BuildTime(typeof(App));
#if DEBUG
      ApplicationView.GetForCurrentView().Title = tbVer.Text = $@"Dbg: {(DateTime.Now - bt):d\ h\:mm} ago";
#else
			ApplicationView.GetForCurrentView().Title = tbVer.Text = $@"Rls: {bt}";
#endif
			      
      this.NavigationCacheMode = NavigationCacheMode.Required; // I want this page to be always cached so that we don't have to add logic to save/restore state for the checkbox. //C:\gh\Windows - universal - samples\Samples\BackButton\cs\Scenario1.xaml.cs
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      rootPage = MainPage.Current;  //C:\gh\Windows-universal-samples\Samples\BackButton\cs\Scenario1.xaml.cs
      Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
      _vm.F2Cmd.Execute(null);
      _vm.IsReady = true; //todo: 
    }

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
      base.OnKeyDown(e);

      switch (e.Key)
      {
        case VirtualKey.Escape: CoreApplication.Exit(); break;
        default: Debug.WriteLine($"case VirtualKey.{e.Key}:            break;"); break;
      }
    }


    public MainVM VM { get { return _vm; } }


    void onGoToPrivPlcy(object sender, RoutedEventArgs e) { (Window.Current.Content as Frame).Navigate(typeof(PrivacyPolicy)); }
    void onGoToFileMngr(object sender, RoutedEventArgs e) { (Window.Current.Content as Frame).Navigate(typeof(FileMngr)); }
    void onGoToAudioPlr(object sender, RoutedEventArgs e) { (Window.Current.Content as Frame).Navigate(typeof(AudioPlayer)); }
    void onGoToGhsAuPlr(object sender, RoutedEventArgs e) { (Window.Current.Content as Frame).Navigate(typeof(GhsAudioPlayerVw)); }
  }
}
