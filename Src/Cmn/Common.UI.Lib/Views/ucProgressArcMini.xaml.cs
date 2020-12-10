using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Common.UI.Lib.Views
{
    public partial class ucProgressArcMini : UserControl
	{
		public ucProgressArcMini()
		{
			InitializeComponent();
		}

		static void recalc(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ucProgressArcMini pb = d as ucProgressArcMini;
			if (pb.PrgDuratSec > -.01 || pb.PrgPositSec > -.01)
			{
				pb.PrgDuration = TimeSpan.FromSeconds(pb.PrgDuratSec);
				pb.PrgPosition = TimeSpan.FromSeconds(pb.PrgPositSec);
			}

			if (pb.PrgDuratSec == -.01)
				pb.PrgsAngle = (pb.PrgDuration == null || pb.PrgDuration.TotalSeconds == 0) ? 0 : 360 * (pb.PrgPosition == null ? 0 : pb.PrgPosition.TotalSeconds) / pb.PrgDuration.TotalSeconds;
			else
				pb.PrgsAngle = (pb.PrgDuratSec <= 0) ? 0 : 360 * (pb.PrgPositSec <= 0 ? 0 : pb.PrgPositSec) / pb.PrgDuratSec;
			pb.ReplaceInlines();
		}
		public void ReplaceInlines()
		{
			//try { ReplaceInlines(tb1.Inlines); }			catch (Exception ex) { Trace.WriteLine(ex, System.Reflection.MethodInfo.GetCurrentMethod().ToString());  }
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

		public double PrgsAngle { get { return (double)GetValue(AngleProperty); } set { SetValue(AngleProperty, value); } }								public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("PrgsAngle", typeof(double), typeof(ucProgressArcMini), new PropertyMetadata(0.0));

		public TimeSpan PrgDuration { get { return (TimeSpan)GetValue(DurationProperty); } set { SetValue(DurationProperty, value); } }		public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("PrgDuration", typeof(TimeSpan), typeof(ucProgressArcMini), new PropertyMetadata(TimeSpan.Zero));
		public TimeSpan PrgPosition { get { return (TimeSpan)GetValue(PositionProperty); } set { SetValue(PositionProperty, value); } }		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("PrgPosition", typeof(TimeSpan), typeof(ucProgressArcMini), new PropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(recalc)));

		public double PrgDuratSec { get { return (double)GetValue(DuratSecProperty); } set { SetValue(DuratSecProperty, value); } }		public static readonly DependencyProperty DuratSecProperty = DependencyProperty.Register("PrgDuratSec", typeof(double), typeof(ucProgressArcMini), new PropertyMetadata(-.01));
		public double PrgPositSec { get { return (double)GetValue(PositSecProperty); } set { SetValue(PositSecProperty, value); } }		public static readonly DependencyProperty PositSecProperty = DependencyProperty.Register("PrgPositSec", typeof(double), typeof(ucProgressArcMini), new PropertyMetadata(-.01, new PropertyChangedCallback(recalc)));
	}
}
