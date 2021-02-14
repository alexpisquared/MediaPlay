using MVVM.Common;
using AsLink;
using DDJ.DB.Models;
using DDJ.Main.Cmn;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Speech.Synthesis;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Shell;
using System.Threading.Tasks;
using System.Windows;
using AAV.Sys.Helpers;

namespace DDJ.Main.ViewModels
{
    public partial class DdjViewModel : BindableBaseViewModel
	{
		DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.33) };
		public DdjEf4DBContext _db = new DdjEf4DBContext();
		int _animIdx = 0; bool _isLooping = false, _askToDeleteAtEnd = false;
		//SpeechSynthesizer synth = new SpeechSynthesizer();

		public DdjViewModel()
		{
			_timer.Tick += onTick;
			_timer.Start();
			TopRightTiny =					$"   {VerHelper.CurVerStr("4.8")} ";
			//UserA = DevOp.IsVIP;
			UserM = Environment.UserName.ToLower().Contains("mei");
			UserN = Environment.UserName.ToLower().Contains("nadi");
			UserZ = Environment.UserName.ToLower().Contains("zoe");


			_TrgFolders = new ObservableCollection<string>(new[] { @"M:\", @"C:\Users\nadin\OneDrive\Music\dm\", $@"C:\1\M\" });

			//RegisterHotKey(this.Handle, 4540, 0, Keys.MediaPlayPause);
		}

		protected override void AutoExec()
		{
			//Debug.Assert(Application.Current.Dispatcher.CheckAccess(), "Must be no UI thread!!!"); // if on UI thread							
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () => await LoadLkus()));
		}


		//VMState _VMState;
		//public string SaveState()
		//{
		//	var gfArray = new int[GenreFilter.Count()];
		//	int i = 0; foreach (var g in GenreFilter) { gfArray[i++] = g.ID; }

		//	var vms = new VMState
		//	{
		//		GenreFilter = gfArray,
		//		AddRandomDay = AddRandomDay,
		//		PlaylilstLen = PlaylilstLen,
		//		StringFilter = StringFilter,
		//		AutoStart = AutoStart
		//	};
		//	return Serializer.SaveToString(vms);
		//}
		//public static DdjViewModel RestoreState(string vmState)
		//{
		//	var dvm = new DdjViewModel();
		//	if (!string.IsNullOrEmpty(vmState))
		//	{
		//		var vms = Serializer.LoadFromString<VMState>(vmState) as VMState;
		//		if (vms != null)
		//		{
		//			//dvm._VMState = vms;
		//			dvm.AddRandomDay = vms.AddRandomDay < 1 ? 1 : vms.AddRandomDay;
		//			dvm.PlaylilstLen = vms.PlaylilstLen;
		//			dvm.StringFilter = vms.StringFilter;
		//			dvm.AutoStart = vms.AutoStart;

		//			foreach (var g in vms.GenreFilter) { dvm.GenreFilter.Add(new LkuGenre { ID = g }); dvm.onTglGenre(g); }
		//		}
		//	}
		//	return dvm;
		//}

		void onTick(object sender, EventArgs e)
		{
			if (_player == null || _player.Source == null) return;

			var views = $"{CurMediaUnit.MuAuditions.Count}vws {CurMediaUnit.MuRateHists.Sum(r => r.LikeUnit)}lks";
			var el = new string[] { " ...", ". ..", ".. .", "... " }[_animIdx % 4];

			CurMedUnInfo =
					$"{(_lastPlayStartAt == DateTime.MaxValue ? (isPlaying ? "▼ Stopping" + el : "▌▐") : (isPlaying ? "►" : "▲ Starting" + el))} {(Math.Abs(_player.SpeedRatio - 1.0) < .1 ? "" : $"{_player.SpeedRatio,4}")}{(_isLooping ? " ∞ " : "")}{views} {(HasPlayedEnough ? "·" : "°")}{(_player.Volume == 0 ? "MUTE" : "")}";
			//ExceptionMsg = string.Format("{0:HH:mm}", DateTime. Now);

			try
			{
				if (_player.NaturalDuration.HasTimeSpan && _player.NaturalDuration.TimeSpan > _player.Position)
				{
					MUProgressPerc = _player.NaturalDuration.TimeSpan.TotalSeconds == 0 ? 0 : _player.Position.TotalSeconds / _player.NaturalDuration.TimeSpan.TotalSeconds;

					if (_player.Position.TotalSeconds != 0)           // only if not 0 (trying to prevent zeroing position on media change).	
					{
						_ignorePlayerPosnMove = true;
						VMPosn = CurMediaUnit.CurPositionSec = _player.Position.TotalSeconds;
						_ignorePlayerPosnMove = false;
					}
					//CurMedUnInfo += string.Format(" {0:m\\:ss} + {1:m\\:ss} = {2:m\\:ss}", _player.Position, _player.NaturalDuration.Subtract(_player.Position).TimeSpan, _player.NaturalDuration.TimeSpan);//ViewTimeLogCopy.TodayTotal.Add(viewtime));
					Window_Title =
							$"{100.0 * _player.Position.TotalSeconds / (_player.NaturalDuration.TimeSpan.TotalSeconds + .000001):N0}% {Path.GetFileNameWithoutExtension(CurMediaUnit.PathFileExtOrg)}";
				}
			}
			catch (Exception ex) { TopLefttInfo = ex.Message; Debug.WriteLine(ex, ":>Error"); }

			if (++_animIdx % 180 == 0) //save to db every min (if period is 333ms)
			{
				//if (can_L_ && HasPlayedEnough) updatePosnMuStatsAndSave();
			}

			//Debug.WriteLine(string.Format("{0:N1} / {1:N1}", CurCurrentPostn, CurCurrentDurtn));
		}

		MediaElement _player = null;
		readonly ObservableCollection<MediaUnit> _PlayList = new ObservableCollection<MediaUnit>();  /**/ public ObservableCollection<MediaUnit> PlayList { get { return _PlayList; } }
		readonly ObservableCollection<LkuGenre> _Genres = new ObservableCollection<LkuGenre>();      /**/ public ObservableCollection<LkuGenre> Genres { get { return _Genres; } }
		readonly ObservableCollection<LkuGenre> _GenreFilter = new ObservableCollection<LkuGenre>(); /**/ public ObservableCollection<LkuGenre> GenreFilter { get { return _GenreFilter; } }
		readonly ObservableCollection<string> _TrgFolders;                                           /**/ public ObservableCollection<string> TrgFolders { get { return _TrgFolders; } }

		public MediaUnit CurMediaUnit { get { return _cmu; } set { logOldIfNewComing(_cmu, value); Set(ref _cmu, value); } }
		MediaUnit _cmu = null;
		public LkuGenre CurGenre { get { return _gnr; } set { Set(ref _gnr, value); } }
		LkuGenre _gnr = null;

		public double MUProgressPerc { get { return _MUProgressPerc; } set { Set(ref _MUProgressPerc, value); } }
		double _MUProgressPerc;
		public TaskbarItemProgressState MUProgressState { get { return _mups; } set { Set(ref _mups, value); } }
		TaskbarItemProgressState _mups = TaskbarItemProgressState.Normal;

		public string TrgFolder { get { return _TrgFolder; } set { Set(ref _TrgFolder, value); } }
    string _TrgFolder = @"M:\"; //  @"C:\1\M\";
		public string WmpSource { get { return _WmpSource; } set { Set(ref _WmpSource, value); } }
		string _WmpSource;
		public string BotmRghtInfo { get { return _BotmRghtInfo; } set { Set(ref _BotmRghtInfo, value); } }
		string _BotmRghtInfo;
		public string ExceptionMsg { get { return _ExceptionMsg; } set { Set(ref _ExceptionMsg, value); } }
		string _ExceptionMsg;
		public string TopRightTiny { get { return _TopRightTiny; } set { Set(ref _TopRightTiny, value); } }
		string _TopRightTiny;
		public string CurMedUnInfo { get { return _CurMedUnInfo; } set { Set(ref _CurMedUnInfo, value); } }
		string _CurMedUnInfo;
		public string HelpMessage_ { get { return _HelpMessage_; } set { Set(ref _HelpMessage_, value); } }
		string _HelpMessage_;
		public string TopLefttInfo { get { return _TopLefttInfo; } set { Set(ref _TopLefttInfo, value); } }
		string _TopLefttInfo;
		public string Window_Title { get { return _Window_Title; } set { Set(ref _Window_Title, value); } }
		string _Window_Title = "";
		public string StringFilter { get { return _StringFilter; } set { Set(ref _StringFilter, value); } }
		string _StringFilter = "";
		public int AddRandomDay { get { return _AddRandomDay; } set { Set(ref _AddRandomDay, value); } }
		int _AddRandomDay = 1;
		public int PlaylilstLen { get { return _PlaylilstLen; } set { Set(ref _PlaylilstLen, value); } }
		int _PlaylilstLen = 10;
		public bool? AutoStart { get { return _AutoStart; } set { Set(ref _AutoStart, value); } }
		bool? _AutoStart = false;
		public bool? LastPiece { get { return _LastPiece; } set { Set(ref _LastPiece, value); } }
		bool? _LastPiece = false;

		public bool UserA { get { return _UserA; } set { Set(ref _UserA, value); } }
		bool _UserA = true; // Jun2017
		public bool UserM { get { return _UserM; } set { Set(ref _UserM, value); } }
		bool _UserM = false;
		public bool UserN { get { return _UserN; } set { Set(ref _UserN, value); } }
		bool _UserN = false;
		public bool UserZ { get { return _UserZ; } set { Set(ref _UserZ, value); } }
		bool _UserZ = false;

		public bool IsLoading { get { return _IsLoading; } set { Set(ref _IsLoading, value); } }
		bool _IsLoading = false;
		public bool AudioRprtg { get { return _AudioRprtg; } set { Set(ref _AudioRprtg, value); } }
		bool _AudioRprtg = false;

		public double IniPosn { get { return _lastStartPosnSec; } set { Set(ref _lastStartPosnSec, value); } }
		double _lastStartPosnSec = -1d;
		public double VMPosn
		{
			get { return _vmPosn; }
			set
			{
				if (Set(ref _vmPosn, value) && !_ignorePlayerPosnMove)
				{
					_prevPosn = value;
					Task.Run(async () => await Task.Delay(125)).ContinueWith(_ =>
					{
						if (_prevPosn == value)
						{
							_player.Position = TimeSpan.FromSeconds(CurMediaUnit.CurPositionSec = value);
							Bpr.BeepOkB();
						}
					}, TaskScheduler.FromCurrentSynchronizationContext());
				}
			}
		}
		double _vmPosn = .0, _prevPosn = -1;


		protected override async Task  ClosingVM()
		{
			_player.Pause();
			logAuditionCurPosn();
      await Task.Delay(9);
    }

    public bool _ignorePlayerPosnMove { get; set; }
	}
}
