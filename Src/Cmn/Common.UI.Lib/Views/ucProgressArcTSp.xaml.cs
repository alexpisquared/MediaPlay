using Common.UI.Lib.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Common.UI.Lib.Views
{
    public partial class ucProgressArcTSp : UserControl
    {
        bool _isMouseDown = false;

        public ucProgressArcTSp()
        {
            InitializeComponent();
            //BookMarks = new ObservableCollection<MuBookmark>();
            MouseLeftButtonDown += onDn;
            MouseLeftButtonUp += onUp;
            MouseMove += onMv;
        }

        void onDn(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var el = (UIElement)sender; //			_point = e.MouseDevice.GetPosition(el);
            el.CaptureMouse();
            _isMouseDown = true;

            ln.Visibility = Visibility.Visible;

            drawLineTSp(sender, e);
        }
        void onMv(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (((UIElement)sender).IsMouseCaptured && _isMouseDown)
                drawLineTSp(sender, e);
        }
        void onUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            ((UIElement)sender).ReleaseMouseCapture();
            _isMouseDown = false;
            ln.Visibility = Visibility.Collapsed;
        }
        void drawLineTSp(object sender, MouseEventArgs e)
        {
            double x = e.GetPosition((IInputElement)sender).X;
            double y = e.GetPosition((IInputElement)sender).Y;

            ln.X2 = x;
            ln.Y2 = y;

            var dx = ln.X2 - ln.X1;
            var dy = ln.Y1 - ln.Y2;
            var rd = Math.Sqrt(dx * dx + dy * dy);
            if (rd != 0)
            {
                var an = Math.Asin(dx / rd) * 2.0 / Math.PI;
                Debug.WriteLine("{0:N2} / {1:N2} => {2:N2}°   Y2:{3} Y1:{4}", dx, rd, an, PrgPosition, PrgDuration);

                if (dy >= 0 && dx > 0)                      /**/
                    PrgPosition = TimeSpan.FromSeconds(PrgDuration.TotalSeconds * an * .25);
                else if (dy < 0 && dx >= 0)             /**/
                    PrgPosition = TimeSpan.FromSeconds(PrgDuration.TotalSeconds * (2 - an) * .25);
                else if (dy < 0 && dx < 0)              /**/
                    PrgPosition = TimeSpan.FromSeconds(PrgDuration.TotalSeconds * (2 - an) * .25);
                else                                                            /**/
                    PrgPosition = TimeSpan.FromSeconds(PrgDuration.TotalSeconds * (4 + an) * .25);
            }
        }

        static void recalcTs(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pa = d as ucProgressArcTSp;
            //Debug.WriteLine("::UC> {0} ==> {1}", pa.PrgPosition , (TimeSpan)e.NewValue);
            pa.PrgPosition = (TimeSpan)e.NewValue;
            pa.arcAngle = (pa.PrgDuration.TotalSeconds <= 0) ? 0 : 360 * (pa.PrgPosition == null ? 0 : pa.PrgPosition.TotalSeconds) / pa.PrgDuration.TotalSeconds;

            pa.ReplaceInlines();
            pa.DrawBookmarks();
        }
        static void drawBookmarks(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ucProgressArcTSp pb = d as ucProgressArcTSp;
            pb.DrawBookmarks();
        }
        public void ReplaceInlines()
        {
            tbDone.Text = PrgPosition.TotalHours > 1 ? $"{PrgPosition:h\\:mm\\.ss}" : $"{PrgPosition:m\\.ss}";
            var left = PrgDuration - PrgPosition;
            tbLeft.Text = 
                left.TotalHours > 1 ? $"{left:h\\:mm\\.ss}" :
                left.TotalMinutes > 10 ? $"  {left:mm\\.ss}" : 
                $"   {left:m\\.ss}";
            tbTotl.Text = $"{PrgDuration:h\\:mm\\.ss}";

            ////replaceInlines(tb1.Inlines);
            //replaceDoneInlines(tbDone.Inlines);
            //replaceLeftInlines(tbLeft.Inlines);
            //replaceTotlInlines(tbTotl.Inlines);
        }
        const int fsz = 22;
        void replaceDoneInlines(InlineCollection ic) { ic.Clear(); dualFontSize(ic, fsz, (PrgPosition)); }
        void replaceLeftInlines(InlineCollection ic) { ic.Clear(); dualFontSize(ic, fsz, (PrgDuration - PrgPosition)); }
        void replaceTotlInlines(InlineCollection ic) { ic.Clear(); dualFontSize(ic, fsz, (PrgDuration)); }
        void replaceInlines(InlineCollection ic) { ic.Clear(); dualFontSize(ic, fsz, PrgPosition); ic.Add((new Run() { Text = " · " })); dualFontSize(ic, fsz, PrgDuration - PrgPosition); ic.Add((new Run() { Text = "\n", FontSize = 1 })); dualFontSize(ic, fsz, PrgDuration); }

        void dualFontSize(InlineCollection ic, int fsz, TimeSpan ts)
        {
            try
            {
                if (ts.TotalMinutes > 60)
                    ic.Add((new Run($@"{ts:h\:mm}")));
                else
                    ic.Add((new Run($"   {ts.Minutes}")));

                ic.Add((new Run() { Text = $".{ts:ss}", FontSize = fsz }));
            }
            catch (Exception ex) { Trace.WriteLine(ex, System.Reflection.MethodInfo.GetCurrentMethod().ToString()); }
        }

        double arcAngle { get { return (double)GetValue(AngleProperty); } set { SetValue(AngleProperty, value); } }
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("PrgsAngle", typeof(double), typeof(ucProgressArcTSp), new PropertyMetadata(270.0));

        public TimeSpan PrgDuration { get { return (TimeSpan)GetValue(DurationProperty); } set { SetValue(DurationProperty, value); } }
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("PrgDuration", typeof(TimeSpan), typeof(ucProgressArcTSp), new PropertyMetadata(TimeSpan.Zero));
        public TimeSpan PrgPosition { get { return (TimeSpan)GetValue(PositionProperty); } set { SetValue(PositionProperty, value); } }
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("PrgPosition", typeof(TimeSpan), typeof(ucProgressArcTSp), new PropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(recalcTs)));

        public ObservableCollection<MuBookmark> BookMarks { get { return (ObservableCollection<MuBookmark>)GetValue(BookMarksProperty); } set { SetValue(BookMarksProperty, value); DrawBookmarks(); } }
        public static readonly DependencyProperty BookMarksProperty = DependencyProperty.Register("BookMarks", typeof(ObservableCollection<MuBookmark>), typeof(ucProgressArcTSp), new PropertyMetadata(null, new PropertyChangedCallback(drawBookmarks)));

        public void DrawBookmarks()
        {
            //g1.Children.Close() =>
            //var designerItems = this.Children.OfType<Line>();

            //?g1.Children.OfType<Line>().ForEach(line => g1.Children.Remove(line));

            restart:
            foreach (UIElement line in g1.Children)
            {
                if (line is Line) { g1.Children.Remove(line); goto restart; }
            }

            if (BookMarks == null) return;

            foreach (var bm in BookMarks) DrawBookmark(bm);
            //BookMarks.ToList().ForEach(b => DrawBookmark(b));
            //BookMarks.ToList().ForEach(DrawBookmark);
        }
        public void DrawBookmark(MuBookmark position)
        {
            if (PrgDuration == null || PrgDuration.TotalSeconds == 0) return;
            if (position.PositionSec == 0) return;

            var angle = 2.0 * Math.PI * (position.PositionSec / PrgDuration.TotalSeconds - .25);
            var radius = 0.5 * g1.Width;
            g1.Children.Add(new Line
            {
                X1 = radius * (1 + Math.Cos(angle) * .5),
                Y1 = radius * (1 + Math.Sin(angle) * .5),
                X2 = radius * (1 + Math.Cos(angle)),
                Y2 = radius * (1 + Math.Sin(angle)),
                Fill = new SolidColorBrush(Colors.Red),
                Stroke = new SolidColorBrush(Colors.LimeGreen),
                StrokeThickness = 1
            });
        }
    }
}
