using MVVM.Common;
using System.Windows.Input;
using Windows.Media.Playback;
using Windows.Storage;
namespace ABR.VMs
{
    public partial class AbrVM
    {
        ICommand _ClearLibsList; /**/public ICommand ClearLibsList { get { return _ClearLibsList ?? (_ClearLibsList = new RelayCommand(x => LibLst.Clear())); } }
        ICommand _ClearMRUsList; /**/public ICommand ClearMRUsList { get { return _ClearMRUsList ?? (_ClearMRUsList = new RelayCommand(x => onClearAllAsk(MruLst))); } }
        ICommand _RemoveDelsMru; /**/public ICommand RemoveDelsMru { get { return _RemoveDelsMru ?? (_RemoveDelsMru = new RelayCommand(x => onRemovNonExst(MruLst))); } }
        ICommand _RemoveDelsLib; /**/public ICommand RemoveDelsLib { get { return _RemoveDelsLib ?? (_RemoveDelsLib = new RelayCommand(x => onRemovNonExst(LibLst))); } }
        ICommand _LoadVideAudio; /**/public ICommand LoadVideAudio { get { return _LoadVideAudio ?? (_LoadVideAudio = new RelayCommand(x => onLoadLib_void(KnownFolderId.AllAppMods), x => CanLoadLibs)); } }
        ICommand _ItemMenuPoc00; /**/public ICommand ItemMenuPoc00 { get { return _ItemMenuPoc00 ?? (_ItemMenuPoc00 = new RelayCommand(x => onItemMenuPoc00(x), x => true)); } }
        ICommand _PlayNowThsMid; /**/public ICommand PlayNowThsMid { get { return _PlayNowThsMid ?? (_PlayNowThsMid = new RelayCommand(x => onPlayNowThsMid(x), x => true)); } }
        ICommand _SetThumbsMrus; /**/public ICommand SetThumbsMrus { get { return _SetThumbsMrus ?? (_SetThumbsMrus = new RelayCommand(x => onSetThumbsMrus(), x => true)); } }
        ICommand _SetThumbsLibs; /**/public ICommand SetThumbsLibs { get { return _SetThumbsLibs ?? (_SetThumbsLibs = new RelayCommand(x => onSetThumbsLibs(), x => true)); } }
        ICommand _RefreshFromFS; /**/public ICommand RefreshFromFS { get { return _RefreshFromFS ?? (_RefreshFromFS = new RelayCommand(x => onRefreshFromFS(), x => true)); } }
        ICommand _RemoveCurSlct; /**/public ICommand RemoveCurSlct { get { return _RemoveCurSlct ?? (_RemoveCurSlct = new RelayCommand(x => onRemoveCurSlct(), x => CanElimi)); } }
        ICommand _DeleteCurSlct; /**/public ICommand DeleteCurSlct { get { return _DeleteCurSlct ?? (_DeleteCurSlct = new RelayCommand(x => onDeleteCurSlct(), x => CanElimi)); } }
        ICommand _RemoveThisMid; /**/public ICommand RemoveThisMid { get { return _RemoveThisMid ?? (_RemoveThisMid = new RelayCommand(x => onRemoveThisMid(x), x => true)); } }
        ICommand _DeleteThisMid; /**/public ICommand DeleteThisMid { get { return _DeleteThisMid ?? (_DeleteThisMid = new RelayCommand(x => onDeleteThisMid(x), x => true)); } }

        ICommand _JumpArnd;       /**/public ICommand JumpArnd { get { return _JumpArnd ?? (_JumpArnd = new RelayCommand(x => onJumpArnd(x), x => true)); } }
        ICommand _GoSlower;       /**/public ICommand GoSlower { get { return _GoSlower ?? (_GoSlower = new RelayCommand(x => onGoSlower(), x => true)); } }
        ICommand _GoSpeed0;       /**/public ICommand GoSpeed0 { get { return _GoSpeed0 ?? (_GoSpeed0 = new RelayCommand(x => { jump(); mp_Vm.PlaybackSession.PlaybackRate = PlayRate = .2; updateSaveSettings(PlayRate.ToString()); }, x => true)); } }
        ICommand _GoSpeed1;       /**/public ICommand GoSpeed1 { get { return _GoSpeed1 ?? (_GoSpeed1 = new RelayCommand(x => { jump(); mp_Vm.PlaybackSession.PlaybackRate = PlayRate = 1; updateSaveSettings(PlayRate.ToString()); }, x => true)); } }
        ICommand _GoSpeed3;       /**/public ICommand GoSpeed3 { get { return _GoSpeed3 ?? (_GoSpeed3 = new RelayCommand(x => { jump(); mp_Vm.PlaybackSession.PlaybackRate = PlayRate = 2; updateSaveSettings(PlayRate.ToString()); }, x => true)); } }
        ICommand _GoSpeed9;       /**/public ICommand GoSpeed9 { get { return _GoSpeed9 ?? (_GoSpeed9 = new RelayCommand(x => { jump(); mp_Vm.PlaybackSession.PlaybackRate = PlayRate = 8; updateSaveSettings(PlayRate.ToString()); }, x => true)); } }
        ICommand _GoFaster;       /**/public ICommand GoFaster { get { return _GoFaster ?? (_GoFaster = new RelayCommand(x => onGoFaster(), x => true)); } }
        ICommand _GoToPage;       /**/public ICommand GoToPage { get { return _GoToPage ?? (_GoToPage = new RelayCommand(x => onGoToPage(), x => true)); } }
        ICommand _OpenPick;       /**/public ICommand OpenPick { get { return _OpenPick ?? (_OpenPick = new RelayCommand(x => onOpenPick(), x => true)); } }
        ICommand _ClearLog;       /**/public ICommand ClearLog { get { return _ClearLog ?? (_ClearLog = new RelayCommand(x => TbInfo = "", x => true)); } }
        ICommand _SpeakMsg;       /**/public ICommand SpeakMsg { get { return _SpeakMsg ?? (_SpeakMsg = new RelayCommand(async x => await Speak(x as string), x => true)); } }
        ICommand _ResetPos;       /**/public ICommand ResetPos { get { return _ResetPos ?? (_ResetPos = new RelayCommand(x => SlctMru.PlayPosn = mp_Vm.PlaybackSession.Position, x => true)); } }
        ICommand _PlayPaus;       /**/public ICommand PlayPaus { get { return _PlayPaus ?? (_PlayPaus = new RelayCommand(x => { if (mp_Vm.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) mp_Vm.Pause(); else mp_Vm.Play(); }, x => true)); } }
        ICommand _DoSmth;         /**/public ICommand DoSmth { get { return _DoSmth ?? (_DoSmth = new RelayCommand(x => onDoSmth(), x => true)); } }
    }
}
//todo: chose the cleaner mode of item action menu implementation and finish all the desired commands.