using AsLink;
using DDJ.DB.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using MVVM.Common;
using System.Windows.Shell;
using DDJ.Main.Cmn;
using AAV.Sys.Ext;
using AAV.Sys.Helpers;

namespace DDJ.Main.ViewModels
{
    public partial class DdjViewModel : BindableBaseViewModel
  {
    const int cAtLeast10Min = 10 * 60;
    bool HasPlayedEnough
    {
      get
      {
        var minSecRequired = TimeSpan.FromSeconds(15);
        return //(DateTime. Now - _lastPlayStartAt) > minSecRequired &&
          _player.Position > minSecRequired ||
          (_player.NaturalDuration.HasTimeSpan && _player.NaturalDuration.TimeSpan < minSecRequired);
      }
    }

    public void Init_AutoStartPlayer(MediaElement wmp)
    {
      if (_player == null)
      {
        _player = wmp;
        _player.MediaEnded += player_MediaEnded;
        _player.MediaOpened += player_MediaOpened;
        _player.MediaFailed += player_MediaFailed;
        _player.LoadedBehavior =
        _player.UnloadedBehavior = MediaState.Manual;
        _player.Volume = DDJ.Main.Properties.Settings.Default.QuietPCsCsv.Contains(Environment.MachineName) ? .1 : 1;

        if (AutoStart == true)
          playAndStartCounters();
      }
    }
    bool isPlaying
    {
      get
      {
        if (_player == null) return false;

        var sw = Stopwatch.StartNew();
        var ms = getMediaState(_player);
        //Debug.WriteLine("{0} --> {1}", sw.Elapsed, ms);

        var prev = _player.Position;

        Thread.Sleep(1); // Debug.WriteLine("{0}-{1}={2} {3} {4}", _player.Position, prev, _player.Position - prev, _player.Position != prev, !(_player.Position.Equals(prev)));

        var rv = _player.Position != prev;
        MUProgressState = rv ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.Paused;
        return rv;//!(_player.Position.Equals(prev));				//return (_player.Position - prev).TotalMilliseconds > 0;
      }
    }
    async Task<bool> isPlayingAsync()
    {
      if (_player == null) return false;

      var prev = _player.Position;
      await Task.Delay(1);

      var rv = _player.Position != prev;
      MUProgressState = rv ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.Paused;
      return rv;//!(_player.Position.Equals(prev));				//return (_player.Position - prev).TotalMilliseconds > 0;
    }
    MediaState getMediaState(MediaElement player)
    {
      var helperObject = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(player);
      return (MediaState)helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(helperObject);
    }

    void player_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
    {
      _player.Position = TimeSpan.FromSeconds(IniPosn = VMPosn = CurMediaUnit.CurPositionSec);

      if (_player.NaturalDuration.HasTimeSpan &&
        CurMediaUnit.DurationSec != _player.NaturalDuration.TimeSpan.TotalSeconds)
        CurMediaUnit.DurationSec = _player.NaturalDuration.TimeSpan.TotalSeconds;

      _lastPlayStartAt = DateTime.Now;
    }
    async void player_MediaFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
    {
      var now = DateTime.Now;

      var exm = e.ErrorException.InnermostMessage();
      ExceptionMsg = $"MediaFailed: {exm}";

      CurMediaUnit.GenreID = 15; // exception: do not play next time ...until deliberately selected and fixed.
      if (CurMediaUnit.Notes == null || !CurMediaUnit.Notes.Contains(exm))
      {
        var len = (CurMediaUnit.Notes == null ? 0 : CurMediaUnit.Notes.Length);
        var errmsg = $" {now} {exm} ";
        if (errmsg.Length < (1024 - len)) CurMediaUnit.Notes += $" {now} {exm} ";
        else if (200 > (1024 - len)) CurMediaUnit.Notes += $" {now} {exm} ".Substring(0, 199);
      }

      if (!File.Exists(CurMediaUnit.PathFileExtOrg))
      {
        if (File.Exists(CurMediaUnit.PathFileExtOrg.ToLower().Replace("m:\\", "c:\\1\\m\\")))
        {
          _player.Source = new Uri(CurMediaUnit.PathFileExtOrg = CurMediaUnit.PathFileExtOrg.ToLower().Replace("m:\\", "c:\\1\\m\\"));
          saveAllToDb_Speak();
          ExceptionMsg = $"MediaFailed: SOLVED: renamed M:\\ to C:\\1\\M\\ {_player.Source.LocalPath} ";

          await Task.Delay(500);
          synth.Speak("Restarting...");
          playAndStartCounters();
          return;
        }
        ExceptionMsg = string.Format("MediaFailed: File does not exist; marking that in DB.  ");
        CurMediaUnit.DeletedAt = now;
        CurMediaUnit.Notes = FileSysProcessor.SAfeAddNote(CurMediaUnit, $" Found missing trying to play on {now.ToShortDateString()}.");
      }

      saveAllToDb_Speak();

      if (canMoveNext)
      {
        onMoveNext(null);
        playAndStartCounters();
      }
    }
    void player_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
    {
      _lastPlayStartAt = DateTime.MaxValue;

      logAuditionCurPosn();

      if (_isLooping)
      {
        _player.Position = TimeSpan.FromSeconds(0);
      }
      else
      {
        if (_askToDeleteAtEnd && (Environment.UserName.Contains("igid") || Environment.UserName.Contains("lex"))) doDeleteMU(_player);
        if (canMoveNext) onMoveNext(null);
      }

      if (canMoveNext)
        playAndStartCounters();
      else
        _player.Pause(); //2017-07: does not stop playing without this.
    }

