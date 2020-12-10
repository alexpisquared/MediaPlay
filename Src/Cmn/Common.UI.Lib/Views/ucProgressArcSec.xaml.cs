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
    public partial class ucProgressArcSec : UserControl
    {
        bool _isMouseDown = false;
        const int fsz = 22;

        public ucProgressArcSec()
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
            drawLineSec(e.GetPosition((IInputElement)sender));
            ln1.Visibility = Visibility.Visible;
        }
        void onMv(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            UIElement el = (UIElement)sender;
            if (el.IsMouseCaptured && _isMouseDown)
                drawLineSec(e.GetPosition((IInputElement)sender));
        }
        void onUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            ((UIElement)sender).ReleaseMouseCapture();
            _isMouseDown = false;
            ln1.Visibility = Visibility.Collapsed;
        }
        void drawLineSec(Point lcn)
        {
            ln1.X2 = lcn.X;
            ln1.Y2 = lcn.Y;

            var dx = ln1.X2 - ln1.X1;
            var dy = ln1.Y1 - ln1.Y2;
            var rd = Math.Sqrt(dx * dx + dy * dy);
            if (rd != 0)
            {
                var an = Math.Asin(dx / rd) * 2.0 / Math.PI; // Debug.WriteLine("{0:N2} / {1:N2} => {2:N2}°   Y2:{3:N1} Y1:{4:N1}", dx, rd, an, PrgPositSec, PrgDuratSec);

                if (dy >= 0 && dx > 0)              /**/
                    PrgPositSec = PrgDuratSec * an * .25;
                else if (dy < 0 && dx >= 0)     /**/
                    PrgPositSec = PrgDuratSec * (2 - an) * .25;
                else if (dy < 0 && dx < 0)      /**/
                    PrgPositSec = PrgDuratSec * (2 - an) * .25;
                else                                                    /**/
                    PrgPositSec = PrgDuratSec * (4 + an) * .25;

            }
        }

        static void recalcSec(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pa = d as ucProgressArcSec;
            pa.PrgPositSec = (double)e.NewValue;
            pa.arcAngle = (pa.PrgDuratSec <= 0) ? 0 : 360 * (pa.PrgPositSec <= 0 ? 0 : pa.PrgPositSec) / pa.PrgDuratSec;

            pa.ReplaceInlines();
            pa.DrawBookmarks();
        }
        static void drawBookmarks(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ucProgressArcSec pb = d as ucProgressArcSec;
            pb.DrawBookmarks();
        }
        public void ReplaceInlines()
        {
            //replace3Inlines(tb1.Inlines);
            replaceDoneInlines(tbDone.Inlines);
            replaceLeftInlines(tbLeft.Inlines);
            replaceTotlInlines(tbTotl.Inlines);
        }
        void replaceDoneInlines(InlineCollection ic) { ic.Clear(); dualFontSize(ic, fsz, TimeSpan.FromSeconds(PrgPositSec)); }
        void replaceLeftInlines(InlineCollection ic) { ic.Clear(); dualFontSize(ic, fsz, TimeSpan.FromSeconds(PrgDuratSec) - TimeSpan.FromSeconds(PrgPositSec)); }
        void replaceTotlInlines(InlineCollection ic) { ic.Clear(); dualFontSize(ic, fsz, TimeSpan.FromSeconds(PrgDuratSec)); }
        void replace3Inlines(InlineCollection ic)
        {
            ic.Clear();

            dualFontSize(ic, fsz, TimeSpan.FromSeconds(PrgPositSec));
            ic.Add((new Run() { Text = " · " }));
            dualFontSize(ic, fsz, TimeSpan.FromSeconds(PrgDuratSec) - TimeSpan.FromSeconds(PrgPositSec));
            ic.Add((new Run() { Text = "\n", FontSize = 1 }));
            dualFontSize(ic, fsz, TimeSpan.FromSeconds(PrgDuratSec));
        }
        void dualFontSize(InlineCollection ic, int fsz, TimeSpan ts)
        {
            try
            {
                if (ts.TotalMinutes > 60)
                    ic.Add((new Run($@"{ts:h\:mm}")));
                else
                    ic.Add((new Run($"{ts.Minutes}")));
                ic.Add((new Run() { Text = $":{ts:ss}", FontSize = fsz }));
            }
            catch (Exception ex) { Trace.WriteLine(ex, System.Reflection.MethodInfo.GetCurrentMethod().ToString()); }
        }

        double arcAngle { get { return (double)GetValue(AngleProperty); } set { SetValue(AngleProperty, value); } }
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("PrgsAngle", typeof(double), typeof(ucProgressArcSec), new PropertyMetadata(270.0));

        //public static int GetAp(DependencyObject obj) { return (int)obj.GetValue(ApProperty); }
        //public static void SetAp(DependencyObject obj, int value) { obj.SetValue(ApProperty, value); }
        //public static readonly DependencyProperty ApProperty = DependencyProperty.RegisterAttached("Ap", typeof(int), typeof(ownerclass), new PropertyMetadata(0));

        //public int Dp { get { return (int)GetValue(DpProperty); } set { SetValue(DpProperty, value); } }
        //public static readonly DependencyProperty DpProperty = DependencyProperty.Register("Dp", typeof(int), typeof(ownerclass), new PropertyMetadata(0));


        public double PrgDuratSec { get { return (double)GetValue(DuratSecProperty); } set { SetValue(DuratSecProperty, value); } }
        public static readonly DependencyProperty DuratSecProperty = DependencyProperty.Register("PrgDuratSec", typeof(double), typeof(ucProgressArcSec), new PropertyMetadata(-.01));
        public double PrgPositSec { get { return (double)GetValue(PositSecProperty); } set { SetValue(PositSecProperty, value); } }
        public static readonly DependencyProperty PositSecProperty = DependencyProperty.Register("PrgPositSec", typeof(double), typeof(ucProgressArcSec), new PropertyMetadata(-.01, new PropertyChangedCallback(recalcSec)));

        public ObservableCollection<MuBookmark> BookMarks { get { return (ObservableCollection<MuBookmark>)GetValue(BookMarksProperty); } set { SetValue(BookMarksProperty, value); DrawBookmarks(); } }
        public static readonly DependencyProperty BookMarksProperty = DependencyProperty.Register("BookMarks", typeof(ObservableCollection<MuBookmark>), typeof(ucProgressArcSec), new PropertyMetadata(null, new PropertyChangedCallback(drawBookmarks)));

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
            if (TimeSpan.FromSeconds(PrgPositSec) == null || TimeSpan.FromSeconds(PrgPositSec).TotalSeconds == 0) return;
            if (position.PositionSec == 0) return;

            var angle = 2.0 * Math.PI * (position.PositionSec / TimeSpan.FromSeconds(PrgPositSec).TotalSeconds - .25);
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
