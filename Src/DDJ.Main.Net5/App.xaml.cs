using AAV.WPF.Helpers;
using DDJ.Main.ViewModels;
using DDJ.Main.Views;
using MVVM.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DDJ.Main.Net5
{
  public partial class App : Application
  {
    DdjViewModel _vm;

    protected override async void OnStartup(StartupEventArgs e)
    {
      ShutdownMode = ShutdownMode.OnExplicitShutdown;
      Application.Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
      EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox).SelectAll(); })); //tu: TextBox
                                                                                                                                                     //---SetupTracingOptions("DDJ.", new TraceSwitch("OnlyUsedWhenInConfig", "This is the trace for all               messages... but who cares?   See ScrSvr for a model.") { Level = TraceLevel.Verbose });

      base.OnStartup(e);

#if __DEBUG
			new xPositionCircularSlider().ShowDialog(); // new KeyViewer().ShowDialog(); //			new AudioCompareMain().ShowDialog();
#else



      _vm = new DdjViewModel();     //../todo: _vm = DdjViewModel.RestoreState(Settings.Default.DdjViewModel);

      var vw = new MainPlayerView();

      _vm.Init_AutoStartPlayer(vw.wmp);

      var rv = BindableBaseViewModel.ShowModalMvvm(_vm, vw);

      await Task.Delay(1250);
#endif

      //InterceptKeys.UnHook();
      Application.Current.Shutdown();
    }
    protected override void OnExit(ExitEventArgs e) => Debug.Write($"OnExit(ExitEventArgs e): {e}");//../todo: if (_vm != null) Settings.Default.DdjViewModel = _vm.SaveState();     //if (_vm.CurMediaUnit != null) Settings.Default.StartUpMedia = _vm.CurMediaUnit.PathFileExtOrg;//../Settings.Default.Save();
  }
}
