using MVVM.Common;
using AsLink;
using DDJ.DB.Models;
using DDJ.Main.Cmn;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
//using AudioCompare;
using AAV.Sys.Helpers;
using AudioCompare;

namespace DDJ.Main.ViewModels
{
  public partial class DdjViewModel : BindableBaseViewModel
    {
        public ICommand VolumeTglCmd { get { return _VolumeTglCmd ?? (_VolumeTglCmd = new RelayCommand(x => onVolumeTgl(x), x => canVolumeTgl) { GestureKey = Key.Q, GestureModifier = ModifierKeys.Alt }); } }
        ICommand _VolumeTglCmd;
        public bool canVolumeTgl { get { return true; } }
        void onVolumeTgl(object x) { _player.Volume = _player.Volume == 1 ? .1 : _player.Volume == .1 ? .01 : 1; }

        public ICommand Delete0Cmd { get { return _Delete0Cmd ?? (_Delete0Cmd = new RelayCommand(x => onDelete0(x), x => canDelete0) { GestureKey = Key.Delete, GestureModifier = ModifierKeys.None }); } }
        ICommand _Delete0Cmd;
        public bool canDelete0 { get { return UserA; } }
        void onDelete0(object x)
        {
            var v = x; if (Debugger.IsAttached) Debugger.Break();
            Bpr.BeepOk();
            var now = DateTime.Now;
            CurMediaUnit.DeletedAt = now; //in case of exceptions at least the intent is recorded.
            var rows = DbSaveMsgBox.TrySaveAsk(_db);

            var ddir = @"C:\1\M.zDeleted";
            if (!Directory.Exists(ddir)) Directory.CreateDirectory(ddir);
            var file = (string.IsNullOrEmpty(CurMediaUnit.PathName) && !string.IsNullOrEmpty(CurMediaUnit.FileName)) ? Path.Combine(CurMediaUnit.PathName, CurMediaUnit.FileName) : CurMediaUnit.PathFileExtOrg;
            var dest = Path.Combine(ddir, Path.GetFileName(file));
            Task.Run(() => Task.Delay(5000)).ContinueWith(_ => File.Move(file, dest));

            if (canMoveNext) onMoveNext(x);
            else if (canMovePrev) onMovePrev(x);
        }

        ICommand _LoadListCmd; public ICommand LoadListCmd { get { return _LoadListCmd ?? (_LoadListCmd = new RelayCommand(async x => await onLoadList(x), x => true) { GestureKey = Key.L, GestureModifier = ModifierKeys.Control }); } }
        async Task<int> onLoadList(object ___notUsed___) { Bpr.BeepOk(); return await LoadList(); }

        public ICommand SeeDupesCmd { get { return _SeeDupesCmd ?? (_SeeDupesCmd = new RelayCommand(x => onSeeDupes(x), x => canSeeDupes) { GestureKey = Key.D, GestureModifier = ModifierKeys.Alt }); } }
        ICommand _SeeDupesCmd;
        public bool canSeeDupes { get { return true; } }
        void onSeeDupes(object ___notUsed___) { Bpr.BeepOk(); new AudioCompareMain(CurMediaUnit.FileName).Show(); }

