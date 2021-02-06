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
    [Obsolete("Use TSp or Sec", true)]
	public partial class ucProgressArc : UserControl
	{
		bool _isMouseDown = false;
		double x0, y0;

		public ucProgressArc()
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
			x0 = e.GetPosition((IInputElement)sender).X;
			y0 = e.GetPosition((IInputElement)sender).Y;
			//v0 = Val;
			ln.Visibility = Visibility.Visible;
		}
		void onMv(object sender, MouseEventArgs e)
		{
			e.Handled = true;

			UIElement el = (UIElement)sender;
			if (el.IsMouseCaptured && _isMouseDown)
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
					Debug.WriteLine("{0:N2} / {1:N2} => {2:N2}°   Y2:{3:N1} Y1:{4:N1}", dx, rd, an, PrgPositSec, PrgDuratSec);

					if (dy > 0 && dx > 0)
						PrgPositSec = PrgDuratSec * an * .25;
					else if (dy < 0 && dx > 0)
						PrgPositSec = PrgDuratSec * (2 - an) * .25;
					else if (dy < 0 && dx < 0)
						PrgPositSec = PrgDuratSec * (2 - an) * .25;
					else
						PrgPositSec = PrgDuratSec * (4 + an) * .25;

				}
			}
		}
		void onUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			((UIElement)sender).ReleaseMouseCapture();
			_isMouseDown = false;
			ln.Visibility = Visibility.Collapsed;
		}

		static void recalcSec(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var pa = d as ucProgressArc;
			pa.PrgPositSec = (double)e.NewValue;
			pa.arcAngle = (pa.PrgDuratSec <= 0) ? 0 : 360 * (pa.PrgPositSec <= 0 ? 0 : pa.PrgPositSec) / pa.PrgDuratSec;

			pa.ReplaceInlines();
			pa.DrawBookmarks();
		}
		static void recalcTs(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var pa = d as ucProgressArc;
			pa.PrgPosition = (TimeSpan)e.NewValue;
			pa.arcAngle = (pa.PrgDuration.TotalSeconds <= 0) ? 0 : 360 * (pa.PrgPosition == null ? 0 : pa.PrgPosition.TotalSeconds) / pa.PrgDuration.TotalSeconds;

			pa.ReplaceInlines();
			pa.DrawBookmarks();
		}
		static void drawBookmarks(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ucProgressArc pb = d as ucProgressArc;
			pb.DrawBookmarks();
		}
		public void ReplaceInlines()
		{
			try { ReplaceInlines(tb1.Inlines); }
			catch (Exception ex) { Trace.WriteLine(ex, System.Reflection.MethodInfo.GetCurrentMethod().ToString());  }
		}
		public void ReplaceInlines(InlineCollection ic)
		{
			const int fsz = 22;
			ic.Clear();
			//string s = string.Format(@"{0:h\:mm}<{1:h\:mm}", (StartPos), ts);

			dualFontSize(ic, fsz, PrgPosition);
			ic.Add((new Run() { Text = " · " }));
			dualFontSize(ic, fsz, PrgDuration - PrgPosition);
			ic.Add((new Run() { Text = "\n", FontSize = 1 }));
			dualFontSize(ic, fsz, PrgDuration);
		}
		void dualFontSize(InlineCollection ic, int fsz, TimeSpan ts)
		{
			try
			{
				if (ts.TotalMinutes > 60)
					ic.Add((new Run($@"{ts:h\:mm}")));
				else
					ic.Add((new Run($"{ts.Minutes}")));
				ic.Add((new Run() { Text = $".{ts:ss}", FontSize = fsz }));
			}
			catch (Exception ex) { Trace.WriteLine(ex, System.Reflection.MethodInfo.GetCurrentMethod().ToString());  }
		}

		double arcAngle { get { return (double)GetValue(AngleProperty); } set { SetValue(AngleProperty, value); } }								public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("PrgsAngle", typeof(double), typeof(ucProgressArc), new PropertyMetadata(270.0));

		//public static int GetAp(DependencyObject obj) { return (int)obj.GetValue(ApProperty); }
		//public static void SetAp(DependencyObject obj, int value) { obj.SetValue(ApProperty, value); }
		//public static readonly DependencyProperty ApProperty = DependencyProperty.RegisterAttached("Ap", typeof(int), typeof(ownerclass), new PropertyMetadata(0));

		//public int Dp { get { return (int)GetValue(DpProperty); } set { SetValue(DpProperty, value); } }
		//public static readonly DependencyProperty DpProperty = DependencyProperty.Register("Dp", typeof(int), typeof(ownerclass), new PropertyMetadata(0));


		public TimeSpan PrgDuration { get { return (TimeSpan)GetValue(DurationProperty); } set { SetValue(DurationProperty, value); } }		public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("PrgDuration", typeof(TimeSpan), typeof(ucProgressArc), new PropertyMetadata(TimeSpan.Zero));
		public TimeSpan PrgPosition { get { return (TimeSpan)GetValue(PositionProperty); } set { SetValue(PositionProperty, value); } }		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("PrgPosition", typeof(TimeSpan), typeof(ucProgressArc), new PropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(recalcTs)));

		public double PrgDuratSec { get { return (double)GetValue(DuratSecProperty); } set { SetValue(DuratSecProperty, value); } }		public static readonly DependencyProperty DuratSecProperty = DependencyProperty.Register("PrgDuratSec", typeof(double), typeof(ucProgressArc), new PropertyMetadata(-.01));
		public double PrgPositSec { get { return (double)GetValue(PositSecProperty); } set { SetValue(PositSecProperty, value); } }		public static readonly DependencyProperty PositSecProperty = DependencyProperty.Register("PrgPositSec", typeof(double), typeof(ucProgressArc), new PropertyMetadata(-.01, new PropertyChangedCallback(recalcSec)));

		public ObservableCollection<MuBookmark> BookMarks { get { return (ObservableCollection<MuBookmark>)GetValue(BookMarksProperty); } set { SetValue(BookMarksProperty, value); DrawBookmarks(); } }		public static readonly DependencyProperty BookMarksProperty = DependencyProperty.Register("BookMarks", typeof(ObservableCollection<MuBookmark>), typeof(ucProgressArc), new PropertyMetadata(null, new PropertyChangedCallback(drawBookmarks)));

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
