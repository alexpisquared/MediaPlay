using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
namespace VideoPlayerBackground
{
  sealed partial class App : Application
  {
    public App()
    {
      InitializeComponent();
      Suspending += OnSuspending;

      Debug.WriteLine($"{ApplicationData.Current.RoamingFolder.Path}");
      EnteredBackground += onEnteredBackground;
      LeavingBackground += onLeavingBackground;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
      var rootFrame = Window.Current.Content as Frame;

      if (rootFrame == null) // Do not repeat app initialization when the Window already has content, just ensure that the window is active
      {
        rootFrame = new Frame(); // Create a Frame to act as the navigation context and navigate to the first page

        rootFrame.NavigationFailed += OnNavigationFailed;

        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
        {
          //TODO: Load state from previously suspended application
        }

        Window.Current.Content = rootFrame; // Place the frame in the current Window
      }

      if (e.PrelaunchActivated == false)
      {
        if (rootFrame.Content == null)
        {
          rootFrame.Navigate(typeof(MainPageAbr), e.Arguments); // When the navigation stack isn't restored navigate to the first page, configuring the new page by passing required information as a navigation parameter
        }

        Window.Current.Activate(); // Ensure the current window is active
      }

      afterOnLaunched__(e);
    }
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    void OnSuspending(object sender, SuspendingEventArgs e) // Invoked when application execution is being suspended.  Application state is saved without knowing whether the application will be terminated or resumed with the contents of memory still intact.
    {
      var deferral = e.SuspendingOperation.GetDeferral();
      //TODO: Save application state and stop any background activity
      deferral.Complete();
    }

    #region MinimalBcgrdMedia
    void afterOnLaunched__(LaunchActivatedEventArgs e)
    {
      if ((e.PreviousExecutionState != ApplicationExecutionState.Running) &&
          (e.PreviousExecutionState != ApplicationExecutionState.Suspended))
      {
        createMediaPlayer();
        createUI();
        Mp_Ap.Play();
        //Window.Current.Activate();
      }
    }
    void onLeavingBackground(object sender, LeavingBackgroundEventArgs e) { createUI(); isForeground = true; }
    void onEnteredBackground(object sender, EnteredBackgroundEventArgs e) { destroyUI(); isForeground = false; }
    void destroyUI() { MpeAp.SetMediaPlayer(null); MpeAp = null; } //Window.Current.Content = null; }
    void createMediaPlayer()
    {
      Mp_Ap = ((MainPageAbr)(Window.Current.Content as Frame).Content).Mp_Xm;      //this._mp = new MediaPlayer() { Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/PrimalEnd.mp3")) };
      Mp_Ap.SystemMediaTransportControls.IsEnabled = true;
      Mp_Ap.SystemMediaTransportControls.AutoRepeatMode = MediaPlaybackAutoRepeatMode.Track;
    }
    void createUI()
    {
      MpeAp = ((MainPageAbr)(Window.Current.Content as Frame).Content).MpeXm;      //this._mpe = new MediaPlayerElement();
      MpeAp.AreTransportControlsEnabled = true;
      MpeAp.SetMediaPlayer(Mp_Ap);
    }

    public MediaPlayerElement MpeAp;
    public MediaPlayer Mp_Ap;
    bool isForeground;
    #endregion
  }
}
/// https://mtaulty.com/2016/10/16/windows-10-1607-uwp-and-background-media/