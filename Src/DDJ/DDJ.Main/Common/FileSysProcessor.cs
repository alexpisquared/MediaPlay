using AsLink;
using DDJ.DB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DDJ.Main.Cmn
{
    internal class FileSysProcessor
  {
    DateTime _now = DateTime.Now;
    MD5 _md5 = MD5.Create();

    public async Task<int> FsToDb(DdjEf4DBContext db, string dir0, IProgress<string> progress)
    {
      _now = DateTime.Now;

      await Task.Delay(500);

      Task<int> tsk = Task.Run(async () =>
      {
        try
        {
          var dir = Directory.Exists(dir0) ? dir0 : Directory.Exists(@"M:\") ? @"M:\" : Directory.Exists(@"C:\1\M\") ? @"C:\1\M\" : Directory.Exists(@"D:\1\M\") ? @"D:\1\M\" : "";

          progress.Report("Loading files from FS"); await Task.Delay(500);

          var fis = new List<FileInfo>();
          foreach (var ext in new string[] { "*.ape", "*.flac", "*.m4a", "*.mp3", "*.mp4", "*.ogg", "*.wav", "*.wma", "*.wmv" })
            foreach (var fi in new DirectoryInfo(dir).GetFiles(ext, SearchOption.AllDirectories).OrderBy(r => r.Length)) // Directory.GetFiles(dir, ext, SearchOption.AllDirectories))
              fis.Add(fi);

          progress.Report("For each found in FS update DB"); await Task.Delay(500);

          fis.ForEach(fi => addIfNewOrUpdate(db, fi));

          progress.Report("For each in DB check exisance in FS"); await Task.Delay(500);

          db.MediaUnits.Where(r => r.PathName.StartsWith(dir0) && r.DeletedAt == null).ToList().ForEach(mu => markDeletedIf(fis, mu));

          return (await db.TrySaveReportAsync()).rowsSavedCnt;
        }
        catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); }
        return -1765;
      });

      return await tsk;
    }

    void markDeletedIf(List<FileInfo> fis, MediaUnit mu)
    {
      if (fis.Any(r => string.Compare(r.FullName, mu.PathFileExtOrg, true) == 0)) return;
      if (fis.Any(r => string.Compare(r.FullName, Path.Combine(mu.PathName, mu.FileName), true) == 0)) return;
      mu.DeletedAt = _now;
      mu.Notes = SAfeAddNote(mu, $"Missing on {_now.ToShortDateString()}. ");
    }

    public static string SAfeAddNote(MediaUnit mu, string note)
    {
      var extra = mu.Notes.Length + note.Length - 1024;
      var otes = note + (extra > 0 ? mu.Notes.Substring(0, extra) : mu.Notes);
      return otes;
    }

    void addIfNewOrUpdate(DdjEf4DBContext db, FileInfo fi)
    {
      var dbf = db.MediaUnits.FirstOrDefault(r => string.Compare(r.PathFileExtOrg, fi.FullName, true) == 0);

      //var sw = Stopwatch.StartNew();
      //var hs = BitConverter.ToInt64(_md5.ComputeHash(fi.OpenRead()), 0);
      //Debug.WriteLine("{0,11:N0} / {1,6:N1}  = {2,9:N0}  ==> {3} {4} {5}", fi.Length, sw.Elapsed.TotalMilliseconds, fi.Length / sw.Elapsed.TotalMilliseconds, hs, dbf.FileHashQck, fi);

      if (dbf != null) // if in DB by filename:
      {
        if (dbf.DeletedAt != null)
          dbf.DeletedAt = null;

        if (dbf.FileHashMD5 == 0)
          dbf.FileHashMD5 = BitConverter.ToInt64(_md5.ComputeHash(fi.OpenRead()), 0);

        return;
      }


      var fileHashMD5 = BitConverter.ToInt64(_md5.ComputeHash(fi.OpenRead()), 0);

      dbf = db.MediaUnits.FirstOrDefault(r => r.FileHashMD5 == fileHashMD5);

      if (dbf != null) // if in DB by hash:
      {
        if (dbf.DeletedAt != null)
          dbf.DeletedAt = null;

        dbf.FileName = fi.Name;
        dbf.PathName = fi.DirectoryName;
        dbf.FileLength = fi.Length;
        dbf.PathFileExtOrg = fi.FullName;

        return;
      }

      //else // not in DB:

      db.MediaUnits.Add(new MediaUnit
      {
        AddedAt = _now,
        FileName = fi.Name,
        PathName = fi.DirectoryName,
        FileLength = fi.Length,
        PathFileExtOrg = fi.FullName,
        CurPositionSec = 0,
        DurationSec = 0,
        FileHashMD5 = BitConverter.ToInt64(_md5.ComputeHash(fi.OpenRead()), 0),
        FileHashQck = 0,
        GenreID = 1,
        Notes = ""
      });
    }
  }
}
