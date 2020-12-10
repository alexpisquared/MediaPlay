using AAV.Sys.Helpers;
using AsLink;
using System;
using System.Diagnostics;
//using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DDJ.Main
{
	public class InterceptKeys
	{
		public const int WH_KEYBOARD_LL = 13, WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101;
		public static LowLevelKeyboardProc _proc = HookCallback;
		public static IntPtr _hookID = IntPtr.Zero;

		public static void DoHook(LowLevelKeyboardProc proc) { _hookID = SetHook(proc); }		//public static void DoHook() { _hookID = SetHook(_proc); }
		public static void UnHook()
		{
			if (_hookID == IntPtr.Zero) return;

			UnhookWindowsHookEx(_hookID);
			_hookID = IntPtr.Zero;
		}

		static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

	public	delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			Bpr.BeepOk();
			if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
			{
				int vkCode = Marshal.ReadInt32(lParam);
				Debug.WriteLine(/*(Keys)*/vkCode, ">>>>>>>>>>");
			}
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern IntPtr GetModuleHandle(string lpModuleName);
	}
}

///		void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) { switch (args.VirtualKey) { case (VirtualKey)0xB3: Debug.WriteLine("VK_MEDIA_PLAY_PAUSE"); break; } }
///
