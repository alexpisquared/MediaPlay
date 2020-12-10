using AAV.Sys.Ext;
using AAV.Sys.Helpers;
using AsLink;
using DDJ.DB.Models;
using DDJ.Main.Cmn;
using MVVM.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DDJ.Main.ViewModels
{
  public partial class DdjViewModel : BindableBaseViewModel
  {
    public async Task<int> LoadLkus()
    {
      Bpr.Beep1of2();
      var sw = Stopwatch.StartNew();
      int genresCount = await Task.Run(() => { _db.LkuGenres.Load(); return _db.LkuGenres.Local.Count(); });

      Genres.ClearAddRange(_db.LkuGenres.Local);
      if (Genres.Count() > 0)
        CurGenre = _db.LkuGenres.First(r => r.Name == "All");

      TopLefttInfo = $"{Genres.Count()} genres loaded in {.001 * sw.ElapsedMilliseconds:N1} sec. ";

      var ttlMU = await LoadList();

      Bpr.Beep2of2();
      return genresCount;
    }
    public async Task<int> LoadList()
    {
      //var genres = CurGenre.ID == 2 ? new int[] { } : new int[] { CurGenre.ID };
      var gfa = new int[GenreFilter.Count()];
      int i = 0; foreach (var g in GenreFilter) { gfa[i++] = g.ID; }

      var users0 = new List<string>();
      if (UserA) users0.Add("Alex");
      if (UserM) users0.Add("Mei");
      if (UserN) users0.Add("Nadine");
      if (UserZ) users0.Add("Zoe");

      int tr = await onLoadList(gfa, users0.ToArray(), StringFilter, AddRandomDay);
      return tr;
    }

    async Task<int> onLoadList(int[] genres, string[] users, string filter, int rndDay)
    {
      try
      {
        var sw = Stopwatch.StartNew();
        int ttlRows = await Task.Run(() => { _db.MediaUnits.Where(r => !r.DeletedAt.HasValue).Load(); return _db.MediaUnits.Local.Count(); });

        var sql = $@"
SELECT TOP (@take) MediaUnit.ID, MediaUnit.GenreID, MediaUnit.PathFileExtOrg, MediaUnit.PathName, MediaUnit.FileName, MediaUnit.FileHashMD5, MediaUnit.FileHashQck, MediaUnit.FileLength, MediaUnit.DurationSec, MediaUnit.CurPositionSec, MediaUnit.Notes, MediaUnit.AddedAt, MediaUnit.DeletedAt
FROM   MuAudition INNER JOIN MediaUnit ON MuAudition.MediaUnitID = MediaUnit.ID
GROUP BY MuAudition.MediaUnitID, MediaUnit.ID, MediaUnit.GenreID, MediaUnit.PathFileExtOrg, MediaUnit.PathName, MediaUnit.FileName, MediaUnit.FileHashMD5, MediaUnit.FileHashQck, MediaUnit.FileLength, MediaUnit.DurationSec, MediaUnit.CurPositionSec, MediaUnit.Notes, MediaUnit.AddedAt, MediaUnit.DeletedAt
HAVING MediaUnit.DeletedAt IS NULL AND MediaUnit.PathFileExtOrg LIKE '%'+@filter+'%' {(genres.Length > 0 ? " AND (MediaUnit.GenreID IN (" + string.Join(",", genres) + ")) " : " ")}
ORDER BY DATEDIFF(day, MAX(MuAudition.DoneAt), GETDATE()) / (1+COUNT(*)) + ABS(CHECKSUM(NEWID())) % @rndDay DESC";

        //var oldNOtUsed = await _db.Database.SqlQuery<MediaUnit>(sql, new SqlParameter("take", PlaylilstLen), new SqlParameter("rndDay", rndDay), new SqlParameter("filter", filter)).ToListAsync();

        var take = PlaylilstLen;
        var qa = $@"
SELECT        TOP ({take}) MuAudition.MediaUnitID
FROM            MuAudition INNER JOIN                          MediaUnit ON MuAudition.MediaUnitID = MediaUnit.ID 
GROUP BY MuAudition.MediaUnitID, MediaUnit.GenreID, MediaUnit.PathFileExtOrg, MediaUnit.DeletedAt
HAVING MediaUnit.DeletedAt IS NULL AND MediaUnit.PathFileExtOrg LIKE '%{filter}%' {(genres.Length > 0 ? " AND (MediaUnit.GenreID IN (" + string.Join(",", genres) + ")) " : " ")}
ORDER BY DATEDIFF(day, MAX(MuAudition.DoneAt), GETDATE()) / (1+COUNT(*)) + ABS(CHECKSUM(NEWID())) % {rndDay} DESC";

        var auds = await _db.Database.SqlQuery<int>(qa, new SqlParameter("take", PlaylilstLen), new SqlParameter("rndDay", rndDay)).ToListAsync();

        var pl1 = _db.MediaUnits.Where(r => auds.Contains(r.ID)).OrderBy(r => r.MuAuditions.Where(a => a.MediaUnitID == r.ID).Max(a => a.DoneAt)); //Jun2017
        var pl2 = _db.MediaUnits.Where(r => r.DeletedAt == null && auds.Contains(r.ID)).OrderBy(r => r.MuAuditions.Where(a => a.MediaUnitID == r.ID).Max(a => a.DoneAt)); //Jun2017

        var aud = _db.MuAuditions.Select(r => r.MediaUnitID);
        PlayList.Clear();
        _db.MediaUnits.Where(r => r.DeletedAt == null && !aud.Contains(r.ID)).ToList().ForEach(PlayList.Add); // 2017-07: start from never listened to.
        _db.MediaUnits.Where(r => r.DeletedAt == null && auds.Contains(r.ID)).OrderBy(r => r.MuAuditions.Where(a => a.MediaUnitID == r.ID).Max(a => a.DoneAt)).ToList().ForEach(PlayList.Add); // 2017-06
        if (PlayList.Count() > 0)
          CurMediaUnit = PlayList[0];

        TopLefttInfo = $"Top {PlayList.Count()} songs of {"oldNOtUsed.Count()"} matches of {_db.MediaUnits.Local.Count()} total loaded in {.001 * sw.ElapsedMilliseconds:N1}s. Last pos: {TimeSpan.FromSeconds(CurMediaUnit.CurPositionSec):m\\:ss}";

        if (_AudioRprtg) synth.SpeakAsync($"Loaded in {sw.Elapsed.TotalSeconds:N0} seconds.");

        ExceptionMsg = "";

        return ttlRows;
      }
      catch (Exception ex) { ex.Log(); }
      return -1;
    }
    async Task<int> onLoadList__(object ___notUsed___)
    {
      Bpr.Beep1of2();
      var sw = Stopwatch.StartNew();
      int ttlRows = await Task.Run(() => { _db.MediaUnits.Take(7).Load(); return _db.MediaUnits.Local.Count(); });        //{ Task.Delay(3333); return DateTime .Now.Millisecond; });

      PlayList.ClearAddRange(_db.MediaUnits.Local);
      if (PlayList.Count() > 0)
        CurMediaUnit = PlayList[0];

      TopLefttInfo = $"{PlayList.Count()} songs loaded in {.001 * sw.ElapsedMilliseconds:N1} sec. ";
      Bpr.Beep2of2();

      return ttlRows;
    }
    async Task<int> onLoadList_(object ___notUsed___)
    {
      Bpr.Beep1of2();

      var sw = Stopwatch.StartNew();

      Task<int> task = Task.Run(() => { _db.MediaUnits.Take(7).Load(); return _db.MediaUnits.Local.Count(); });       //{ Task.Delay(3333); return DateTime .Now.Millisecond; });

      await task.ContinueWith(_ =>
      {
        PlayList.ClearAddRange(_db.MediaUnits.Local);
        if (PlayList.Count() > 0)
        {
          CurMediaUnit = PlayList[0];
          onTglPlayPause(___notUsed___);
        }

        TopLefttInfo = $"{PlayList.Count()} songs loaded in {.001 * sw.ElapsedMilliseconds:N1} sec. ";
        Bpr.Beep2of2();
      }, TaskScheduler.FromCurrentSynchronizationContext());

      int ttlRows = await task;

      TopLefttInfo = $"{PlayList.Count()} songs loaded in {.001 * sw.ElapsedMilliseconds:N1} sec. ";

      //await _db.MediaUnits.Take(7).LoadAsync();


      return ttlRows;
    }
  }
}
