using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using AsLink;
using System.Diagnostics;
using System.IO;
using AAV.Sys.Helpers;

namespace VPC.Models
{
	public class TotalDayViewTime
	{
		TimeSpan _ttl;

		[XmlIgnore]
		public TimeSpan Ttl { get { return _ttl; } set { _ttl = value; } }
		[XmlAttribute("Total")]
		public string TotalXml { get { return _ttl.ToString(); } set { _ttl = TimeSpan.Parse(value); } }

		[XmlAttribute()]
		public DateTime Day { get; set; }
		[XmlAttribute()]
		public DateTime DoneAt { get; set; }
		[XmlAttribute()]
		public string DoneBy { get; set; }
	}

	public class ViewTimeLog
	{
#if DDDDDDD
		static string _file = OneDrive.AlexsVpdbFolder + "Deleted\\ViewTimeLog.Dbg.xml";
#else
    static string _file = OneDrive.VpdbFolder + "Deleted\\ViewTimeLog.xml";
#endif
		static ViewTimeLog _vtl = null;

		public List<TotalDayViewTime> DayList = new List<TotalDayViewTime>();

		public TimeSpan TodayTotal { get { return TimeSpan.FromSeconds(DayList.Where(r => r.Day == DateTime.Today).Sum(r => r.Ttl.TotalSeconds)); } }
		public TimeSpan GrandTotal { get { return TimeSpan.FromSeconds(DayList.Sum(r => r.Ttl.TotalSeconds)); } }

		public static ViewTimeLog GetViewTimeLogSingleton(bool needLatest = false)
		{
			if (_vtl == null)
			{
				var fdel = Path.GetDirectoryName(_file);
				try { if (!Directory.Exists(fdel)) Directory.CreateDirectory(fdel); }
				catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
				_vtl = Serializer.LoadFromFile<ViewTimeLog>(_file) as ViewTimeLog;
			}

			return _vtl;
		}
		public static void Log(TimeSpan newViewTimeInterval)
		{
			Log(newViewTimeInterval, DateTime.Today);
		}
		static TimeSpan _prevViewTimeInterval;
		public static void Log(TimeSpan newViewTimeInterval, DateTime dayStartedOn)
		{
			//todo: 			if (ttlSessionTime < TimeSpan.FromMinutes(1)) return;

			Debug.Assert(0 <= newViewTimeInterval.TotalSeconds && newViewTimeInterval < TimeSpan.FromDays(1));

			if (_prevViewTimeInterval == newViewTimeInterval) return;
			
			_prevViewTimeInterval = newViewTimeInterval;

			var log = GetViewTimeLogSingleton(true); //  Serializer.LoadFromFile<ViewTimeLog>(_file) as ViewTimeLog;

			var today = log.DayList.FirstOrDefault(r => r.Day == dayStartedOn);
			if (today == null)
			{
				log.DayList.Add(new TotalDayViewTime { Day = dayStartedOn, Ttl = newViewTimeInterval, DoneAt = DateTime.Now, DoneBy = Environment.MachineName + @"\" + Environment.UserName }); ;
				today = log.DayList.FirstOrDefault(r => r.Day == dayStartedOn);
			}
			else
			{
				Debug.Assert(0 <= today.Ttl.TotalSeconds && today.Ttl < TimeSpan.FromDays(1));
				today.Ttl = today.Ttl.Add(newViewTimeInterval);
				today.DoneAt = DateTime.Now;
				today.DoneBy = Environment.MachineName + @"\" + Environment.UserName;
			}

			Debug.Assert(0 <= today.Ttl.TotalSeconds && today.Ttl < TimeSpan.FromDays(1));
			if (0 <= today.Ttl.TotalSeconds && today.Ttl < TimeSpan.FromDays(1))
				Serializer.SaveToFile(log, _file);
			else
				Debug.WriteLine("???");

			newViewTimeInterval = TimeSpan.FromSeconds(0);
		}
	}
}
