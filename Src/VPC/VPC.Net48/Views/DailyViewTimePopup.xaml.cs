using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VPC.Models;

namespace VPC.Views
{
	public partial class DailyViewTimePopup : Window
	{
		public DailyViewTimePopup()
		{
			InitializeComponent(); MouseLeftButtonDown += (s, e) => { DragMove(); e.Handled = true; }; KeyDown += (s, ves) => { switch (ves.Key) { case Key.Escape: Close(); /*App.Current.Shutdown();*/ break; } };

			DataContext = this;
			HeaderInfo = "No go here yet";
			TotalDayViewTimes = new ObservableCollection<TotalDayViewTime>(ViewTimeLogCopy.DayList.OrderByDescending(r => r.DoneAt));

			Loaded += (s, e) =>
			{
				HeaderInfo = TotalDayViewTimes.Count.ToString();
				dg1.ScrollIntoView(TotalDayViewTimes[0]);// TotalDayViewTimes.Count - 1]);
			};
		}

		public ViewTimeLog ViewTimeLogCopy { get { return _ViewTimeLogCopy; } } ViewTimeLog _ViewTimeLogCopy = ViewTimeLog.GetViewTimeLogSingleton();

		public TimeSpan TodayTotalC { get { return (TimeSpan)GetValue(TodayTotalCProperty); } set { SetValue(TodayTotalCProperty, value); } }		public static readonly DependencyProperty TodayTotalCProperty = DependencyProperty.Register("TodayTotalC", typeof(TimeSpan), typeof(DailyViewTimePopup), new PropertyMetadata());
		public string HeaderInfo { get { return (string)GetValue(HeaderInfoProperty); } set { SetValue(HeaderInfoProperty, value); } }					public static readonly DependencyProperty HeaderInfoProperty = DependencyProperty.Register("HeaderInfo", typeof(string), typeof(DailyViewTimePopup), new PropertyMetadata("CS"));
		public ObservableCollection<TotalDayViewTime> TotalDayViewTimes { get { return (ObservableCollection<TotalDayViewTime>)GetValue(TotalDayViewTimesProperty); } set { SetValue(TotalDayViewTimesProperty, value); } }		public static readonly DependencyProperty TotalDayViewTimesProperty = DependencyProperty.Register("TotalDayViewTimes", typeof(ObservableCollection<TotalDayViewTime>), typeof(DailyViewTimePopup), new PropertyMetadata(null));
	}
}