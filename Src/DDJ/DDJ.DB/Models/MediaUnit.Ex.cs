using MVVM.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDJ.DB.Models
{
	public partial class MediaUnit : BindableBase
	{
		public double DurationSec { get { return _DurationSec; } set { Set(ref _DurationSec, value); DurationTS = TimeSpan.FromSeconds(value); } }									double _DurationSec = .0;
		public double CurPositionSec { get { return _CurPositionSec; } set { Set(ref _CurPositionSec, value); PositionTS = TimeSpan.FromSeconds(value); } }									double _CurPositionSec = .0;

		[NotMapped]
		public TimeSpan DurationTS { get { return _DurationTS; } set { Set(ref _DurationTS, value); } }														TimeSpan _DurationTS;
		[NotMapped]
		public TimeSpan PositionTS { get { return _PositionTS; } set { Set(ref _PositionTS, value); } }														TimeSpan _PositionTS;
	}
}
