using AAV.Sys.Helpers;
using AsLink;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DDJ.Main.Views
{
    public partial class xPositionCircularSlider : Window
    {
        public xPositionCircularSlider()
        {
            InitializeComponent();
            KeyDown += (s, e) => { if (e.Key == Key.Escape) { Close(); /* messes up Cancel on save: App.Current.Shutdown();*/ } e.Handled = true; }; //tu:
                                                                                                                                                     //			MouseLeftButtonDown += (s, e) => { DragMove(); e.Handled = true; }; 

            DataContext = this;

            me1.Source = get1stWmv();
            Title = me1.Source.LocalPath;
        }
        Uri get1stWmv()
        {
            var videoFolder = Environment.MachineName == "ASUS2" ? @"D:\1\v\" : @"C:\1\v\";
            var firs0 = Directory.GetFiles(videoFolder, "*.*", SearchOption.AllDirectories);
            var first = Directory.GetFiles(videoFolder, "*.*", SearchOption.AllDirectories).FirstOrDefault(r =>
                r.ToLower().EndsWith(@".*") ||
                r.ToLower().EndsWith(@".avi") ||
                r.ToLower().EndsWith(@".mov") ||
                r.ToLower().EndsWith(@".mp3") ||
                r.ToLower().EndsWith(@".mp4") ||
                r.ToLower().EndsWith(@".wav") ||
                r.ToLower().EndsWith(@".wmv") ||
                r.ToLower().EndsWith(@".wma"));
            Title = first ?? "Notihng found";
            return first == null ? null : new Uri(first);
        }

        public TimeSpan PosTs { get { return (TimeSpan)GetValue(PosTProperty); } set { SetValue(PosTProperty, value); } }
        public static readonly DependencyProperty PosTProperty = DependencyProperty.Register("PosTs", typeof(TimeSpan), typeof(xPositionCircularSlider), new PropertyMetadata(TimeSpan.FromMinutes(7), new PropertyChangedCallback(moveMePosTS)));
        public double PosnS { get { return (double)GetValue(PosnProperty); } set { SetValue(PosnProperty, value); } }
        public static readonly DependencyProperty PosnProperty = DependencyProperty.Register("PosnS", typeof(double), typeof(xPositionCircularSlider), new PropertyMetadata(100d, new PropertyChangedCallback(moveMePosnS)));

        static TimeSpan _prevTs = TimeSpan.Zero;
        static void moveMePosTS(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _prevTs = (TimeSpan)e.NewValue;
            Task.Run(async () => await Task.Delay(1125)).ContinueWith(_ =>
            {
                if (_prevTs == (TimeSpan)e.NewValue)
                {
                    Bpr.BeepOk();
                    ((xPositionCircularSlider)d).me1.Position = (TimeSpan)e.NewValue;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        static double _prevSec = -1;
        async static void moveMePosnS(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _prevSec = (double)e.NewValue;
            await Task.Delay(125);
            if (_prevSec != (double)e.NewValue) return;

            Bpr.BeepOk();
            ((xPositionCircularSlider)d).me1.Position = TimeSpan.FromSeconds((double)e.NewValue);
        }
    }
}
