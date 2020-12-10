using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace xPocBits
{
  /// <summary>
  /// Provides application-specific behavior to supplement the default Application class.
  /// </summary>
  sealed partial class App : Application
  {
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
      InitializeComponent();
      Suspending += OnSuspending;
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="e">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
      var rootFrame = Window.Current.Content as Frame;

      // Do not repeat app initialization when the Window already has content,
      // just ensure that the window is active
      if (rootFrame == null)
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
          rootFrame.Navigate(typeof(MainPage), e.Arguments); // When the navigation stack isn't restored navigate to the first page, configuring the new page by passing required information as a navigation parameter
        }
        // Ensure the current window is active
        Window.Current.Activate();
      }


      ////todo:  You should add Microsoft Mobile Extension SDK (http://stackoverflow.com/questions/32725185/using-statusbar-in-windows-universal-app)
      //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
      //{
      //	var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
      //	statusBar.BackgroundColor = Windows.UI.Colors.Green;
      //	statusBar.BackgroundOpacity = 1;
      //}


      //<< Back Button                                                                              //tu: Back Button C:\gh\201505-MVA\KeepTheKeys\KeepTheKeys\App.xaml.cs
      Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;  //tu: Back Button C:\gh\201505-MVA\KeepTheKeys\KeepTheKeys\App.xaml.cs
    }
    /// <summary>
    /// Handles the back button press and navigates through the history of the root frame.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Details about the back button press.</param>
    void App_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
    {
      if (!(Window.Current.Content is Frame frame))
        return;

      BackPressed?.Invoke(sender, e);

      if (frame.CanGoBack && !e.Handled)
      {
        frame.GoBack();
        e.Handled = true;
      }
    }


    /// <summary>
    /// This event wraps SystemNavigationManager.BackRequested to ensure that any pages that
    /// want to override the default behavior can subscribe to this event to potentially
    /// handle the back button press a different way (e.g. dismissing dialogs).
    /// </summary>
    public event EventHandler<Windows.UI.Core.BackRequestedEventArgs> BackPressed;
    //>>





    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

    /// <summary>
    /// Invoked when application execution is being suspended.  Application state is saved
    /// without knowing whether the application will be terminated or resumed with the contents
    /// of memory still intact.
    /// </summary>
    /// <param name="sender">The source of the suspend request.</param>
    /// <param name="e">Details about the suspend request.</param>
    void OnSuspending(object sender, SuspendingEventArgs e)
    {
      var deferral = e.SuspendingOperation.GetDeferral();
      //TODO: Save application state and stop any background activity
      deferral.Complete();
    }
  }
}
