using AsLink;
using DDJ.Main.Properties;
using DDJ.Main.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace DDJ.Main.Views
{
    public partial class MainPlayerView : Window
  {
    InterceptKeys.LowLevelKeyboardProc _hookCallback;

    public MainPlayerView()
    {
      InitializeComponent();

      AppSettings.RestoreSizePosition(this, Settings.Default.AppSettings);

      InterceptKeys.DoHook(_hookCallback = HookCallback);
    }
    ~MainPlayerView() { InterceptKeys.UnHook(); }
    IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode >= 0)
        if (wParam == (IntPtr)InterceptKeys.WM_KEYDOWN)
        {
          int vkCode = Marshal.ReadInt32(lParam); //					tb3.Text = string.Format("{0}  {1}  {2}  DN", nCode, wParam, vkCode);
        }
        else if (wParam == (IntPtr)InterceptKeys.WM_KEYUP)
        {
          if (Marshal.ReadInt32(lParam) == 179)
          {
            var vm = (DdjViewModel)DataContext;
            if (vm.TglPlayPausACmd.CanExecute(null))
              vm.TglPlayPauseCmd.Execute(null);
          }
        }

      PreviewKeyUp += (s, e) => { if (e.Key == Key.Escape) { Close(); /* messes up Cancel on save: App.Current.Shutdown();*/ } e.Handled = true; }; //tu: !KeyDown - blocks all keystrokes!!!!!!! ...probably because of e.Handled = true; 
      MouseLeftButtonDown += (s, e) => { DragMove(); e.Handled = true; }; 

      return InterceptKeys.CallNextHookEx(InterceptKeys._hookID, nCode, wParam, lParam);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);
      Settings.Default.AppSettings = AppSettings.SaveSizePosition(this, Settings.Default.AppSettings);
      Settings.Default.Save();
    }

    void onClick1(object sender, RoutedEventArgs e) { new KeyViewer().Show(); }
    void onClick2(object sender, RoutedEventArgs e) { new xPositionCircularSlider().ShowDialog(); bFcs.Focus(); }
    void onClick3(object sender, RoutedEventArgs e) { Title = $"{FocusManager.GetFocusedElement(this)} - {Keyboard.FocusedElement}"; }
  }
}