        public ICommand TglPlayPausCCmd { get { return _TglPlayPausCCmd ?? (_TglPlayPausCCmd = new RelayCommand(x => onTglPlayPause(x), x => true) { GestureKey = Key.Pause, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglPlayPausCCmd;
        public ICommand TglPlayPausBCmd { get { return _TglPlayPausBCmd ?? (_TglPlayPausBCmd = new RelayCommand(x => onTglPlayPause(x), x => true) { GestureKey = Key.Play, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglPlayPausBCmd;
        public ICommand TglPlayPausACmd { get { return _TglPlayPausACmd ?? (_TglPlayPausACmd = new RelayCommand(x => onTglPlayPause(x), x => true) { GestureKey = Key.Space, GestureModifier = ModifierKeys.Control }); } }
        ICommand _TglPlayPausACmd;
        public ICommand TglPlayPauseCmd { get { return _TglPlayPauseCmd ?? (_TglPlayPauseCmd = new RelayCommand(x => onTglPlayPause(x), x => true) { GestureKey = Key.Space, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglPlayPauseCmd;
        void onTglPlayPause(object ___notUsed___)
        {
            CurMedUnInfo = "░░░░░░░░░░░░░░░";
            Bpr.BeepOk();
            if (isPlaying == true)
                pauseAndLog();
            else
                playAndStartCounters();
        }

        public ICommand MoveNextCmd { get { return _MoveNextCmd ?? (_MoveNextCmd = new RelayCommand(x => onMoveNext(x), x => canMoveNext) { GestureKey = Key.Down, GestureModifier = ModifierKeys.None }); } }
        ICommand _MoveNextCmd;
        public bool canMoveNext { get { return LastPiece == false && PlayList.IndexOf(CurMediaUnit) < PlayList.Count() - 1; } }
        void onMoveNext(object x)
        {
            Bpr.BeepOk(); savePosIfLong();
            if (LastPiece == true)
                pauseAndLog();
            else if (PlayList.IndexOf(CurMediaUnit) < PlayList.Count() - 1)
                CurMediaUnit = PlayList[PlayList.IndexOf(CurMediaUnit) + 1];
            else
                pauseAndLog();
        }
        public ICommand MovePrevCmd { get { return _MovePrevCmd ?? (_MovePrevCmd = new RelayCommand(x => onMovePrev(x), x => canMovePrev) { GestureKey = Key.Up, GestureModifier = ModifierKeys.None }); } }
        ICommand _MovePrevCmd;
        public bool canMovePrev { get { return PlayList.IndexOf(CurMediaUnit) > 0; } }
        void onMovePrev(object x)
        {
            Bpr.BeepOk(); savePosIfLong();
            if (canMovePrev) CurMediaUnit = PlayList[PlayList.IndexOf(CurMediaUnit) - 1];
        }

        public ICommand JumpNextCmd { get { return _JumpNextCmd ?? (_JumpNextCmd = new RelayCommand(x => onJumpNext(x), x => canJumpNext) { GestureKey = Key.Down, GestureModifier = ModifierKeys.Shift }); } }
        ICommand _JumpNextCmd;
        public bool canJumpNext { get { return true; } }
        void onJumpNext(object x) { Bpr.BeepOk(); var v = x; if (Debugger.IsAttached) Debugger.Break(); }

        public ICommand JumpPrevCmd { get { return _JumpPrevCmd ?? (_JumpPrevCmd = new RelayCommand(x => onJumpPrev(x), x => canJumpPrev) { GestureKey = Key.Up, GestureModifier = ModifierKeys.Shift }); } }
        ICommand _JumpPrevCmd;
        DateTime _lastPlayStartAt = DateTime.MaxValue;
        public bool canJumpPrev { get { return true; } }
        void onJumpPrev(object x) { Bpr.BeepOk(); var v = x; if (Debugger.IsAttached) Debugger.Break(); }

        ICommand _FsToDbCmd; public ICommand FsToDbCmd { get { return _FsToDbCmd ?? (_FsToDbCmd = new RelayCommand(x => onFsToDb(x), x => true) { GestureKey = Key.S, GestureModifier = ModifierKeys.Alt | ModifierKeys.Control }); } }
        async void onFsToDb(object x)
        {
            ExceptionMsg = $"Checking  \"{TrgFolder}\"  for new & missing files ...  ";
            Bpr.BeepOk();

            var progressHandler = new Progress<string>(value => { ExceptionMsg = value; });
            var progress = progressHandler as IProgress<string>;

            var sw = Stopwatch.StartNew();
            var task = new FileSysProcessor().FsToDb(_db, TrgFolder, progress);
            await task;
            ExceptionMsg = $"{task.Result} new rows added in {sw.Elapsed:m\\:ss}, final status: {task.Status}.   ";
        }

        public ICommand MoveToStartCmd { get { return _MoveToStartCmd ?? (_MoveToStartCmd = new RelayCommand(x => onMoveToStart(x), x => _player.NaturalDuration.HasTimeSpan) { GestureKey = Key.Home, GestureModifier = ModifierKeys.None }); } }
        ICommand _MoveToStartCmd;
        void onMoveToStart(object x) { Bpr.BeepOk(); _player.Position = TimeSpan.FromSeconds(0); }

        public ICommand MoveToEndCmd { get { return _MoveToEndCmd ?? (_MoveToEndCmd = new RelayCommand(x => onMoveToEnd(x), x => _player.NaturalDuration.HasTimeSpan) { GestureKey = Key.End, GestureModifier = ModifierKeys.None }); } }
        ICommand _MoveToEndCmd;
        void onMoveToEnd(object x) { Bpr.BeepOk(); _player.Position = _player.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromSeconds(1.5)); }

        public ICommand BackToStartPosnCmd { get { return _BackToStartPosn ?? (_BackToStartPosn = new RelayCommand(x => { _player.Position = TimeSpan.FromSeconds(IniPosn); }, x => true) { GestureKey = Key.A, GestureModifier = ModifierKeys.Control }); } }
        ICommand _BackToStartPosn;

        public ICommand MoveLefNCommand { get { return moveLefNCommand ?? (moveLefNCommand = new RelayCommand(x => { _player.Position -= TimeSpan.FromSeconds(5); }, x => canMoveLeftN) { GestureKey = Key.Left }); } }
        ICommand moveLefNCommand;
        public ICommand MoveRghNCommand { get { return moveRghNCommand ?? (moveRghNCommand = new RelayCommand(x => { _player.Position += TimeSpan.FromSeconds(5); }, x => canMoveRghtN) { GestureKey = Key.Right }); } }
        ICommand moveRghNCommand;
        public ICommand MoveLefCCommand { get { return moveLefCCommand ?? (moveLefCCommand = new RelayCommand(x => { _player.Position -= TimeSpan.FromMinutes(1); }, x => canMoveLeftC) { GestureKey = Key.Left, GestureModifier = ModifierKeys.Control }); } }
        ICommand moveLefCCommand;
        public ICommand MoveRghCCommand { get { return moveRghCCommand ?? (moveRghCCommand = new RelayCommand(x => { _player.Position += TimeSpan.FromMinutes(1); }, x => canMoveRghtC) { GestureKey = Key.Right, GestureModifier = ModifierKeys.Control }); } }
        ICommand moveRghCCommand;
        public ICommand MoveLefACommand { get { return moveLefACommand ?? (moveLefACommand = new RelayCommand(x => { _player.Position -= TimeSpan.FromMinutes(5); }, x => canMoveLeftA) { GestureKey = Key.Left, GestureModifier = ModifierKeys.Alt }); } }
        ICommand moveLefACommand;
        public ICommand MoveRghACommand { get { return moveRghACommand ?? (moveRghACommand = new RelayCommand(x => { _player.Position += TimeSpan.FromMinutes(5); }, x => canMoveRghtA) { GestureKey = Key.Right, GestureModifier = ModifierKeys.Alt }); } }
        ICommand moveRghACommand;
        public ICommand MoveLefSCommand { get { return moveLefSCommand ?? (moveLefSCommand = new RelayCommand(x => { _player.Position -= TimeSpan.FromSeconds(.5); }, x => canMoveLeftS) { GestureKey = Key.Left, GestureModifier = ModifierKeys.Shift }); } }
        ICommand moveLefSCommand;
        public ICommand MoveRghSCommand { get { return moveRghSCommand ?? (moveRghSCommand = new RelayCommand(x => { _player.Position += TimeSpan.FromSeconds(.5); }, x => canMoveRghtS) { GestureKey = Key.Right, GestureModifier = ModifierKeys.Shift }); } }
        ICommand moveRghSCommand;

        public bool canMoveLeftN { get { return _player.Position > TimeSpan.FromSeconds(5); } }
        public bool canMoveRghtN { get { return _player.NaturalDuration - _player.Position > TimeSpan.FromSeconds(5); } }
        public bool canMoveLeftC { get { return _player.Position > TimeSpan.FromMinutes(1); } }
        public bool canMoveRghtC { get { return _player.NaturalDuration - _player.Position > TimeSpan.FromMinutes(1); } }
        public bool canMoveLeftA { get { return _player.Position > TimeSpan.FromMinutes(5); } }
        public bool canMoveRghtA { get { return _player.NaturalDuration - _player.Position > TimeSpan.FromMinutes(5); } }
        public bool canMoveLeftS { get { return _player.Position > TimeSpan.FromSeconds(.5); } }
        public bool canMoveRghtS { get { return _player.NaturalDuration - _player.Position > TimeSpan.FromSeconds(.5); } }


        ICommand _AddLikeCmd; public ICommand AddLikeCmd { get { return _AddLikeCmd ?? (_AddLikeCmd = new RelayCommand(x => onAddLike(x), x => isCurNotNull) { GestureKey = Key.OemPlus, GestureModifier = ModifierKeys.None }); } }
        ICommand _AddHateCmd; public ICommand AddHateCmd { get { return _AddHateCmd ?? (_AddHateCmd = new RelayCommand(x => onAddHate(x), x => isCurNotNull) { GestureKey = Key.OemMinus, GestureModifier = ModifierKeys.None }); } }
        void onAddLike(object x) { addLike(1); }
        void onAddHate(object x) { addLike(-1); }
        void addLike(int like1)
        {
            Bpr.BeepOk();
            var now = DateTime.Now;
            if (UserA) _db.MuRateHists.Add(new MuRateHist { DoneAt = now, MediaUnitID = CurMediaUnit.ID, LikeUnit = like1, CurPositionSec = _player.Position.TotalSeconds, DoneBy = "Alex" });
            if (UserM) _db.MuRateHists.Add(new MuRateHist { DoneAt = now, MediaUnitID = CurMediaUnit.ID, LikeUnit = like1, CurPositionSec = _player.Position.TotalSeconds, DoneBy = "Mei" });
            if (UserN) _db.MuRateHists.Add(new MuRateHist { DoneAt = now, MediaUnitID = CurMediaUnit.ID, LikeUnit = like1, CurPositionSec = _player.Position.TotalSeconds, DoneBy = "Nadine" });
            if (UserZ) _db.MuRateHists.Add(new MuRateHist { DoneAt = now, MediaUnitID = CurMediaUnit.ID, LikeUnit = like1, CurPositionSec = _player.Position.TotalSeconds, DoneBy = "Zoe" });

            var rows = DbSaveMsgBox.TrySaveAsk(_db);
        }


        public bool isCurNotNull { get { return CurMediaUnit != null; } }
        public bool alwaysCan { get { return true; } }

        void onTglGenre(object gi)
        {
            Bpr.BeepOk();
            var intGnr = Convert.ToInt16(gi);
            var genre = GenreFilter.FirstOrDefault(r => r.ID == intGnr);
            if (genre != null) GenreFilter.Remove(genre);
            else GenreFilter.Add(Genres.FirstOrDefault(r => r.ID == intGnr));
        }
        void onSetGenre(object gi)
        {
            Bpr.BeepOk();
            var intGnr = Convert.ToInt16(gi);
            CurMediaUnit.GenreID = intGnr;
        }
        public ICommand TglGenre1Cmd { get { return _TglGenre1Cmd ?? (_TglGenre1Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F1, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre1Cmd;
        public ICommand TglGenre2Cmd { get { return _TglGenre2Cmd ?? (_TglGenre2Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F2, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre2Cmd;
        public ICommand TglGenre3Cmd { get { return _TglGenre3Cmd ?? (_TglGenre3Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F3, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre3Cmd;
        public ICommand TglGenre4Cmd { get { return _TglGenre4Cmd ?? (_TglGenre4Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F4, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre4Cmd;
        public ICommand TglGenre5Cmd { get { return _TglGenre5Cmd ?? (_TglGenre5Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F5, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre5Cmd;
        public ICommand TglGenre6Cmd { get { return _TglGenre6Cmd ?? (_TglGenre6Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F6, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre6Cmd;
        public ICommand TglGenre7Cmd { get { return _TglGenre7Cmd ?? (_TglGenre7Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F7, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre7Cmd;
        public ICommand TglGenre8Cmd { get { return _TglGenre8Cmd ?? (_TglGenre8Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F8, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre8Cmd;
        public ICommand TglGenre9Cmd { get { return _TglGenre9Cmd ?? (_TglGenre9Cmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F9, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenre9Cmd;
        public ICommand TglGenreACmd { get { return _TglGenreACmd ?? (_TglGenreACmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F10, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenreACmd;
        public ICommand TglGenreBCmd { get { return _TglGenreBCmd ?? (_TglGenreBCmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F11, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenreBCmd;
        public ICommand TglGenreCCmd { get { return _TglGenreCCmd ?? (_TglGenreCCmd = new RelayCommand(x => onTglGenre(x), x => alwaysCan) { GestureKey = Key.F12, GestureModifier = ModifierKeys.None }); } }
        ICommand _TglGenreCCmd;

        public ICommand SetGenre1Cmd { get { return _SetGenre1Cmd ?? (_SetGenre1Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F1, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre1Cmd;
        public ICommand SetGenre2Cmd { get { return _SetGenre2Cmd ?? (_SetGenre2Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F2, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre2Cmd;
        public ICommand SetGenre3Cmd { get { return _SetGenre3Cmd ?? (_SetGenre3Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F3, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre3Cmd;
        public ICommand SetGenre4Cmd { get { return _SetGenre4Cmd ?? (_SetGenre4Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F4, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre4Cmd;
        public ICommand SetGenre5Cmd { get { return _SetGenre5Cmd ?? (_SetGenre5Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F5, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre5Cmd;
        public ICommand SetGenre6Cmd { get { return _SetGenre6Cmd ?? (_SetGenre6Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F6, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre6Cmd;
        public ICommand SetGenre7Cmd { get { return _SetGenre7Cmd ?? (_SetGenre7Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F7, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre7Cmd;
        public ICommand SetGenre8Cmd { get { return _SetGenre8Cmd ?? (_SetGenre8Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F8, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre8Cmd;
        public ICommand SetGenre9Cmd { get { return _SetGenre9Cmd ?? (_SetGenre9Cmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F9, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenre9Cmd;
        public ICommand SetGenreACmd { get { return _SetGenreACmd ?? (_SetGenreACmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F10, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenreACmd;
        public ICommand SetGenreBCmd { get { return _SetGenreBCmd ?? (_SetGenreBCmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F11, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenreBCmd;
        public ICommand SetGenreCCmd { get { return _SetGenreCCmd ?? (_SetGenreCCmd = new RelayCommand(x => onSetGenre(x), x => isCurNotNull) { GestureKey = Key.F12, GestureModifier = ModifierKeys.Control }); } }
        ICommand _SetGenreCCmd;

    }
}
