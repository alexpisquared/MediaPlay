using AAV.WPF.Helpers;
using DDJ.Main.ViewModels;
using DDJ.Main.Views;
using MVVM.Common;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DDJ.Main.Net5
{
  public partial class App : Application
  {

    protected override async void OnStartup(StartupEventArgs e)
    {
      ShutdownMode = ShutdownMode.OnExplicitShutdown;
      Application.Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
      EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox).SelectAll(); })); //tu: TextBox

      base.OnStartup(e);

#if __DEBUG
			new xPositionCircularSlider().ShowDialog(); // new KeyViewer().ShowDialog(); //			new AudioCompareMain().ShowDialog();
#else

      vmdl = DdjViewModel.RestoreState() ?? new DdjViewModel();

      var view = new MainPlayerView();

      vmdl.Init_AutoStartPlayer(view.wmp);

      var rv = BindableBaseViewModel.ShowModalMvvm(vmdl, view);

      await Task.Delay(1250);
#endif

      //InterceptKeys.UnHook();
      Application.Current.Shutdown();
    }
    DdjViewModel vmdl;

    protected override void OnExit(ExitEventArgs e) => vmdl.SaveState();
  }
}
