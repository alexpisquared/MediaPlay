//using BOM.OLP.Common.Interfaces;
//using Microsoft.Win32;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BOM.OLP.Common.Interfaces
//{
//  public interface ISysLogger
//  {
//    void LogSessionStart();
//    void LogSessionEnd();
//    void LogException(Exception ex, MethodBase mb = null);
//    void LogMessage(string message, string logReason = "Info.");
//  }
//}


//namespace AsLink
//{
//  [Obsolete("Repplace " +
//            "DevOp.CurVer   with VerHelper.CurVer" +
//            "DevOp.OneDrive with OneDrive." +
//            "DevOp.Beep     with Bpr.Beep" +
//            "")]
//  public static partial class DevOp
//  {
//    public static string CurVerStr(string pls = null)
//    {
//      var clng = new FileInfo(Assembly.GetCallingAssembly().Location).LastWriteTime;
//      var excg = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
//      var max = clng > excg ? clng : excg;
//      var dt = (DateTime.Now - max);
//      if (dt.TotalSeconds < 10) return $"Now!!! {pls ?? CompileMode}";
//      if (dt.TotalSeconds < 100) return $"{dt.TotalSeconds:N0} seconds ago {pls ?? CompileMode}";
//      if (dt.TotalMinutes < 60) return $"{dt.TotalMinutes:N0} minutes ago {pls ?? CompileMode}";
//      if (dt.TotalHours < 24) return $"{dt.TotalHours:N1} hours ago {pls ?? CompileMode}";
//      if (dt.TotalDays < 10) return $"{dt.TotalDays:N1} days ago {pls ?? CompileMode}";

//      return $"{max:yyyy.M.d}.{pls ?? CompileMode}";
//    }
//    public static string CurVerStr_(string pls) { return $"{CurVer}.{pls}"; }
//    public static Version CurVer
//    {
//      get
//      {
//        return
//#if usingSystemDeploymentApplication //  <== define for Release
// System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed ?
//          System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion : // z.ves.: ClickOnce deployed.
//#endif
// Assembly.GetEntryAssembly().GetName().Version;       // BOM.OLP.DAQ -.EXE !!!
//                                                      //          Assembly.GetCallingAssembly().GetName().Version;		// BOM.OLP.ViewModel.DAQ - use this as the most likely to change, thus be reflective of the latest state.
//                                                      //          Assembly.GetExecutingAssembly().GetName().Version;	// BOM.OLP.Common
//                                                      // FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("MM.dd HH"); 
//      }
//    }

//    public static string CurVerStr(object sqlEnv)
//    {
//      throw new NotImplementedException();
//    }

//    public static string CompileMode
//    {
//      get
//      {
//#if DEBUG
//        return Debugger.IsAttached ? "Dbg-Atchd" : "Dbg";
//#else
//        return Debugger.IsAttached ? "Rls-Atchd" : "Rls";
//#endif
//      }
//    }

//    public static ISysLogger SysLogger = null;

//    public static bool IsVIP
//    {
//      get
//      {
//        var un = Environment.UserName.ToLower();
//        return un.Contains("zzz") || un.Contains("pigid") || un.Contains("lex");
//      }
//    }
//    public static string ExHrT(Exception ex, MethodBase mb = null, string param = "")
//    {

//      var msg4fs = GetErrMsg4Fs(GetInnermostException(ex), mb, param);

//      try
//      {

//#if RAD_DEBUG
//        System.Windows.Clipboard.SetText(msg);				
//#endif

//#if DEBUG
//        log(ex, mb, param, msg4fs);
//#else
//        Task.Run(() => log(ex, mb, param, msg4fs));
//#endif

//        if (Debugger.IsAttached) Debugger.Break(); else BeepEr();
//      }
//      catch (Exception fatalEx) { Environment.FailFast("An error occured whilst reporting an error.", fatalEx); }//tu: http://blog.functionalfun.net/2013/05/how-to-debug-silent-crashes-in-net.html // Capturing dump files with Windows Error Reporting: Create a key at HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\Windows Error Reporting\LocalDumps\[Your Application Exe FileName]. In that key, create a string value called DumpFolder, and set it to the folder where you want dumps to be written. Then create a DWORD value called DumpType with a value of 2.

//      return msg4fs;
//    }

//    static void log(Exception ex, MethodBase mb, string param, string msg4fs)
//    {
//      Trace.WriteLine(msg4fs);

//      if (SysLogger != null)
//        SysLogger.LogMessage(GetErrMsg4Db(GetInnermostException(ex), mb, param), "Exception");
//    }

//    public static string GetErrMsg4Db(Exception ex, MethodBase mb, string param = "") { return string.Format("{0}.{1}({5}): \r\n\r\n{2} \r\n\r\nSource: \r\n{3} \r\n\r\nStack: \r\n{4}", mb?.DeclaringType.Name, mb?.Name, ex.Message, ex.Source, ex.StackTrace, param); }
//    public static string GetErrMsg4Fs(Exception ex, MethodBase mb, string param = "") { return string.Format("\r\n\r\n///{6}\r\n{0}  {1}.{2}({7}):\r\n\r\n{3}\r\n\r\nSource: \r\n{4}\r\n\r\nStack: \r\n{5}\r\n\r\n\\\\\\{6}\r\n\r\n", DateTime.Now.ToString("MMM-dd HH:mm"), mb?.DeclaringType?.Name, mb?.Name, ex.Message, ex.Source, ex.StackTrace, new string('═', 110), param); }

