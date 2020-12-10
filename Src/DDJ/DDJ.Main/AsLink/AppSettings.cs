using System;
using System.Windows;

namespace AsLink
{
    public partial class AppSettings
    {
        public int IValue { get; set; }

        public static string SaveSizePosition(Window w, string appStngs)
        {
            try
            {
                var stgs = (string.IsNullOrEmpty(appStngs) || !(Serializer.LoadFromString<AppSettings>(appStngs) is AppSettings)) ? new AppSettings() : Serializer.LoadFromString<AppSettings>(appStngs) as AppSettings;
                stgs.Window1.windowTop = w.Top;
                stgs.Window1.windowLeft = w.Left;
                stgs.Window1.windowWidth = w.Width;
                stgs.Window1.windowHeight = w.Height;
                return Serializer.SaveToString(stgs);
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; } // return Serializer.SaveToString(new AppSettings());
        }
        public static void RestoreSizePosition(Window w, string appStngs)
        {
            try
            {
                if (!string.IsNullOrEmpty(appStngs))
                {
                    if (Serializer.LoadFromString<AppSettings>(appStngs) is AppSettings stgs)
                    {
                        w.WindowStartupLocation = WindowStartupLocation.Manual;
                        w.Top = stgs.Window1.windowTop;
                        w.Left = stgs.Window1.windowLeft;
                        w.Width = stgs.Window1.windowWidth;
                        w.Height = stgs.Window1.windowHeight;
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
        }

        public WindowPlace Window1 { get { return window1; } set { window1 = value; } }
        WindowPlace window1 = new WindowPlace();
        public WindowPlace Window2 { get { return window2; } set { window2 = value; } }
        WindowPlace window2 = new WindowPlace();
        public WindowPlace Window3 { get { return window3; } set { window3 = value; } }
        WindowPlace window3 = new WindowPlace();
    }

    public class WindowPlace
    {
        public double windowTop = 600;
        public double windowLeft = 10;
        public double windowWidth = 900;
        public double windowHeight = 440;
    }
}
