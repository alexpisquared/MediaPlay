using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VPC.Models;

namespace VPC.Views
{
	public partial class DailyViewTimeUsrCtrl : UserControl
	{
		public DailyViewTimeUsrCtrl()
		{
			InitializeComponent();
			DataContext = this;
			HeaderInfo = "No go here yet";
			TotalDayViewTimes = new ObservableCollection<TotalDayViewTime>(ViewTimeLogCopy.DayList.OrderBy(r => r.DoneAt));

			Loaded += (s, e) =>
			{
				HeaderInfo = TotalDayViewTimes.Count.ToString();
				dg1.ScrollIntoView(TotalDayViewTimes[TotalDayViewTimes.Count - 1]);
			};
		}


		ViewTimeLog _ViewTimeLogCopy = ViewTimeLog.GetViewTimeLogSingleton();

		public ViewTimeLog ViewTimeLogCopy { get { return _ViewTimeLogCopy; } }//set { Set(ref _ViewTimeLogCopy, value); } }



		public TimeSpan TodayTotalC { get { return (TimeSpan)GetValue(TodayTotalCProperty); } set { SetValue(TodayTotalCProperty, value); } }		public static readonly DependencyProperty TodayTotalCProperty = DependencyProperty.Register("TodayTotalC", typeof(TimeSpan), typeof(DailyViewTimeUsrCtrl), new PropertyMetadata());
		public string HeaderInfo { get { return (string)GetValue(HeaderInfoProperty); } set { SetValue(HeaderInfoProperty, value); } }		public static readonly DependencyProperty HeaderInfoProperty = DependencyProperty.Register("HeaderInfo", typeof(string), typeof(DailyViewTimeUsrCtrl), new PropertyMetadata("CS"));
		public ObservableCollection<TotalDayViewTime> TotalDayViewTimes { get { return (ObservableCollection<TotalDayViewTime>)GetValue(TotalDayViewTimesProperty); } set { SetValue(TotalDayViewTimesProperty, value); } }		public static readonly DependencyProperty TotalDayViewTimesProperty = DependencyProperty.Register("TotalDayViewTimes", typeof(ObservableCollection<TotalDayViewTime>), typeof(DailyViewTimeUsrCtrl), new PropertyMetadata(null));
	}
}