//    public static Exception GetInnermostException(Exception ex) { return ex.InnerException != null ? GetInnermostException(ex.InnerException) : ex; }

//    public static string LocalAppData
//    {
//      get
//      {
//        var rv = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "Local AppData", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local"));
//        if (rv == null) rv = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Explorer\Shell Folders", "Local AppData", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local"));
//        return rv != null ? rv.ToString() : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local"); // Sep 25, 2014 ... feels like there is a copy with this fix already: use compare to find out which one is the best.
//      }
//    }

//    public static bool BeepAllowed
//    {
//      get
//      {
//        if (_BeepAllowed == null)
//          _BeepAllowed = string.CompareOrdinal(Environment.UserDomainName, "ABSCIEXDEV") != 0;

//        return _BeepAllowed.Value;
//      }
//      set { _BeepAllowed = value; }
//    }

//    public static string LocalAppDataFolder(string subFolder) { return Path.Combine(LocalAppData, subFolder); }

//    //<< Beep
//    const int mind = 41, medd = 50; // minimal and medium durations on Dell 990.
//    static double K = .95;
//    static int N = 9, D = mind, F = 9000;
//    static bool? _BeepAllowed;

//    public static void BeepOk() { BeepFD(10500, medd); }
//    public static void BeepOkB() { BeepFD(9300, medd); }
//    public static void BeepClk() { BeepFD(10000, medd); }
//    public static void BeepShort() { BeepFD(11000, mind); }
//    public static void BeepHi() { BeepFD(12000, mind); }
//    public static void BeepLong() { BeepFD(8000, 100); }
//    public static void BeepNo() { BeepFD(4000, mind); BeepFD(4000, mind); }
//    public static void Beep1of2() { BeepFD(9100, medd); BeepFD(9900, mind); }
//    public static void Beep2of2() { BeepFD(9900, mind); BeepFD(8100, medd); }
//    public static void BeepEnd6() { BeepFD(7700, mind); BeepFD(6000, mind); BeepFD(5000, mind); BeepFD(4000, 100); BeepFD(7700, 100); BeepFD(7700, 100); }
//    public static void BeepN(int n) { for (int i = 0; i < n; i++) BeepFD(9999, 22); }
//    public static void BeepN(int f, int d, int n, int df) { for (int i = 0; i < n; i++) BeepFD(f += df, d); }
//    public static void BeepFDNK(int f = 3000, int d = 70, int n = 3, double k = .04) { for (int i = 0; i < n; i++) BeepFD(f = ((int)(f * (1.0 + k))), d); F = f; D = d; N = n; K = k; }
//    public static void BeepFDNKAsy(int f = 3000, int d = 70, int n = 3, double k = .04) { Task.Run(() => BeepFDNK(f, d, n, k)); }
//    public static void BeepDone() { for (int i = 0; i < N; i++) BeepFD(F = ((int)(F * (1.0 - K))), D); }

//    public static void BeepFD(int freq, int dur) { Beep_(freq, dur); }
//    public static void BeepEr()
//    {
//#if !!!QUIET
//      Task.Run(() =>
//      {
//        Beep_(6000, 40);
//        Beep_(6000, 40);
//        Beep_(6000, 40);
//        Beep_(5000, 80);
//      });
//#else
//      BeepFD(10000, 25);
//#endif
//    }
//    public static void BeepSmallError()                         /**/
//    { for (double i = 8000; i < 9000; i *= 1.141) Beep_((int)i, 12); }
//    public static void BeepBigError()                           /**/
//    { for (double i = 1000; i < 5000; i *= 1.5) Beep_((int)i, (int)(10000 / i)); }
//    public static void BeepFinish()                             /**/
//    { for (double i = 300; i > 240; i *= 0.9) Beep_((int)i, (int)(1000 / i)); }
//    static void dbgSleepAndBeep(ref int cummulativeDelay, bool final = false)
//    {
//      Thread.Sleep(cummulativeDelay += 500);
//      if (final) { Beep_(9900, 40); Beep_(9000, 40); Beep_(81000, 140); }
//      else { Beep_(9000, 40); Beep_(9900, 40); }
//    }
//    public static void BeepTestPlayAll()
//    {
//      BeepShort();
//      BeepOk();
//      BeepLong();
//      BeepOkB();
//      Beep1of2();
//      Beep2of2();
//      BeepEr();
//    }

//    public static void Beep_(int freq, int dur)
//    {
//      if (BeepAllowed)
//        Beep(freq, dur);
//      //else
//      //	Thread.Sleep(dur);
//    }

//    [System.Runtime.InteropServices.DllImport("kernel32.dll")] public static extern bool Beep(int freq, int dur); // public static void Beep(int freq, int dur) { }    // 

//    //>>
//  }
//}