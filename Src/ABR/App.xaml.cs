using MVVM.Common;
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
namespace ABR
{
  sealed partial class App : Application
  {
    public App()
    {
      InitializeComponent();
      Suspending += OnSuspending;

      Debug.WriteLine($"{ApplicationData.Current.LocalFolder.Path}\r\n{ApplicationData.Current.RoamingFolder.Path}");
      EnteredBackground += onEnteredBackground;
      LeavingBackground += onLeavingBackground;
      //UnhandledException += App_UnhandledException;
    //}

    //async void App_UnhandledException(object s, UnhandledExceptionEventArgs e)
    //{
    //  var msgbox = new Windows.UI.Popups.MessageDialog("Unhandled exception: " + e.Exception.ToString());
    //  msgbox.Commands.Add(new Windows.UI.Popups.UICommand("close"));
    //  await msgbox.ShowAsync();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
      // Do not repeat app initialization when the Window already has content,
      // just ensure that the window is active
      if (!(Window.Current.Content is Frame rootFrame))
      {
        // Create a Frame to act as the navigation context and navigate to the first page
        rootFrame = new Frame();

        rootFrame.NavigationFailed += OnNavigationFailed;

        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
        {
          //TODO: Load state from previously suspended application
        }

        // Place the frame in the current Window
        Window.Current.Content = rootFrame;
      }

      if (e.PrelaunchActivated == false)
      {
        if (rootFrame.Content == null)
        {
          // When the navigation stack isn't restored navigate to the first page,
          // configuring the new page by passing required information as a navigation
          // parameter
          rootFrame.Navigate(typeof(AbrMainPg), e.Arguments);
        }
        // Ensure the current window is active
        Window.Current.Activate();
      }

      afterOnLaunched__(e);
    }
    void OnNavigationFailed(object s, NavigationFailedEventArgs e)
    {
      throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }
    void OnSuspending(object s, SuspendingEventArgs e)
    {
      var deferral = e.SuspendingOperation.GetDeferral();
      //TODO: Save application state and stop any background activity
      deferral.Complete();
    }

    #region MinimalBcgrdMedia // https://mtaulty.com/2016/10/16/windows-10-1607-uwp-and-background-media/
    void afterOnLaunched__(LaunchActivatedEventArgs e)
    {
      if ((e.PreviousExecutionState != ApplicationExecutionState.Running) &&
          (e.PreviousExecutionState != ApplicationExecutionState.Suspended))
      {
        createMediaPlayer();
        create_UI();
        Mp_Ap.Play();               //Window.Current.Activate();
      }
    }
    void onLeavingBackground(object s, LeavingBackgroundEventArgs e) { create_UI(); }
    void onEnteredBackground(object s, EnteredBackgroundEventArgs e) { destroyUI(); }
    void destroyUI() { MpeAp.SetMediaPlayer(null); MpeAp = null; } //Window.Current.Content = null; }
    void create_UI()
    {
      MpeAp = ((AbrMainPg)(Window.Current.Content as Frame).Content).Pg1_PlrUC1.MpeXm;
      MpeAp.AreTransportControlsEnabled = true;
      MpeAp.SetMediaPlayer(Mp_Ap);
    }
    void createMediaPlayer()
    {
      Mp_Ap = ViewModelDispatcher.AbrVM.Mp_Vm;
      Mp_Ap.SystemMediaTransportControls.IsEnabled = true;
      Mp_Ap.SystemMediaTransportControls.AutoRepeatMode = MediaPlaybackAutoRepeatMode.Track;
    }

    public MediaPlayerElement MpeAp;
    public MediaPlayer Mp_Ap;
    #endregion
  }
}
