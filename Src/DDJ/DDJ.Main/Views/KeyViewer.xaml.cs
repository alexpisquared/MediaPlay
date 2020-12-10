using AAV.Sys.Helpers;
using AsLink;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace DDJ.Main.Views
{
    public partial class KeyViewer : Window
	{
		InterceptKeys.LowLevelKeyboardProc _hookCallback;

		public KeyViewer()
		{
			InitializeComponent();
			KeyDown += (s, e) => { if (e.Key == Key.Escape) { Close(); /* messes up Cancel on save: App.Current.Shutdown();*/ } e.Handled = true; }; //tu:
			MouseLeftButtonDown += (s, e) => { DragMove(); e.Handled = true; }; 

			PreviewKeyDown += KeyViewer_PreviewKeyDn;
			PreviewKeyUp += KeyViewer_PreviewKeyUp;

			InterceptKeys.DoHook(_hookCallback = HookCallback);
		}
		~KeyViewer() { InterceptKeys.UnHook(); }

		void KeyViewer_PreviewKeyUp(object sender, KeyEventArgs e) { tb2.Text = $"{e.Key}  {e.SystemKey}"; }
		void KeyViewer_PreviewKeyDn(object sender, KeyEventArgs e) { tb1.Text = $"{e.Key}  {e.SystemKey}"; }

		IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			Bpr.BeepOk();

			//tb3.Text = string.Format("{0}  {1}", nCode, wParam);
			if (nCode >= 0)
				if (wParam == (IntPtr)InterceptKeys.WM_KEYDOWN)
				{
					int vkCode = Marshal.ReadInt32(lParam);
					tb3.Text = $"{nCode}  {wParam}  {vkCode}  DN";
					Debug.WriteLine(/*(Keys)*/vkCode, "DN >>>>>>");
				}
				else if (wParam == (IntPtr)InterceptKeys.WM_KEYUP)
				{
					int vkCode = Marshal.ReadInt32(lParam);
					tb4.Text = $"{nCode}  {wParam}  {vkCode}  UP";
					Debug.WriteLine(/*(Keys)*/vkCode, "UP >>>>>>");
				}

			KeyDown += (s, e) => { if (e.Key == Key.Escape) { Close(); /* messes up Cancel on save: App.Current.Shutdown();*/ } e.Handled = true; }; //tu:
			MouseLeftButtonDown += (s, e) => { DragMove(); e.Handled = true; }; 

			return InterceptKeys.CallNextHookEx(InterceptKeys._hookID, nCode, wParam, lParam);
		}
	}
}