    void logOldIfNewComing(MediaUnit old, MediaUnit neu)
    {
      if (old == null || neu == null || object.Equals(old, neu)) return;

      IniPosn = neu.CurPositionSec;

      logAuditionCurPosn();
    }

    DateTime _prevSaveTime = DateTime.MinValue;
    void safeAddAudition(int id)
    {
      var now = DateTime.Now;

      if ((now - _prevSaveTime).TotalSeconds < 10)
        return;

#if !DEBUG
			var partyMode = false;
#else
      var partyMode = true;
#endif
      Bpr.BeepOk();
      if (UserA) _db.MuAuditions.Add(new MuAudition { DoneAt = now, PartyMode = partyMode, MediaUnitID = id, DoneBy = "Alex" });
      if (UserM) _db.MuAuditions.Add(new MuAudition { DoneAt = now, PartyMode = partyMode, MediaUnitID = id, DoneBy = "Mei" });
      if (UserN) _db.MuAuditions.Add(new MuAudition { DoneAt = now, PartyMode = partyMode, MediaUnitID = id, DoneBy = "Nadine" });
      if (UserZ) _db.MuAuditions.Add(new MuAudition { DoneAt = now, PartyMode = partyMode, MediaUnitID = id, DoneBy = "Zoe" });
    }
    void doDeleteMU(MediaElement _player)
    {
      if (Debugger.IsAttached) Debugger.Break();
    }
    void playAndStartCounters()
    {
      if (_player == null) return;

      _player.Play();
      _lastPlayStartAt = DateTime.Now;
      //_timer.Start();
    }
    void pauseAndLog()
    {
      if (_player == null) return;

      _player.Pause();
      _lastPlayStartAt = DateTime.MaxValue;
      //_timer.Stop();
      logAuditionCurPosn();
    }
    void logAuditionCurPosn()
    {
      if (CurMediaUnit == null) return;

      if (CurMediaUnit.DurationSec > 0)
      {
        if ((CurMediaUnit.DurationSec < 600 && (CurMediaUnit.CurPositionSec / CurMediaUnit.DurationSec) > .5) ||  // if a muz piece - half counts as 1 audition
          (CurMediaUnit.DurationSec >= 600 && (CurMediaUnit.DurationSec - CurMediaUnit.CurPositionSec) < 60))     // if a long muz piece or podcast - last minute could be advertising.
        {
          safeAddAudition(CurMediaUnit.ID);
          CurMediaUnit.CurPositionSec = 0;
        }
      }

      if (CurMediaUnit.CurPositionSec < 10)
        VMPosn = CurMediaUnit.CurPositionSec = 0;

      Bpr.BeepOk();

      saveAllToDb_Speak();
    }
    void savePosIfLong()
    {
      if (CurMediaUnit.DurationSec > cAtLeast10Min && (CurMediaUnit.DurationSec - CurMediaUnit.CurPositionSec) < 60) // if a long multi-song unit and not very close to the end - save the position for the next start time
      {
        saveAllToDb_Speak();
      }
    }
    void saveAllToDb_Speak()
    {
      var rows = DbSaveMsgBox.TrySaveAsk(_db);
      Debug.WriteLine("{0} rows saved", rows);

      if (_AudioRprtg) synth.SpeakAsync($" {rows} rows saved. Position is {CurMediaUnit.CurPositionSec:N0} seconds.");
    }
  }
}
