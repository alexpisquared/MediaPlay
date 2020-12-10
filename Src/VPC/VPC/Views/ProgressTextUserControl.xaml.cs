using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace VPC.Views
{
	/// <summary>
	/// Interaction logic for ProgressTextUserControl.xaml
	/// </summary>
	public partial class ProgressTextUserControl : UserControl
	{
		public ProgressTextUserControl()
		{
			InitializeComponent();
		}

		public TimeSpan Position { get { return (TimeSpan)GetValue(PositionProperty); } set { SetValue(PositionProperty, value); } }		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("PrgPosition", typeof(TimeSpan), typeof(ProgressTextUserControl), new PropertyMetadata(TimeSpan.FromHours(1), update));
		public TimeSpan Duration { get { return (TimeSpan)GetValue(DurationProperty); } set { SetValue(DurationProperty, value); } }		public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("PrgDuration", typeof(TimeSpan), typeof(ProgressTextUserControl), new PropertyMetadata(TimeSpan.FromHours(2), update));

		static void update(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ProgressTextUserControl)d).ReplaceInlines();
		}

		public void ReplaceInlines() { ReplaceInlines(tbCurPosTtl.Inlines); }
		public void ReplaceInlines(InlineCollection ic)
		{
			const int fsz = 12;
			ic.Clear();
			//string s = string.Format(@"{0:h\:mm}<{1:h\:mm}", (StartPos), ts);

			dualFontSize(ic, fsz, Position);
			ic.Add((new Run() { Text = " · " }));
			dualFontSize(ic, fsz, Duration - Position);
			ic.Add((new Run() { Text = "\n────────────────\n", FontSize = fsz }));
			dualFontSize(ic, fsz, Duration);
		}
		void dualFontSize(InlineCollection ic, int fsz, TimeSpan ts)
		{
			if (ts.TotalMinutes > 60)
				ic.Add((new Run($@"{ts:h\:mm}")));
			else
				ic.Add((new Run($@" {ts.Minutes}")));
			ic.Add((new Run() { Text = $".{ts:ss}", FontSize = fsz }));
		}

	}
}
