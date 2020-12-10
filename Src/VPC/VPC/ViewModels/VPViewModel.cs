using AAV.Sys.Ext;
using AAV.Sys.Helpers;
using AsLink;
using Common.UI.Lib.Model;
using Microsoft.Win32;
using MVVM.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using VPC.Models;
using VPC.Views;

namespace VPC.ViewModels
{
  public class VPViewModel : BindableBaseViewModel, IVPViewModel
  {
    MediaElement _vpcPlayer = null;
    FolderViewUsrCtrl _fvc;
    readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.14) };
    DateTime _lastPlayStartAt = DateTime.MaxValue, _tillTime = DateTime.MaxValue;
    readonly DateTime _daySsnStarted = DateTime.Today;
    TimeSpan _lastStartPosn, _viewTime = TimeSpan.FromSeconds(0), _posA, _durnToB, _TodayTotal;
    TimeSpan? _isJumpingTo = null;
    const double _speedMultr = 1.18920712;//=2^(1 / 4);  1.14869835;//2^(1 / 5); 1.18920712;//=2^(1 / 4);  1.25992105;//=2^(1 / 3);   1.41421356//=2^(1 / 2) ;
    #region
    readonly int _v1x = 8, _v2x = 11;
    readonly double[] _speeds = {
            .0001,    //  0
            .001,     //  1
            .01,      //  2
            .03,      //  3
            .1,       //  4
            .2,       //  5
            .50,      //  6
            .75,      //  7
            1,        //  8 <== _v1x == 1
            1.41,     //  9
            1.68,     // 10 
            2.0,      // 11 <== _v2x == 2
            2.3,      // 12
            2.6,      // 13
            3.0,      // 14
            4,        // 15
            8,        // 16
            16,       // 17
            32,       // 18
            100       // 19
        };
    #endregion
    string _Top_CentrMsg, _BotmCentrMsg, _BotmRghtInfo, _TopRightInfo, _TopRightTiny, _BotmLeftInfo, _HelpMessage_, _Window_Title = "...", _EOTMsg = "Zoe, that is enough. Its time to go to bed?";
    int cntr = 0, _intervalPlay = 0;
    Visibility _ChromeVisibility = Visibility.Visible;
    private bool _isLooping = false;
    private readonly bool _isAutoNext = true;
    private bool _popMenuAtEnd = true;
    ViewTimeLog _ViewTimeLogCopy = ViewTimeLog.GetViewTimeLogSingleton();
    SpeechSynthesizer _synth; public SpeechSynthesizer Synth { get { if (_synth == null) _synth = new SpeechSynthesizer(); return _synth; } }

    int playerMargin = 0;

    public int PlayerMargin
    {
      get => playerMargin;
      set { playerMargin = value; _vpcPlayer.Margin = new Thickness(0, playerMargin, 0, playerMargin); }
    }

    Panel _LayoutRoot;
    public Panel LayoutRoot
    {
      get => _LayoutRoot;
      set
      {
        Set(ref _LayoutRoot, value);
        if (value != null) RegisterDragAndDrop();
      }
    }
    void RegisterDragAndDrop()
    {
      LayoutRoot.Drop += OnDrop;
      LayoutRoot.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
      LayoutRoot.PreviewDragOver += OnDragOver;
    }
    void OnDrop(object s, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

      var files = e.Data.GetData(DataFormats.FileDrop) as string[];
      if (files.Length < 1) return;

      var csv = string.Join("|", files);
      //if (ex.KeyStates == DragDropKeyStates.ControlKey)
      //	m.LoadNewMedia(csv);//TODO: Add to the curent list
      //else
      //	m.LoadNewMedia(csv);

      PlayNewFile(csv);
    }
    void OnMouseLeftButtonDown(object s, MouseButtonEventArgs e) { var control = (FrameworkElement)e.Source; }
    void OnDragOver(object s, DragEventArgs e) { }

    TimeSpan _vmPosn = TimeSpan.Zero, _prevPosn = TimeSpan.Zero;
    public TimeSpan VMPosn
    {
      get => _vmPosn;  // VPModel.CrntMU.Position.HasValue ? VPModel.CrntMU.Position.Value : TimeSpan.Zero; }
      set
      {
        if (Set(ref _vmPosn, value))
        {
          //Debug.WriteLine("::VM> {0} ==> {1}", _vmPosn, value);
          if (!_ignorePlayerPosnMove)
          {
            _prevPosn = value;
            Task.Run(async () => await Task.Delay(125)).ContinueWith(_ =>
            {
              if (_prevPosn == value)
              {
                Bpr.BeepOk();
                _vpcPlayer.Position = (TimeSpan)(VPModel.CrntMU.Position = value);
              }
            }, TaskScheduler.FromCurrentSynchronizationContext());
          }
        }
      }
    }


    VPModel _vpModel = new VPModel(); public VPModel VPModel { get => _vpModel; set => _vpModel = value; }
    public MediaElement Player { set => initPlayer(value); }
    public FolderViewUsrCtrl Fvc { set => _fvc = value; }
    public ViewTimeLog ViewTimeLogCopy { get => _ViewTimeLogCopy; set => Set(ref _ViewTimeLogCopy, value); }

    public double MUProgressPerc { get => _MUProgressPerc; set => Set(ref _MUProgressPerc, value); }
    double _MUProgressPerc;
    public TaskbarItemProgressState MUProgressState { get => _MUProgressState; set => Set(ref _MUProgressState, value); }
    TaskbarItemProgressState _MUProgressState = TaskbarItemProgressState.Normal;
    public string BotmRghtInfo { get => _BotmRghtInfo; set => Set(ref _BotmRghtInfo, value); }
    public string TopRightInfo { get => _TopRightInfo; set => Set(ref _TopRightInfo, value); }
    public string TopRightTiny { get => _TopRightTiny; set => Set(ref _TopRightTiny, value); }
    public string BotmLeftInfo { get => _BotmLeftInfo; set => Set(ref _BotmLeftInfo, value); }
    public string HelpMessage_ { get => _HelpMessage_; set => Set(ref _HelpMessage_, value); }
    public string Top_CentrMsg { get => _Top_CentrMsg; set => Set(ref _Top_CentrMsg, value); }
    public string BotmCentrMsg { get => _BotmCentrMsg; set => Set(ref _BotmCentrMsg, value); }
    public string Window_Title { get => _Window_Title; set => Set(ref _Window_Title, value); }
    public TimeSpan TotalByNow { get => _TodayTotal; set => Set(ref _TodayTotal, value); }

    string _srch = null; /**/ public string Srch { get => _srch; set { if (_srch != value) if (Set(ref _srch, value)) _fvc.DoSearch(_srch, _sdir); } }
    bool _sdir = false;  /**/ public bool Sdir { get => _sdir; set { if (_sdir != value) if (Set(ref _sdir, value)) _fvc.DoSearch(_srch, _sdir); } }



    public bool IsTopmost { get => _IsTopmost; set => Set(ref _IsTopmost, value); }
    bool _IsTopmost = false;
    public bool IsPlaying { get => _IsBusy; set { Set(ref _IsBusy, value); MUProgressState = value ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.Paused; } }
    bool _IsBusy = false;

    public Visibility ChromeVisibility { get => _ChromeVisibility; set => Set(ref _ChromeVisibility, value); }

    static Version getVersion()
    {
      //DateTime buildDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
      try { return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion; }
      catch { return Assembly.GetExecutingAssembly().GetName().Version; }
    }

    public VPViewModel()
    {
      _timer.Tick += onTick;
      _timer.Start();
      TopRightTiny = VerHelper.CurVerStr(".NET 4.8");
    }

    async void onTick(object s, EventArgs e)
    {
      if (_tillTime < DateTime.Now && await isPlayingAsync())
      {
        pauseCollectViewTimeAndUpdatePosnMuStatsAndSave();
        Synth.Speak(_EOTMsg);
      }

      if (_vpcPlayer == null) return;
      if (_vpcPlayer.Source == null) return;
      var views = "";
      if (VPModel.CrntMU != null)
      {
        _ignorePlayerPosnMove = true;
        VPModel.CrntMU.PositionSec = (VPModel.CrntMU.Position = VMPosn = _vpcPlayer.Position).Value.TotalSeconds;
        _ignorePlayerPosnMove = false;
        views = $"{VPModel.CrntMU.Auditions.Count}vws";
      }

      var ip = await isPlayingAsync();
      var el = new string[] { " ...", ". ..", ".. .", "... " }[cntr % 4];
      BotmLeftInfo = $"{(_lastPlayStartAt == DateTime.MaxValue ? (ip ? "▼ Stopping" + el : " ▌▐") : (ip ? " ►" : "▲ Starting" + el))}  {views}  {(Math.Abs(_vpcPlayer.SpeedRatio - 1.0) < .1 ? " " : $"x{_vpcPlayer.SpeedRatio}")}  {(_isLooping ? " ∞" : " ")}  {(HasPlayedEnough ? "·" : "°")}  {(_vpcPlayer?.Volume == 0 ? "MUTE" : $"Vol:{_vpcPlayer?.Volume:N1}")}  {(IsTopmost ? "Z!" : "  ")}";
      TopRightInfo = $"{DateTime.Now:HH:mm} ";

      try
      {
        if (_vpcPlayer.NaturalDuration.HasTimeSpan && _vpcPlayer.NaturalDuration.TimeSpan > _vpcPlayer.Position)
        {
          MUProgressPerc = _vpcPlayer?.NaturalDuration.TimeSpan.TotalSeconds == 0 ? 0 : _vpcPlayer.Position.TotalSeconds / _vpcPlayer.NaturalDuration.TimeSpan.TotalSeconds;

          var totalByNow = ViewTimeLogCopy.TodayTotal.Add(ip ? _viewTime.Add(DateTime.Now - _lastPlayStartAt) : _viewTime);
          if (totalByNow.TotalDays > 0)
            TotalByNow = totalByNow;

          BotmRghtInfo = string.Format("({3:h\\:mm})    {0:hh\\:mm\\:ss} + {1:hh\\:mm\\:ss} = {2:hh\\:mm\\:ss}",
                                  _vpcPlayer.Position, _vpcPlayer.NaturalDuration.Subtract(_vpcPlayer.Position).TimeSpan, _vpcPlayer.NaturalDuration.TimeSpan, TotalByNow);

          Window_Title = $"{100.0 * _vpcPlayer.Position.TotalSeconds / (_vpcPlayer.NaturalDuration.TimeSpan.TotalSeconds + .000001):N1}% {VPModel?.CrntMU?.PathFileCur}";
        }


        //appears to be set but not really. await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => _vpcPlayer.SpeedRatio  = _speeds[VPModel.CrntMU.SpeedIdx]));
      }
      catch (Exception ex)
      {
        BotmCentrMsg = ex.Message;
        Debug.WriteLine(ex, ":>Error");
      }

      if (++cntr % 180 == 0) //save to db every min (if period is 333ms)
      {
        if (can_L_ && HasPlayedEnough) updatePosnMuStatsAndSave();
      }

      //_hideCursorAfter = DateTime.Now.AddSeconds(3);				if (DateTime.Now > _hideCursorAfter) _player.Cursor = Cursors.None;

      //Debug.WriteLine(string.Format("{0:N1} / {1:N1}", _vpModel.CurrentPostn, _vpModel.CurrentDurtn));
    }

    //<< Commands

    ICommand movUpDirCommand; public ICommand MovUpDirCommand => movUpDirCommand ?? (movUpDirCommand = new RelayCommand(x => movUpDir(x), x => canMovUpDir) { GestureKey = Key.Up, GestureModifier = ModifierKeys.Alt });

    ICommand moveNextCommand; public ICommand MoveNextCommand => moveNextCommand ?? (moveNextCommand = new RelayCommand(x => moveNext(x), x => canMoveNext) { GestureKey = Key.Down });
    ICommand movePrevCommand; public ICommand MovePrevCommand => movePrevCommand ?? (movePrevCommand = new RelayCommand(x => movePrev(x), x => canMovePrev) { GestureKey = Key.Up });
    ICommand meetNextCommand; public ICommand MeetNextCommand => meetNextCommand ?? (meetNextCommand = new RelayCommand(x => meetNext(x), x => canMoveNext) { GestureKey = Key.Down, GestureModifier = ModifierKeys.Shift });
    ICommand meetPrevCommand; public ICommand MeetPrevCommand => meetPrevCommand ?? (meetPrevCommand = new RelayCommand(x => meetPrev(x), x => canMovePrev) { GestureKey = Key.Up, GestureModifier = ModifierKeys.Shift });

    ICommand moveLefNCommand; public ICommand MoveLefNCommand => moveLefNCommand ?? (moveLefNCommand = new RelayCommand(x => moveLeftN(x), x => canMoveLeftN) { GestureKey = Key.Left });
    ICommand moveRghNCommand; public ICommand MoveRghNCommand => moveRghNCommand ?? (moveRghNCommand = new RelayCommand(x => moveRghtN(x), x => canMoveRghtN) { GestureKey = Key.Right });
    ICommand moveLefCCommand; public ICommand MoveLefCCommand => moveLefCCommand ?? (moveLefCCommand = new RelayCommand(x => moveLeftC(x), x => canMoveLeftC) { GestureKey = Key.Left, GestureModifier = ModifierKeys.Control });
    ICommand moveRghCCommand; public ICommand MoveRghCCommand => moveRghCCommand ?? (moveRghCCommand = new RelayCommand(x => moveRghtC(x), x => canMoveRghtC) { GestureKey = Key.Right, GestureModifier = ModifierKeys.Control });
    ICommand moveLefACommand; public ICommand MoveLefACommand => moveLefACommand ?? (moveLefACommand = new RelayCommand(x => moveLeftA(x), x => canMoveLeftA) { GestureKey = Key.Left, GestureModifier = ModifierKeys.Alt });
    ICommand moveRghACommand; public ICommand MoveRghACommand => moveRghACommand ?? (moveRghACommand = new RelayCommand(x => moveRghtA(x), x => canMoveRghtA) { GestureKey = Key.Right, GestureModifier = ModifierKeys.Alt });
    ICommand moveLefSCommand; public ICommand MoveLefSCommand => moveLefSCommand ?? (moveLefSCommand = new RelayCommand(x => moveLeftS(x), x => canMoveLeftS) { GestureKey = Key.Left, GestureModifier = ModifierKeys.Shift });
    ICommand moveRghSCommand; public ICommand MoveRghSCommand => moveRghSCommand ?? (moveRghSCommand = new RelayCommand(x => moveRghtS(x), x => canMoveRghtS) { GestureKey = Key.Right, GestureModifier = ModifierKeys.Shift });

    ICommand _TglPlyPsCommand; public ICommand TglPlyPsCommand => _TglPlyPsCommand ?? (_TglPlyPsCommand = new RelayCommand(x => doTglPlyPs(x)) { GestureKey = Key.Space });

    ICommand _TglStrcSCommand; public ICommand TglStrcSCommand => _TglStrcSCommand ?? (_TglStrcSCommand = new RelayCommand(x => doTglStrcS(x)) { GestureKey = Key.S, GestureModifier = ModifierKeys.Shift });
    ICommand _DeleteMUCommand; public ICommand DeleteMUCommand => _DeleteMUCommand ?? (_DeleteMUCommand = new RelayCommand(x => doAsk_CanNext(x)) { GestureKey = Key.Delete });

    ICommand _HomeCommand; public ICommand HomeCommand => _HomeCommand ?? (_HomeCommand = new RelayCommand(x => doHome(x)) { GestureKey = Key.Home });
    ICommand _End_Command; public ICommand End_Command => _End_Command ?? (_End_Command = new RelayCommand(x => doEnd_(x)) { GestureKey = Key.End });

    ICommand _PrevBmkCmd; public ICommand PrevBmkCmd => _PrevBmkCmd ?? (_PrevBmkCmd = new RelayCommand(x => do_PrevBmkCmd(x), x => canPrevBmkCmd) { GestureKey = Key.Oem4, GestureModifier = ModifierKeys.None });
    ICommand _NextBmkCmd; public ICommand NextBmkCmd => _NextBmkCmd ?? (_NextBmkCmd = new RelayCommand(x => do_NextBmkCmd(x), x => canNextBmkCmd) { GestureKey = Key.Oem6, GestureModifier = ModifierKeys.None });

    ICommand _F1_Command; public ICommand F1_Command => _F1_Command ?? (_F1_Command = new RelayCommand(x => do_F1_(x), x => can_Yes) { GestureKey = Key.F1, GestureModifier = ModifierKeys.None });
    ICommand _F2_Command; public ICommand F2_Command => _F2_Command ?? (_F2_Command = new RelayCommand(x => do_F2_(x), x => can_F2_) { GestureKey = Key.F2, GestureModifier = ModifierKeys.None });
    ICommand _F3_Command; public ICommand F3_Command => _F3_Command ?? (_F3_Command = new RelayCommand(x => do_F3_(x), x => can_Yes) { GestureKey = Key.F3, GestureModifier = ModifierKeys.None });
    ICommand _F4_Command; public ICommand F4_Command => _F4_Command ?? (_F4_Command = new RelayCommand(x => do_F4_(x), x => can_Yes) { GestureKey = Key.F4, GestureModifier = ModifierKeys.None });
    ICommand _F5_Command; public ICommand F5_Command => _F5_Command ?? (_F5_Command = new RelayCommand(x => do_F5_(x), x => can_Yes) { GestureKey = Key.F5, GestureModifier = ModifierKeys.None });
    ICommand _F6_Command; public ICommand F6_Command => _F6_Command ?? (_F6_Command = new RelayCommand(x => do_F6_(x), x => can_Yes) { GestureKey = Key.F6, GestureModifier = ModifierKeys.None });
    ICommand _F7_Command; public ICommand F7_Command => _F7_Command ?? (_F7_Command = new RelayCommand(x => do_F7_(x), x => can_Yes) { GestureKey = Key.F7, GestureModifier = ModifierKeys.None });
    ICommand _F8_Command; public ICommand F8_Command => _F8_Command ?? (_F8_Command = new RelayCommand(x => do_F8_(x), x => can_Yes) { GestureKey = Key.F8, GestureModifier = ModifierKeys.None });
    ICommand _F9_Command; public ICommand F9_Command => _F9_Command ?? (_F9_Command = new RelayCommand(x => do_F9_(x), x => can_Yes) { GestureKey = Key.F9, GestureModifier = ModifierKeys.None });
    ICommand _F10_Command; public ICommand F10_Command => _F10_Command ?? (_F10_Command = new RelayCommand(x => do_F10_(x), x => can_Yes) { GestureKey = Key.F10, GestureModifier = ModifierKeys.None });
    ICommand _F11_Command; public ICommand F11_Command => _F11_Command ?? (_F11_Command = new RelayCommand(x => do_F11_(x), x => can_Yes) { GestureKey = Key.F11, GestureModifier = ModifierKeys.None });
    ICommand _F12_Command; public ICommand F12_Command => _F12_Command ?? (_F12_Command = new RelayCommand(x => do_F12_(x), x => can_Yes) { GestureKey = Key.F12, GestureModifier = ModifierKeys.None });

    ICommand _D0_Command; public ICommand D0_Command => _D0_Command ?? (_D0_Command = new RelayCommand(x => do_D0_(x), x => can_Yes) { GestureKey = Key.D0, GestureModifier = ModifierKeys.None });
    ICommand _D1_Command; public ICommand D1_Command => _D1_Command ?? (_D1_Command = new RelayCommand(x => do_D1_(x), x => can_Yes) { GestureKey = Key.D1, GestureModifier = ModifierKeys.None });
    ICommand _D2_Command; public ICommand D2_Command => _D2_Command ?? (_D2_Command = new RelayCommand(x => do_D2_(x), x => can_Yes) { GestureKey = Key.D2, GestureModifier = ModifierKeys.None });
    ICommand _D3_Command; public ICommand D3_Command => _D3_Command ?? (_D3_Command = new RelayCommand(x => do_D3_(x), x => can_Yes) { GestureKey = Key.D3, GestureModifier = ModifierKeys.None });
    ICommand _D4_Command; public ICommand D4_Command => _D4_Command ?? (_D4_Command = new RelayCommand(x => do_D4_(x), x => can_Yes) { GestureKey = Key.D4, GestureModifier = ModifierKeys.None });
    ICommand _D5_Command; public ICommand D5_Command => _D5_Command ?? (_D5_Command = new RelayCommand(x => do_D5_(x), x => can_Yes) { GestureKey = Key.D5, GestureModifier = ModifierKeys.None });
    ICommand _D6_Command; public ICommand D6_Command => _D6_Command ?? (_D6_Command = new RelayCommand(x => do_D6_(x), x => can_Yes) { GestureKey = Key.D6, GestureModifier = ModifierKeys.None });
    ICommand _D7_Command; public ICommand D7_Command => _D7_Command ?? (_D7_Command = new RelayCommand(x => do_D7_(x), x => can_Yes) { GestureKey = Key.D7, GestureModifier = ModifierKeys.None });
    ICommand _D8_Command; public ICommand D8_Command => _D8_Command ?? (_D8_Command = new RelayCommand(x => do_D8_(x), x => can_Yes) { GestureKey = Key.D8, GestureModifier = ModifierKeys.None });
    ICommand _D9_Command; public ICommand D9_Command => _D9_Command ?? (_D9_Command = new RelayCommand(x => do_D9_(x), x => can_Yes) { GestureKey = Key.D9, GestureModifier = ModifierKeys.None });

    ICommand _A_Command; public ICommand A_Command => _A_Command ?? (_A_Command = new RelayCommand(x => do_A_(x), x => can_Yes) { GestureKey = Key.A, GestureModifier = ModifierKeys.None });
    ICommand _B_Command; public ICommand B_Command => _B_Command ?? (_B_Command = new RelayCommand(x => do_B_(x), x => can_B_) { GestureKey = Key.B, GestureModifier = ModifierKeys.None });
    ICommand _C_Command; public ICommand C_Command => _C_Command ?? (_C_Command = new RelayCommand(x => do_C_(x), x => can_Yes) { GestureKey = Key.C, GestureModifier = ModifierKeys.None });
    ICommand _D_Command; public ICommand D_Command => _D_Command ?? (_D_Command = new RelayCommand(x => doGoFaster(x), x => can_Yes) { GestureKey = Key.D, GestureModifier = ModifierKeys.None });
    ICommand _E_Command; public ICommand E_Command => _E_Command ?? (_E_Command = new RelayCommand(x => do_E_(x), x => can_Yes) { GestureKey = Key.E, GestureModifier = ModifierKeys.None });
    ICommand _F_Command; public ICommand F_Command => _F_Command ?? (_F_Command = new RelayCommand(x => do_X_(x), x => can_F_) { GestureKey = Key.F, GestureModifier = ModifierKeys.None });
    ICommand _G_Command; public ICommand G_Command => _G_Command ?? (_G_Command = new RelayCommand(x => do_G_V_2x_1x_tgl(x), x => can_G_) { GestureKey = Key.G, GestureModifier = ModifierKeys.None });
    ICommand _H_Command; public ICommand H_Command => _H_Command ?? (_H_Command = new RelayCommand(x => do_H_(x), x => can_Yes) { GestureKey = Key.H, GestureModifier = ModifierKeys.None });
    ICommand _I_Command; public ICommand I_Command => _I_Command ?? (_I_Command = new RelayCommand(x => do_I_(x), x => can_Yes) { GestureKey = Key.I, GestureModifier = ModifierKeys.None });
    ICommand _J_Command; public ICommand J_Command => _J_Command ?? (_J_Command = new RelayCommand(x => do_J_(x), x => can_Yes) { GestureKey = Key.J, GestureModifier = ModifierKeys.None });
    ICommand _K_Command; public ICommand K_Command => _K_Command ?? (_K_Command = new RelayCommand(x => do_K_(x), x => can_Yes) { GestureKey = Key.K, GestureModifier = ModifierKeys.None });
    ICommand _L_Command; public ICommand L_Command => _L_Command ?? (_L_Command = new RelayCommand(x => do_L_(x), x => can_L_) { GestureKey = Key.L, GestureModifier = ModifierKeys.None });
    ICommand _M_Command; public ICommand M_Command => _M_Command ?? (_M_Command = new RelayCommand(x => do_M_(x), x => can_Yes) { GestureKey = Key.M, GestureModifier = ModifierKeys.None });
    ICommand _N_Command; public ICommand N_Command => _N_Command ?? (_N_Command = new RelayCommand(x => do_N_(x), x => can_Yes) { GestureKey = Key.N, GestureModifier = ModifierKeys.None });
    ICommand _O_Command; public ICommand O_Command => _O_Command ?? (_O_Command = new RelayCommand(x => do_O_(x), x => can_Yes) { GestureKey = Key.O, GestureModifier = ModifierKeys.None });
    ICommand _P_Command; public ICommand P_Command => _P_Command ?? (_P_Command = new RelayCommand(x => do_P_(x), x => can_Yes) { GestureKey = Key.P, GestureModifier = ModifierKeys.None });
    ICommand _Q_Command; public ICommand Q_Command => _Q_Command ?? (_Q_Command = new RelayCommand(x => do_Q_(x), x => can_Yes) { GestureKey = Key.Q, GestureModifier = ModifierKeys.None });
    ICommand _R_Command; public ICommand R_Command => _R_Command ?? (_R_Command = new RelayCommand(x => do_R_(x), x => can_Yes) { GestureKey = Key.R, GestureModifier = ModifierKeys.None });
    ICommand _TglSthCmd; public ICommand S_Command => _TglSthCmd ?? (_TglSthCmd = new RelayCommand(x => doGoSlower(x)) { GestureKey = Key.S });
    ICommand _T_Command; public ICommand T_Command => _T_Command ?? (_T_Command = new RelayCommand(x => do_T_(x), x => can_Yes) { GestureKey = Key.T, GestureModifier = ModifierKeys.None });
    ICommand _U_Command; public ICommand U_Command => _U_Command ?? (_U_Command = new RelayCommand(x => do_U_(x), x => can_Yes) { GestureKey = Key.U, GestureModifier = ModifierKeys.None });
    ICommand _V_Command; public ICommand V_Command => _V_Command ?? (_V_Command = new RelayCommand(x => do_V_(x), x => can_Yes) { GestureKey = Key.V, GestureModifier = ModifierKeys.None });
    ICommand _W_Command; public ICommand W_Command => _W_Command ?? (_W_Command = new RelayCommand(x => do_W_(x), x => can_Yes) { GestureKey = Key.W, GestureModifier = ModifierKeys.None });
    ICommand _X_Command; public ICommand X_Command => _X_Command ?? (_X_Command = new RelayCommand(x => do_X_(x), x => can_Yes) { GestureKey = Key.X, GestureModifier = ModifierKeys.None });
    ICommand _X2Command; public ICommand X2Command => _X2Command ?? (_X2Command = new RelayCommand(x => do_X_(x), x => can_Yes) { GestureKey = Key.Enter, GestureModifier = ModifierKeys.Alt });
    ICommand _Y_Command; public ICommand Y_Command => _Y_Command ?? (_Y_Command = new RelayCommand(x => do_Y_(x), x => can_Yes) { GestureKey = Key.Y, GestureModifier = ModifierKeys.None });
    ICommand _Z_Command; public ICommand Z_Command => _Z_Command ?? (_Z_Command = new RelayCommand(x => do_Z_(x), x => can_Yes) { GestureKey = Key.Z, GestureModifier = ModifierKeys.None });

    ICommand _GoFasterCommand; public ICommand GoFasterCommand => _GoFasterCommand ?? (_GoFasterCommand = new RelayCommand(x => doGoFaster(x)) { GestureKey = Key.OemPlus });
    ICommand _GoFasterComman_; public ICommand GoFasterComman_ => _GoFasterComman_ ?? (_GoFasterComman_ = new RelayCommand(x => doGoFaster(x)) { GestureKey = Key.Add });
    ICommand _GoSlowerComman_; public ICommand GoSlowerComman_ => _GoSlowerComman_ ?? (_GoSlowerComman_ = new RelayCommand(x => doGoSlower(x)) { GestureKey = Key.Subtract });
    ICommand _GoSlowerCommand; public ICommand GoSlowerCommand => _GoSlowerCommand ?? (_GoSlowerCommand = new RelayCommand(x => doGoSlower(x)) { GestureKey = Key.OemMinus });


    void doGoSlower(object o) { flashKeyInfo(); if ((VPModel?.CrntMU?.SpeedIdx ?? 1) > 0)                   /**/ --VPModel.CrntMU.SpeedIdx; setSpeed(); _vpcPlayer.Position -= TimeSpan.FromSeconds(.5); } // seems to reset the playback ratio to 1.
    void doGoFaster(object o) { flashKeyInfo(); if ((VPModel?.CrntMU?.SpeedIdx ?? 1) < _speeds.Length - 1)  /**/ ++VPModel.CrntMU.SpeedIdx; setSpeed(); _vpcPlayer.Position -= TimeSpan.FromSeconds(.5); }

    void doTglStrcS(object o) { flashKeyInfo(); _vpcPlayer.Stretch = _vpcPlayer.Stretch == Stretch.Fill ? _vpcPlayer.Stretch = Stretch.UniformToFill : Stretch.Fill; }
    bool doAsk_CanNext(object player)
    {
      var dlgwin = new UsedMediaChoicesWindow { MediaFile = _vpcPlayer.Source.LocalPath };     //dlgwin.Owner = ((FrameworkElement)(((FrameworkElement)(this)).Parent)).Parent as Window;
      dlgwin.Owner = Window;
      var rv = dlgwin.ShowDialog();

      switch (dlgwin.Decision)
      {
        default: return true;
        case Dcsn.Delete: VPModel.CrntMU.DeleteFileFixMetaFile(_vpcPlayer.Source.LocalPath); return true;
        case Dcsn.MoveTo: moveToFolder(dlgwin.SubFolder); return true;
        case Dcsn.ShutDn: App.Current.Shutdown(); return true;
        case Dcsn.NoMore: _popMenuAtEnd = false; return true;
        case Dcsn.Replay: doHome(null); return false;
      }
    }

    void popupAidedFileMove(string curName, string newName)
    {
      while (true)
      {
        try
        {
          var prevMU = VPModel.CrntMU;

          moveNext(_fvc);
          File.Move(curName, newName);

          //		prevMU.PathFileCur = newName;
          //		prevMU.MoveToFolder();

          //					var lclPath = _player.Source.LocalPath;
          //VPModel.CrntMU.MoveToFolder("ViewLater");
          //var next = VPModel.MoveNext(lclPath);

          return;
        }
        catch (Exception ex)
        {
          var msg =
              $"\r\nError in {MethodInfo.GetCurrentMethod().DeclaringType.Name}.{MethodInfo.GetCurrentMethod().Name}():\r\n{ex.Message}\r\n{(ex.InnerException == null ? "" : ex.InnerException.Message)}\r\n";
          if (MessageBox.Show(msg, "Retry ?", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) != MessageBoxResult.OK)
            return;// false;					

          _vpcPlayer.Close(); //?
        }
      }
    }

    void doHome(object o) { flashKeyInfo(); _vpcPlayer.Position = TimeSpan.FromSeconds(0); }
    void doEnd_(object o) { flashKeyInfo(); _vpcPlayer.Position = _vpcPlayer.NaturalDuration.HasTimeSpan ? _vpcPlayer.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromSeconds(5)) : TimeSpan.FromSeconds(0); }

    public bool canPrevBmkCmd => VPModel.CrntMU != null && VPModel.CrntMU.Position.HasValue && VPModel.CrntMU.Bookmarks.FirstOrDefault(r => r.PositionSec < VPModel.CrntMU.Position.Value.TotalSeconds) != null;
    public bool canNextBmkCmd => VPModel.CrntMU != null && VPModel.CrntMU.Position.HasValue && VPModel.CrntMU.Bookmarks.FirstOrDefault(r => r.PositionSec > VPModel.CrntMU.Position.Value.TotalSeconds) != null;
    void do_PrevBmkCmd(object o)
    {
      var p = VPModel.CrntMU.Bookmarks.Where(r => r.PositionSec < VPModel.CrntMU.Position.Value.TotalSeconds).Max(r => r.PositionSec);
      VPModel.CrntMU.Position = _vpcPlayer.Position = TimeSpan.FromSeconds(p - 1);
    }
    void do_NextBmkCmd(object o)
    {
      var p = VPModel.CrntMU.Bookmarks.Where(r => r.PositionSec > VPModel.CrntMU.Position.Value.TotalSeconds).Min(r => r.PositionSec) + .01;
      VPModel.CrntMU.Position = _vpcPlayer.Position = TimeSpan.FromSeconds(p);
    }

    void do_F1_(object o) => flashKeyInfo();
    void do_F2_(object o)
    {
      flashKeyInfo();
      var w = new RenameWindow(Window, Path.GetFileNameWithoutExtension(VPModel.CrntMU.PathFileCur));
      var rv = w.ShowDialog();
      if (rv == true)
      {
        _vpModel.CrntMU.Rename(w.Filename);
      }
    }
    void do_F3_(object o)
    {
      flashKeyInfo();
      var w = new SrchTextBoxWindow(this)
      {
        Owner = Window
      };
      w.ShowDialog();
    }
    void do_F4_(object o)
    {
      flashKeyInfo();
      try
      {
        //r mf = VPModel?.CrntMU?.PathFileCur ?? _fvc.CurMediaFile2;
        var mf = _fvc.CurMediaFile2 ?? VPModel?.CrntMU?.PathFileCur;
        if (!string.IsNullOrEmpty(mf) && !File.Exists(mf))
        {
          var dir = Path.GetDirectoryName(mf);
          if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
          Process.Start(new ProcessStartInfo(dir));
          Bpr.BeepOk();
        }
        else
          Synth.SpeakAsync("File is null or already exists.");
      }
      catch (Exception ex) { BotmCentrMsg = ex.Message; Debug.WriteLine(ex, ":>Error"); }
    }
    void do_F5_(object o) => flashKeyInfo();
    void do_F6_(object o) => flashKeyInfo();
    void do_F7_(object o) => flashKeyInfo();
    void do_F8_(object o)
    {
      flashKeyInfo();
      var win = o as Window;

      win.Width = _vpcPlayer.NaturalVideoWidth;
      win.Height = _vpcPlayer.NaturalVideoHeight;
      _vpcPlayer.Stretch = Stretch.None;

      if (win.Left + win.Width > SystemParameters.WorkArea.Width) win.Left = SystemParameters.WorkArea.Width - win.Width;
      if (win.Top + win.Height > SystemParameters.WorkArea.Height) win.Top = SystemParameters.WorkArea.Height - win.Height;
    }
    void do_F9_(object o)
    {
      flashKeyInfo();
      var win = o as Window;
      win.Left = (SystemParameters.WorkArea.Width - win.Width) * .5;
      win.Top = (SystemParameters.WorkArea.Height - win.Height) * .5; //SystemParameters.PrimaryScreenWidth SystemParameters.PrimaryScreenHeight
    }
    void do_F10_(object o) { flashKeyInfo(); var win = o as Window; }
    void do_F11_(object o) { flashKeyInfo(); var win = o as Window; }
    void do_F12_(object o) => do_F_(o);


    void do_D0_(object o) { flashKeyInfo(); VPModel.CrntMU.SpeedIdx = _v1x; setSpeed(); }
    void do_D1_(object o) { flashKeyInfo();                             /**/ setSpeed(); }
    void do_D2_(object o)
    {
      flashKeyInfo(); if (!string.IsNullOrEmpty(Clipboard.GetText()))
      {
        _vpModel.CrntMU.OrgHttpLink = VPModel.CrntMU.OrgHttpLink = Clipboard.GetText();
        _vpModel.CrntMU.SaveLink(VPModel.CrntMU.OrgHttpLink);
        Synth.SpeakAsync($"Link set to {Clipboard.GetText()}");
      }
      else Synth.SpeakAsync("Clipboard is empty; no http link found.");
    }
    void do_D3_(object o) => flashKeyInfo();
    void do_D4_(object o) => flashKeyInfo();
    void do_D5_(object o) => flashKeyInfo();
    void do_D6_(object o) => flashKeyInfo();
    void do_D7_(object o) => flashKeyInfo();
    void do_D8_(object o) { flashKeyInfo(); _isLooping = !_isLooping; if (_isLooping) Bpr.Beep1of2(); else Bpr.Beep2of2(); }
    void do_D9_(object o) => do_D1_(o);

    void do_A_(object o) { flashKeyInfo(); _vpcPlayer.Position = _lastStartPosn; }
    void do_B_(object o)
    {
      _vpcPlayer.Pause();
      flashKeyInfo();
      var cp = _vpcPlayer.Position;
      Bpr.BeepClk();
      var w = new RenameWindow(Window);
      var rv = w.ShowDialog();
      if (rv != true)
        return;

      removeNearBookmarksInRangeOf(sec: 5);
      var nbm = new MuBookmark { AddedAt = DateTime.Now, AddedBy = Environment.UserName, ID = VPModel.CrntMU.Bookmarks.Count, PositionSec = cp.TotalSeconds - (IsPlaying ? .333 : .0), Note = w.Filename };
      VPModel.CrntMU.Bookmarks.Add(nbm);
      VPModel.CrntMU.OrderByPos();
      updatePosnMuStatsAndSave(cp);
      playAndStartCounters();
    }
    void do_C_(object o) => flashKeyInfo();
    void do_D_(object o) { flashKeyInfo(); _vpcPlayer.Position = _vpcPlayer.NaturalDuration.HasTimeSpan ? _vpcPlayer.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromMilliseconds(150)) : TimeSpan.FromSeconds(0); }
    void do_E_(object o) { flashKeyInfo(); Process.Start(new ProcessStartInfo(Path.GetDirectoryName(_vpcPlayer.Source.LocalPath))); }
    void do_F_(object fvc)
    {
      var fv = ((FolderViewUsrCtrl)fvc) ?? _fvc;
      if (fv.Visibility == Visibility.Visible)
      {
        fv.Visibility = Visibility.Hidden;
        Bpr.BeepOk();
      }
      else
      {
        Bpr.Beep1of2();
        if (ChromeVisibility != Visibility.Visible)
          ChromeVisibility = Visibility.Visible;
        fv.Visibility = Visibility.Visible;
        fv.CurMediaFile2 = VPModel.CrntMU?.PathFileCur ?? _vpcPlayer.Source.LocalPath;
        fv.LoadCurFolder(false);
        Bpr.Beep2of2();
      }
    }
    void do_G_V_2x_1x_tgl(object o) { flashKeyInfo(); VPModel.CrntMU.SpeedIdx = VPModel.CrntMU.SpeedIdx == _v1x ? _v2x : _v1x; setSpeed(); }
    void do_H_(object o) { if (!string.IsNullOrEmpty(BotmCentrMsg)) { BotmCentrMsg = ""; return; } ChromeVisibility = ChromeVisibility == Visibility.Visible ? ChromeVisibility = Visibility.Collapsed : ChromeVisibility = Visibility.Visible; /*if (ChromeVisibility != Visibility.Visible) Bpr.Beep1of2(); else Bpr.Beep2of2();*/ }
    void do_I_(object o) { _vpcPlayer.Stretch = Stretch.Uniform; if (PlayerMargin < 500) PlayerMargin += 20; flashKeyInfo($"PlayerMargin: {PlayerMargin}"); }
    void do_J_(object o) => do_H_(o);
    void do_K_(object o) { if (PlayerMargin > -500) PlayerMargin -= 20; flashKeyInfo($"PlayerMargin: {PlayerMargin}"); }
    void do_L_(object o) { flashKeyInfo(); updatePosnMuStatsAndSave(_vpcPlayer.Position); }
    void do_M_(object o)
    {
      flashKeyInfo();
      _vpcPlayer.Stretch = Stretch.Uniform;

      if (_vpcPlayer.Height == _vpcPlayer.NaturalVideoHeight)
        _vpcPlayer.Height = 2 * _vpcPlayer.NaturalVideoHeight;
      else if (_vpcPlayer.Height == 2 * _vpcPlayer.NaturalVideoHeight)
        _vpcPlayer.Height = 3 * _vpcPlayer.NaturalVideoHeight;
      else
        _vpcPlayer.Height = _vpcPlayer.NaturalVideoHeight;
    }
    void do_N_(object o) { flashKeyInfo(); Debug.Assert(false); }
    void do_O_(object o) { flashKeyInfo(); playUsingExternalPlayer(); }
    void do_P_(object o) { flashKeyInfo(); flashKeyInfo("No action assigned"); }
    void do_Q_(object o) { flashKeyInfo(); _vpcPlayer.Volume = _vpcPlayer?.Volume == 1 ? .1 : _vpcPlayer?.Volume == .1 ? .01 : 1; }
    void do_R_(object o)
    {
      flashKeyInfo();
      var w = new DailyViewTimePopup
      {

        //w.EOTimeMessage = _EOTMsg;
        Owner = (Window)o
      };
      if (w.ShowDialog() != true) return;
    }
    void do_S_(object o)
    {
      flashKeyInfo();
      _vpcPlayer.Stretch = //_player.Stretch == Stretch.None ? _player.Stretch = Stretch.Uniform : Stretch.None;
        (int)_vpcPlayer.Stretch >= 3 ? _vpcPlayer.Stretch = Stretch.None : (Stretch)(1 + (int)_vpcPlayer.Stretch);
    }
    void do_T_(object o)
    {
      flashKeyInfo();
      var w = new TimerDataPopup
      {
        EOTimeMessage = _EOTMsg,
        Owner = (Window)o
      };
      if (w.ShowDialog() != true) return;

      _tillTime = DateTime.Now.AddMinutes(w.MinutesLeft);
      _EOTMsg = w.EOTimeMessage;
    }
    void do_U_(object o) { flashKeyInfo(); var rv = intplay(); }
    void do_V_(object fvc) => moveToFolder("ViewLater");
    void do_W_(object o) { flashKeyInfo(); playUsingExternalPlayer(@"C:\Program Files (x86)\Windows Media Player\wmplayer.exe"); }
    void do_X_(object o)
    {
      flashKeyInfo();
      var win = o as Window;
      //var PrgPosition = _player.PrgPosition;

      if (win.WindowState != WindowState.Maximized)
      {
        win.Background = Brushes.Black;
        win.WindowState = WindowState.Maximized;

        _vpcPlayer.StretchDirection = StretchDirection.Both;
        _vpcPlayer.Stretch = Stretch.Uniform;
      }
      else
      {
        win.WindowState = WindowState.Normal;
      }
      //_player.PrgPosition = PrgPosition;
    }
    void do_Y_(object fvc)
    {
      Bpr.Beep1of2(); var fv = ((FolderViewUsrCtrl)fvc) ?? _fvc;
      fv.CurMediaFile2 = VPModel.CrntMU?.PathFileCur ?? _vpcPlayer.Source.LocalPath;
      fv.LoadOneDrCash(false); fv.Visibility = Visibility.Visible; Bpr.Beep2of2();
    }
    void do_Z_(object win) { if (win != null && win is Window) { IsTopmost = (win as Window).Topmost = !(win as Window).Topmost; } if (IsTopmost) Bpr.Beep1of2(); else Bpr.Beep2of2(); }

    async Task<int> intplay()
    {
      _intervalPlay++;
      if (_intervalPlay == 1)
      {
        _posA = _vpcPlayer.Position;
      }
      else if (_intervalPlay == 2)
      {
        _durnToB = _vpcPlayer.Position - _posA;
        do
        {
          _vpcPlayer.Position = _posA;
          //?_player.Play();
          await Task.Delay(_durnToB);
        } while (_intervalPlay == 2);
      }
      else
      {
        _intervalPlay = 0;
      }

      return _intervalPlay;
    }
    void moveToFolder(string folder)
    {
      pauseCollectViewTimeAndUpdatePosnMuStatsAndSave();
      var lclPath = _vpcPlayer.Source.LocalPath;
      VPModel.CrntMU.MoveToFolder(folder);
      var next = VPModel.MoveNext(lclPath);
      if (!string.IsNullOrEmpty(next))
      {
        PlayNewFile(next);
        _fvc.MovCurrentToNext();
      }

      if (_fvc.Visibility == Visibility.Visible)
        _fvc.LoadCurFolder(false);
    }

    public bool HasPlayedEnough
    {
      get
      {
        var minSecRequired = TimeSpan.FromSeconds(15);
        return //(DateTime.Now - _lastPlayStartAt) > minSecRequired &&
                _vpcPlayer.Position > minSecRequired ||
                (_vpcPlayer.NaturalDuration.HasTimeSpan && _vpcPlayer.NaturalDuration.TimeSpan < minSecRequired);
      }
    }

    void movUpDir(object fvc) { pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); var prev = VPModel.MovUpDir(_vpcPlayer.Source.LocalPath); if (!string.IsNullOrEmpty(prev)) { PlayNewFile(prev); _fvc.MovCurrentToPrev(); } }

    //void movePrev(object fvc) { pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); var prev = VPModel.MovePrev(_player.Source.LocalPath); if (!string.IsNullOrEmpty(prev)) { PlayNewFile(prev); _fvc.MovCurrentToPrev(); } }//j2016
    //void moveNext(object fvc) { pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); var next = VPModel.MoveNext(_player.Source.LocalPath); if (!string.IsNullOrEmpty(next)) { PlayNewFile(next); _fvc.MovCurrentToNext(); } }//j2016
    void movePrev(object fvc) { pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); _fvc.MovCurrentToPrev(); PlayNewFile(_fvc.CurMediaFile2); }//j2016
    void moveNext(object fvc) { pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); _fvc.MovCurrentToNext(); PlayNewFile(_fvc.CurMediaFile2); }//j2016

    void meetPrev(object fvc) { pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); _isJumpingTo = _vpcPlayer.Position; var prev = VPModel.MovePrev(_vpcPlayer.Source.LocalPath); if (!string.IsNullOrEmpty(prev)) { PlayNewFile(prev); _fvc.MovCurrentToPrev(); } }
    void meetNext(object fvc) { pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); _isJumpingTo = _vpcPlayer.Position; var next = VPModel.MoveNext(_vpcPlayer.Source.LocalPath); if (!string.IsNullOrEmpty(next)) { PlayNewFile(next); _fvc.MovCurrentToNext(); } }

    void moveLeftN(object o) { flashKeyInfo(); _vpcPlayer.Position -= TimeSpan.FromSeconds(5); ((VPModel)o).MoveLeft(); }
    void moveRghtN(object x) => _vpcPlayer.Position += TimeSpan.FromSeconds(5);
    void moveLeftC(object o) { flashKeyInfo(); _vpcPlayer.Position -= TimeSpan.FromMinutes(1); ((VPModel)o).MoveLeft(); }
    void moveRghtC(object x) => _vpcPlayer.Position += TimeSpan.FromMinutes(1);
    void moveLeftA(object o) { flashKeyInfo(); _vpcPlayer.Position -= TimeSpan.FromMinutes(5); ((VPModel)o).MoveLeft(); }
    void moveRghtA(object x) => _vpcPlayer.Position += TimeSpan.FromMinutes(5);
    void moveLeftS(object o) { flashKeyInfo(); _vpcPlayer.Position -= TimeSpan.FromSeconds(.5); ((VPModel)o).MoveLeft(); }
    void moveRghtS(object x) => _vpcPlayer.Position += TimeSpan.FromSeconds(.5);

    public bool canTglPlyPs => true;
    public bool canGoFaster => true;
    public bool canGoSlower => true;
    public bool canTglStrch => true;

    public bool canMovUpDir => true;  //todo:
    public bool canMoveNext => _vpcPlayer.Source != null && File.Exists(_vpcPlayer.Source.LocalPath);
    public bool canMovePrev => _vpcPlayer.Source != null && File.Exists(_vpcPlayer.Source.LocalPath);

    public bool canMoveLeftN => _vpcPlayer.Position > TimeSpan.FromSeconds(5);
    public bool canMoveRghtN => _vpcPlayer.NaturalDuration - _vpcPlayer.Position > TimeSpan.FromSeconds(5);
    public bool canMoveLeftC => _vpcPlayer.Position > TimeSpan.FromMinutes(1);
    public bool canMoveRghtC => _vpcPlayer.NaturalDuration - _vpcPlayer.Position > TimeSpan.FromMinutes(1);
    public bool canMoveLeftA => _vpcPlayer.Position > TimeSpan.FromMinutes(5);
    public bool canMoveRghtA => _vpcPlayer.NaturalDuration - _vpcPlayer.Position > TimeSpan.FromMinutes(5);
    public bool canMoveLeftS => _vpcPlayer.Position > TimeSpan.FromSeconds(.5);
    public bool canMoveRghtS => _vpcPlayer.NaturalDuration - _vpcPlayer.Position > TimeSpan.FromSeconds(.5);
    public bool canDeleteMU => VPModel != null && VPModel.CrntMU != null && !string.IsNullOrEmpty(VPModel.CrntMU.PathFileCur) && File.Exists(VPModel.CrntMU.PathFileCur);

    public bool can_B_ => true; // _player.Position.TotalSeconds > 15; } }  <= for splits can be anywhere
    public bool can_F_ => VPModel.CrntMU != null && !string.IsNullOrEmpty(VPModel.CrntMU.PathFileCur);
    public bool can_G_ => VPModel.CrntMU != null && !string.IsNullOrEmpty(VPModel.CrntMU.PathFileCur);
    public bool can_L_ => VPModel.CrntMU != null;
    public bool can_F2_ => VPModel.CrntMU != null;

    public bool can_Yes => true;
    //>>

    static DependencyObject GetParentWindow(object s)
    {
      var parent = VisualTreeHelper.GetParent(s as DependencyObject);
      while (!(parent is Window)) { parent = VisualTreeHelper.GetParent(parent); }
      return parent;
    }

    async void doTglPlyPs(object o)
    {
      if (await isPlayingAsync()) { IsPlaying = false; pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); }
      else { IsPlaying = true; playAndStartCounters(); }
    }
    void playAndStartCounters()
    {
      _vpcPlayer.Play();
      _vpcPlayer.Cursor = Cursors.IBeam;
      _lastPlayStartAt = DateTime.Now;

      setSpeed();
    }

    void setSpeed() // needed setting to some other speed before it will comply with the new speed value (~2018)
    {
      int delayMs = 200, pauseMs = 25;
      double x = 2.5, y = _speeds[VPModel.CrntMU.SpeedIdx];

      if (_vpcPlayer.SpeedRatio == y)        return; // avoid this terrible pause. Aug`20

      Trace.WriteLine($"\n** 0) dPos: {(_vpcPlayer.Position - VPModel.CrntMU.Position.Value).TotalMilliseconds:N0}ms,  dSpeed: {_vpcPlayer.SpeedRatio - _speeds[VPModel.CrntMU.SpeedIdx]} (==0 always!!!)"); //Jan 2018: 2018 Sep: uncom-d:             if (_vpcPlayer.SpeedRatio != _speeds[VPModel.CrntMU.SpeedIdx]) _vpcPlayer.SpeedRatio  = _speeds[VPModel.CrntMU.SpeedIdx];

      Task.Run    /**/(async () => await Task.Delay(delayMs)).ContinueWith(_ => _vpcPlayer.SpeedRatio = x, TaskScheduler.FromCurrentSynchronizationContext()).ContinueWith(_ => Trace.WriteLine($"** 1)                      {_vpcPlayer.SpeedRatio} <= {x}?"))
          .ContinueWith(async _ => await Task.Delay(pauseMs)).ContinueWith(_ => _vpcPlayer.SpeedRatio = y, TaskScheduler.FromCurrentSynchronizationContext()).ContinueWith(_ => Trace.WriteLine($"** 2)                      {_vpcPlayer.SpeedRatio} <= {y}?"));
    }

    void pauseCollectViewTimeAndUpdatePosnMuStatsAndSave()
    {
      _vpcPlayer.Pause();
      _vpcPlayer.Cursor = Cursors.Arrow;
      logViewTimeAndSaveStatsIfPlayedEnough();
      BotmCentrMsg = "";
    }
    void logViewTimeAndSaveStatsIfPlayedEnough()
    {
      if (_lastPlayStartAt != DateTime.MaxValue)
      {
        _viewTime = _viewTime.Add(DateTime.Now - _lastPlayStartAt); // Debug.WriteLine("::>{0:h\\:mm\\:ss} _viewTime for {1}", _viewTime, _player.Source.LocalPath);
        ViewTimeLog.Log(_viewTime, _daySsnStarted); _viewTime = TimeSpan.FromSeconds(0);

        if (can_L_ && HasPlayedEnough) updatePosnMuStatsAndSave(); else Debug.WriteLine("::>_viewTime NOT ENOUGH for persisting: {0} ", _vpcPlayer.Source.LocalPath);
        _lastPlayStartAt = DateTime.MaxValue;
      }
    }
    void updatePosnMuStatsAndSave(TimeSpan? newPosition = null)
    {
      if (newPosition != null) VMPosn = newPosition.Value;
      VPModel.CrntMU.LastPeekAt = DateTime.Now;
      VPModel.CrntMU.LastPeekPC = Environment.MachineName;

      MediaUnit.SaveMetaData(VPModel.CrntMU);
    }
    void playUsingExternalPlayer(string optExe = null)
    {
      string wmp = @"C:\Program Files (x86)\Windows Media Player\wmplayer.exe", t64 = @"C:\Program Files\MpcStar\mpcstar.exe", t86 = @"C:\Program Files (x86)\MpcStar\mpcstar.exe", mpc = @"C:\Program Files\MPC-HC\mpc-hc64.exe",
       exe = (optExe != null && File.Exists(optExe)) ? optExe : File.Exists(mpc) ? mpc : File.Exists(t86) ? t86 : File.Exists(t64) ? t64 : File.Exists(wmp) ? wmp : null;

      pauseCollectViewTimeAndUpdatePosnMuStatsAndSave();

      if (exe == null)
      {
        MessageBox.Show("No suitable players found");
      }
      else
      {
        var prevPos = _vpcPlayer.Position;
        var stopwatch = Stopwatch.StartNew();

        if (exe == mpc) preSendMpcToCurPos();
        else if (exe == wmp) preSendWmpToCurPos();
        else preSendStrToCurPos();

        var pInfo = new ProcessStartInfo(exe,
            $"\"{_vpcPlayer.Source.LocalPath}\" /start {(long)prevPos.TotalMilliseconds - 2500}")
        {
          WindowStyle = ProcessWindowStyle.Normal
        };

        Task.Factory.StartNew(() =>
        {
          var process = Process.Start(pInfo);
          while (process.HasExited == false)
            Thread.Sleep(333);//..Trace.WriteLine(">>>>>>>> Waiting ...");
                              //..Trace.WriteLine(">>>>>>>> Waiting ... DONE!");

                }).ContinueWith(_ =>
                {
                  var externalPlayerViewTime = stopwatch.Elapsed.Add(TimeSpan.FromSeconds(-5));
                  if (exe == mpc)
                  {
                    for (var i = 0; i < 30; i++)
                    {
                      Trace.WriteLine($">>>>>>>> GetLastCurPosInSec ... {i} out of 30.");
                      var fn0 = (string)Registry.GetValue(MediaUnit._key, "File Name 0", "");
                      if (string.Compare(fn0, _vpcPlayer.Source.LocalPath, true) == 0)
                        break;
                      Thread.Sleep(100);
                    }
                    var extPos = TimeSpan.FromTicks(Convert.ToInt64(Registry.GetValue(MediaUnit._key, "File ts 0", 0L)));
                    externalPlayerViewTime = extPos.Subtract(prevPos);
                    Debug.WriteLine(">>>>>>>> old-new pos {0} - {1} = {2} ", prevPos, extPos, externalPlayerViewTime);
                  }
                  else if (exe == wmp) { }
                  else { }

                  if (5 < externalPlayerViewTime.TotalSeconds && externalPlayerViewTime.TotalSeconds < 7200)
                  {
                    _viewTime = _viewTime.Add(externalPlayerViewTime);
                    _vpcPlayer.Position = _vpcPlayer.Position.Add(externalPlayerViewTime);
                  }

                  playAndStartCounters();
                }, TaskScheduler.FromCurrentSynchronizationContext());
      }
    }
    void removeNearBookmarksInRangeOf(int sec)
    {
      MuBookmark bmk;
      while ((bmk = VPModel.CrntMU.Bookmarks.FirstOrDefault(r => Math.Abs(r.PositionSec - _vpcPlayer.Position.TotalSeconds) < sec)) != null)
      {
        VPModel.CrntMU.Bookmarks.Remove(bmk);
      }
    }

    void preSendStrToCurPos()
    {
    }
    void preSendMpcToCurPos()
    {
    }
    void preSendWmpToCurPos()
    {
    }

    void initPlayer(object wmp)
    {
      if (_vpcPlayer == null)
      {
        _vpcPlayer = (MediaElement)wmp;
        _vpcPlayer.MediaEnded += player_MediaEnded;
        _vpcPlayer.MediaOpened += player_MediaOpened;
        _vpcPlayer.MediaFailed += player_MediaFailed;
        _vpcPlayer.LoadedBehavior =
        _vpcPlayer.UnloadedBehavior = MediaState.Manual;
      }
    }
    async Task<bool> isPlayingAsync()
    {
      if (_vpcPlayer == null) return false;

      var prev = _vpcPlayer.Position;
      await Task.Delay(99);
      return _vpcPlayer.Position != prev;
    }

    void player_MediaFailed(object s, ExceptionRoutedEventArgs e) => BotmCentrMsg = $"MediaFailed:\n{e.ErrorException.InnermostMessage()}";
    void player_MediaOpened(object s, RoutedEventArgs e)
    {
      //restore from prev session instead: if (((Window)GetParentWindow(s)).WindowState != WindowState.Maximized) do_F8_(GetParentWindow(s));

      if (string.Compare(_vpcPlayer.Source.LocalPath, VPModel.CrntMU.PathFileCur, true) != 0) logViewTimeAndSaveStatsIfPlayedEnough(); // log prev file's stats.

      VPModel.CrntMU = MediaUnit.LoadMetaData(_vpcPlayer.Source.LocalPath, _vpcPlayer.NaturalDuration.TimeSpan, _vpcPlayer.NaturalVideoHeight, _vpcPlayer.NaturalVideoWidth);
      VPModel.CrntMU.Duration = _vpcPlayer.NaturalDuration.TimeSpan;
      if (_isJumpingTo != null) { _vpcPlayer.Position = _isJumpingTo.Value; _isJumpingTo = null; }
      else if (VPModel.CrntMU.Position != null) { _lastStartPosn = _vpcPlayer.Position = VPModel.CrntMU.Position.Value; }
      VPModel.CrntMU.PassedQA = true;

      setSpeed(); // _vpcPlayer.SpeedRatio  = _speeds[VPModel?.CrntMU?.SpeedIdx ?? 1];

      _vpcPlayer.Cursor = Cursors.Cross;
      _lastPlayStartAt = DateTime.Now;
    }
    void player_MediaEnded(object s, RoutedEventArgs e)
    {
      _vpcPlayer.Cursor = Cursors.Arrow;

      VPModel.CrntMU.AddAuditionResetPosToStart();

      pauseCollectViewTimeAndUpdatePosnMuStatsAndSave(); // since PrgPosition==0 just logs view time only.

      _vpcPlayer.Position = TimeSpan.FromSeconds(0); // trying Aug`20

      if (_isLooping)
      {
        _vpcPlayer.Position = TimeSpan.FromSeconds(0);
        playAndStartCounters();
        return;
      }

      if (_popMenuAtEnd && (Environment.UserName.Contains("igid") || Environment.UserName.Contains("lex")))
      {
        if (!doAsk_CanNext(_vpcPlayer)) return;
      }

      if (_isAutoNext && canMoveNext)
        moveNext(_fvc);
    }

    public void PlayNewFileOrFolder(string mediaFileOrFolder)
    {
      if (File.Exists(mediaFileOrFolder))
        PlayNewFile(mediaFileOrFolder);
      else if (Directory.Exists(mediaFileOrFolder))
        PlayNewFile(VPModel.MoveNext(mediaFileOrFolder));
    }
    public void PlayNewFile(string mediaFile)
    {
      if (string.IsNullOrEmpty(mediaFile)) return;

      VPModel.CrntMU = MediaUnit.LoadMetaData(mediaFile);

      if (!File.Exists(VPModel.CrntMU.PathFileCur) && !string.IsNullOrEmpty(VPModel.CrntMU.OrgHttpLink))
      {
        Process.Start(VPModel.CrntMU.OrgHttpLink);
        return;
      }

      //if (VPModel.CrntMU.PassedQA == false && VPModel.CrntMU.LastPeekPC == Environment.MachineName) if (MessageBox.Show(mediaFile + "\n\ncrashed before...\n\n\n\t Continue?", "Continue?", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return;

      VPModel.CrntMU.PassedQA = false;
      updatePosnMuStatsAndSave(); // pre-saving for crash analisys

      TglPlyPsCommand.Execute(_vpcPlayer);
    }
    internal void LogSessionViewTime(DateTime dayStarted) => pauseCollectViewTimeAndUpdatePosnMuStatsAndSave();
    protected override async Task ClosingVM()
    {
      await Task.Delay(9);

      logViewTimeAndSaveStatsIfPlayedEnough();

      if (!_vpcPlayer.NaturalDuration.HasTimeSpan) return;
      if (_vpcPlayer.NaturalDuration.TimeSpan.TotalSeconds == 0) return;

      var percentLeft = 100.0 * (_vpcPlayer.NaturalDuration.TimeSpan.TotalSeconds - _vpcPlayer.Position.TotalSeconds) / _vpcPlayer.NaturalDuration.TimeSpan.TotalSeconds;
      if (percentLeft < 7)
      {
        VPModel.CrntMU.AddAuditionResetPosToStart();
        MediaUnit.SaveMetaData(VPModel.CrntMU);
        if (_isAutoNext && canMoveNext)
          moveNext(_fvc);
      }
    }

    void flashKeyInfo(string msg = "") { if (ChromeVisibility == Visibility.Visible) return; Task.Run(async () => { ChromeVisibility = Visibility.Visible; Top_CentrMsg = msg; await Task.Delay(2500); }).ContinueWith(_ => { Top_CentrMsg = ""; ChromeVisibility = Visibility.Collapsed; }, TaskScheduler.FromCurrentSynchronizationContext()); }

    public void FlashAllControlls() => flashKeyInfo();

    public bool _ignorePlayerPosnMove { get; set; }
    public Window Window { get; internal set; }
  }
}
///tu: whole word match: x:Name=".*?"
///todo: How to Select Audio Tracks in Various Languages in Windows Store Apps  http://www.c-sharpcorner.com/UploadFile/7e39ca/how-to-select-audio-tracks-in-different-languages-in-windows/ 
///