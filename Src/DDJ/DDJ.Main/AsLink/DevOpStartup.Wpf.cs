using AAV.Sys.Ext;
using System;
using System.Diagnostics;
using System.Windows;

namespace AsLink
{
  public partial class DevOpStartup
  {
    [Obsolete(@"Use C:\C\AavCore\AAV.WPF\Helpers\UnhandledExceptionHndlr.cs", false)]
    public static void OnCurrentDispatcherUnhandledException(object s, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs ex)
    {
      ex.Handled = true;

      try
      {
        var msg = ex.Exception.InnerMessages();
        Trace.Write($"{DateTime.Now:yy.MM.dd HH:mm:ss}> CurrentDispatcherUnhandledException: s: {s.GetType().Name}. {msg}");
        Clipboard.SetText(msg);
#if Speakable
        new System.Speech.Synthesis.SpeechSynthesizer().SpeakAsync($"Oopsee... {imex.Message}");
#endif
        if (Debugger.IsAttached) Debugger.Break();              //seems like always true: if (ex is System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)					Bpr.BeepEr();				else 
        else
        if (MessageBox.Show($"An error occurred in this app...\n\n ...{msg}\n\nDo you want to continue?", "Current Dispatcher Unhandled Exception", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes) == MessageBoxResult.No)
        {
          Trace.WriteLine("Decided NOT to continue: Application.Current.Shutdown();");
          Application.Current.Shutdown();
        }
      }
      catch (Exception fatalEx)
      {
        Environment.FailFast("An error occured whilst reporting an error.", fatalEx); //tu: http://blog.functionalfun.net/2013/05/how-to-debug-silent-crashes-in-net.html // Capturing dump files with Windows Error Reporting: Db a key at HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\Windows Error Reporting\LocalDumps\[Your Application Exe FileName]. In that key, create a string value called DumpFolder, and set it to the folder where you want dumps to be written. Then create a DWORD value called DumpType with a value of 2.

        MessageBox.Show($"An error occurred in this app...\n\n ...{fatalEx.Message}");
      }
    }
  }
}
