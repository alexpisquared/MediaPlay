using ApxCmn;
using MVVM.Common;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace xPocBits.VMs
{


    public partial class MainVM : ViewModelBase
	{

		public static MainVM Instance { get; }
		static MainVM() { Instance = Instance ?? new MainVM(); }// implement singleton pattern		}

		MainVM()
		{
#if DEBUG
			IsDevDbg = true;
			DevDbgViz = Visibility.Visible;
#else
			IsDevDbg = false;
			DevDbgViz = Visibility.Collapsed;
#endif
		}


		public ObservableCollection<MediaInfo> MediaInfos { get; set; } = new ObservableCollection<MediaInfo>();
		public MediaElement MediaElement { get; set; }

		MediaInfo _SelectMI;				/**/public MediaInfo SelectMI { get { return _SelectMI; } set { Set(ref _SelectMI, value); setPlrSrc(); } }
		bool _IsDevDbg;							/**/public bool IsDevDbg { get { return _IsDevDbg; } set { Set(ref _IsDevDbg, value); } }
		Visibility _DevDbgViz;			/**/public Visibility DevDbgViz { get { return _DevDbgViz; } set { Set(ref _DevDbgViz, value); } }
		string _ResumedAt;					/**/public string ResumedAt { get { return _ResumedAt; } set { Set(ref _ResumedAt, value); } }
		string _ExnMsg;							/**/public string ExnMsg { get { return _ExnMsg; } set { Set(ref _ExnMsg, value); } }
		string _Info = null;				/**/public string Info { get { return _Info; } set { Set(ref _Info, value); } }
		bool? _IsReady = true;			/**/public bool? IsReady { get { return _IsReady; } set { Set(ref _IsReady, value); OnPropertyChanged(); } }
		double _PlaybackRate = 1d;	/**/public double PlaybackRate { get { return _PlaybackRate; } set { Set(ref _PlaybackRate, value); } }


		/*
		/// <summary>
		/// Oct2016:
		/// For async acctions only use this pattern to disable button on the period of execution
		///		...or init all commands at once in ctor (like ghs do):
		///		            
		
	ctor()
	{
		if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
		{
			// designtime sample data
			var data = _todoListRepository.Sample().Select(x => new ViewModels.TodoListViewModel(x));
			TodoLists = new ObservableCollection<ViewModels.TodoListViewModel>(data);
		}
		else
		{
			// update Commands
			PropertyChanged += (s, e) =>
			{
					AddListCommand			.RaiseCanExecuteChanged();
					RemoveListCommand		.RaiseCanExecuteChanged();
					RemoveAdsCommand		.RaiseCanExecuteChanged();
					ShowVideoAdCommand	.RaiseCanExecuteChanged();
			};
		}
	}//ctor
		/// For sync actions - no need; just use Lync syntax
		/// </summary>
*/
		RelayCommand _F1Cmd;  /**/public RelayCommand F1Cmd
		{
			get
			{
				if (_F1Cmd == null)
				{
					_F1Cmd = new RelayCommand(async x => await doF1(), x => IsReady == true);
					PropertyChanged += (s, e) => _F1Cmd.RaiseCanExecuteChanged();
				}
				return _F1Cmd;
			}
		}
		ICommand _F2Cmd;				/**/public ICommand F2Cmd { get { return _F2Cmd ?? (_F2Cmd = new RelayCommand(async x => { Info = "F2"; IsReady = false; await libLoad(KnownFolderId.MusicLibrary); }, x => IsReady == true)); } }
		ICommand _F3Cmd;				/**/public ICommand F3Cmd { get { return _F3Cmd ?? (_F3Cmd = new RelayCommand(async x => { Info = "F3"; IsReady = false; await libLoad(KnownFolderId.VideosLibrary); }, x => IsReady == true)); } }
		ICommand _F4Cmd;				/**/public ICommand F4Cmd => _F4Cmd ?? (_F4Cmd = new RelayCommand(async x => { Info = "F4"; IsReady = false; await libLoad(KnownFolderId.VideosLibrary); }, x => IsReady == true));
		ICommand _F5Cmd;				/**/public ICommand F5Cmd => _F5Cmd ?? (_F5Cmd = new RelayCommand(async x => { Info = "F5"; IsReady = false; await libLoad(KnownFolderId.VideosLibrary); }, x => IsReady == true));
		ICommand _F6Cmd;				/**/public ICommand F6Cmd => _F6Cmd ?? (_F6Cmd = new RelayCommand(x => doF6(x), x => IsReady == true));
		ICommand _DltCmd;				/**/public ICommand DltCmd => _DltCmd ?? (_DltCmd = new RelayCommand(x => doDela(x), x => IsReady == true));


		void doDela(object x) { }
		void doF6(object x) { }
		async Task doF1()
		{
			Info = "F1...";
			IsReady = false;
			await Task.Delay(999);
			Info = "F1++!";
			IsReady = true;
		}
	}
}
