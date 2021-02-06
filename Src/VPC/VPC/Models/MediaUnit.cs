using AsLink;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Win32;
using MVVM.Common;
using Common.UI.Lib.Model;
using AAV.Sys.Helpers;

namespace VPC.Models
{
  //[PropertyChanged.AddINotifyPropertyChangedInterfaceAttribute] // [PropertyChanged.ImplementPropertyChanged] //fody  removed on 2021-02-06
  public class MediaUnit : BindableBase
  {
    const string _vpc = ".vpC";

    public static MediaUnit LoadMetaData(string mediaOrMetadataFile, TimeSpan? duration = null, int? NaturalVideoHeight = null, int? NaturalVideoWidth = null)
    {
      var o = isMetadata(mediaOrMetadataFile) ?
        loadMetaDataFromMetadata(mediaOrMetadataFile, duration, NaturalVideoHeight, NaturalVideoWidth) :
        loadMetaDataFromMediaFile(mediaOrMetadataFile, duration, NaturalVideoHeight, NaturalVideoWidth);
      o.OrderByPos();
      return o;
    }
    static MediaUnit loadMetaDataFromMediaFile(string media, TimeSpan? duration = null, int? NaturalVideoHeight = null, int? NaturalVideoWidth = null)
    {
      var mdfn = metadataFilename(media);

      MediaUnit mu;
      if (File.Exists(mdfn))
      {
        mu = Serializer.LoadFromFile<MediaUnit>(mdfn) as MediaUnit;
        mu.Position = TimeSpan.FromSeconds(mu.PositionSec);
        mu.Duration = TimeSpan.FromSeconds(mu.DurationSec);
      }
      else
      {
        mu = new MediaUnit { PathFileCur = media };
      }

      checkMpcViewTime(media, mu);

      if (duration != null && duration.HasValue) mu.Duration = duration.Value;
      if (NaturalVideoHeight != null) mu.VideoHeight = NaturalVideoHeight.Value;
      if (NaturalVideoWidth != null) mu.VideoWidth = NaturalVideoWidth.Value;

      if (mu.AddedAt == DateTime.MinValue) mu.AddedAt = new FileInfo(media).LastWriteTime;
      if (mu.FileLength == 0) mu.FileLength = new FileInfo(media).Length;

      if (string.IsNullOrEmpty(mu.PathFileCur) || mu.PathFileCur != media) mu.PathFileCur = media;
      if (string.IsNullOrEmpty(mu.PathFileOrg)) mu.PathFileOrg = media;

      return mu;
    }
    static MediaUnit loadMetaDataFromMetadata(string mdfn, TimeSpan? duration = null, int? NaturalVideoHeight = null, int? NaturalVideoWidth = null)
    {
      var media = mdfn.Replace(_vpc, "");

      MediaUnit mu;

      if (File.Exists(mdfn))
      {
        mu = Serializer.LoadFromFile<MediaUnit>(mdfn) as MediaUnit;
        mu.Position = TimeSpan.FromSeconds(mu.PositionSec);
        mu.Duration = TimeSpan.FromSeconds(mu.DurationSec);
      }
      else
      {
        mu = new MediaUnit { PathFileCur = mdfn };
      }

      checkMpcViewTime(media, mu);

      if (duration != null && duration.HasValue) mu.Duration = duration.Value;
      if (NaturalVideoHeight != null) mu.VideoHeight = NaturalVideoHeight.Value;
      if (NaturalVideoWidth != null) mu.VideoWidth = NaturalVideoWidth.Value;

      if (mu.AddedAt == DateTime.MinValue) mu.AddedAt = new FileInfo(mdfn).LastWriteTime;
      if (mu.FileLength == 0) mu.FileLength = new FileInfo(mdfn).Length;

      return mu;
    }
    static void checkMpcViewTime(string media, MediaUnit mu)
    {
      var mpc = getMediaPlayerCalssicPositionInSec(media);
      if (mpc > mu.PositionSec)
      {
        if (mu.PositionSec == 0)
        {
          ViewTimeLog.Log(TimeSpan.FromSeconds(mpc));
        }
        mu.Position = TimeSpan.FromSeconds(mu.PositionSec = mpc);
      }
    }

    public static void SaveMetaData(MediaUnit mu)
    {
      const int minLengthWorthSaving = 600;
      if (IsAudio(mu.FileName))
      {
        if (mu.DurationSec < minLengthWorthSaving && (mu.Duration.Value.TotalSeconds < minLengthWorthSaving))
          return;
      }

      SaveMetaData_ALWAYS(mu);
    }
    public static bool IsAudio(string filename) => (".m4a.mp3.wma.wav".Contains(Path.GetExtension(filename).ToLower()));

    public static void SaveMetaData_ALWAYS(MediaUnit mu)
    {
      var mdfn = metadataFilename(mu.PathFileCur);
      Serializer.SaveToFile(mu, mdfn);

      setMediaPlayerCalssicPositionInSec(mu.PathFileCur, mu.PositionSec);

      ///todo: in order to get proper start time implement
      ///set-version of getMediaPlayerCalssicPositionInSec(media); ...
      ///... or maybe remove completely from MPC 
    }

    static string metadataFilename(string mediaOrMetadataFile, string subfolder = "")
    {
      var vpDbFolder = OneDrive.VpdbFolder + subfolder;

      var mdfn = Path.Combine(vpDbFolder, (isMetadata(mediaOrMetadataFile) ? mediaOrMetadataFile : Path.GetFileName(mediaOrMetadataFile) + _vpc));

      return mdfn;
    }
    static string metadataFilenameDeleted(string file)
    {
      return metadataFilename(file, @"Deleted\");
    }


    //ObservableCollection<TimeSpan> _TSBookmarks = new ObservableCollection<TimeSpan>(); public ObservableCollection<TimeSpan> TSBookmarks { get { return _TSBookmarks; } set { Set(ref this._TSBookmarks, value); } }
    ObservableCollection<MuBookmark> _Bookmarks = new ObservableCollection<MuBookmark>(); public ObservableCollection<MuBookmark> Bookmarks { get { return _Bookmarks; } set { Set(ref this._Bookmarks, value); } }
    ObservableCollection<MuAudition> _Auditions = new ObservableCollection<MuAudition>(); public ObservableCollection<MuAudition> Auditions { get { return _Auditions; } set { Set(ref this._Auditions, value); } }

    //[Key]
    int _ID; public int ID { get { return _ID; } set { Set(ref this._ID, value); } }
    string _PathFileCur; public string PathFileCur { get { return _PathFileCur; } set { Set(ref this._PathFileCur, value); FileName = Path.GetFileName(value); PathName = Path.GetDirectoryName(value); } }
    string _OrgHttpLink; public string OrgHttpLink { get { return _OrgHttpLink; } set { Set(ref this._OrgHttpLink, value); } }
    string _PathFileOrg; public string PathFileOrg { get { return _PathFileOrg; } set { Set(ref this._PathFileOrg, value); } }
    string _PathName; public string PathName { get { return _PathName; } set { Set(ref this._PathName, value); } }
    string _FileName; public string FileName { get { return _FileName; } set { Set(ref this._FileName, value); } }
    long _FileHashMD5; public long FileHashMD5 { get { return _FileHashMD5; } set { Set(ref this._FileHashMD5, value); } }
    long _FileHashQck; public long FileHashQck { get { return _FileHashQck; } set { Set(ref this._FileHashQck, value); } }
    long _FileLength; public long FileLength { get { return _FileLength; } set { Set(ref this._FileLength, value); } }
    int _VideoHeight; public int VideoHeight { get { return _VideoHeight; } set { Set(ref this._VideoHeight, value); } }
    int _VideoWidth; public int VideoWidth { get { return _VideoWidth; } set { Set(ref this._VideoWidth, value); } }
    int _SpeedIdx = 10; public int SpeedIdx { get { return _SpeedIdx; } set { Set(ref this._SpeedIdx, value); } }

    [XmlIgnore]
    public TimeSpan? Duration { get { return _Duration; } set { if (Set(ref this._Duration, value)) DurationSec = value.Value.TotalSeconds; } }
    TimeSpan? _Duration;
    [XmlIgnore]
    public TimeSpan? Position { get { return _Position; } set { if (Set(ref this._Position, value)) PositionSec = value.Value.TotalSeconds; } }
    TimeSpan? _Position;
    [XmlIgnore]
    public TimeSpan? EventTime { get { return _EventTime; } set { Set(ref this._EventTime, value); } }
    TimeSpan? _EventTime;

    public double DurationSec { get; set; } // { return _DurationSec; } set { Set(ref this._DurationSec, value); } }
    public double PositionSec { get; set; } // { return _PositionSec; } set { Set(ref this._PositionSec, value); } }

    public LkuGenre Genre { get { return _Genre; } set { Set(ref this._Genre, value); } }
    LkuGenre _Genre;
    public string Notes { get { return _Notes; } set { Set(ref this._Notes, value); } }
    string _Notes;
    public string TmpMsg { get { return _TmpMsg; } set { Set(ref this._TmpMsg, value); } }
    string _TmpMsg;
    public DateTime AddedAt { get { return _AddedAt; } set { Set(ref this._AddedAt, value); } }
    DateTime _AddedAt;
    public DateTime? DeletedAt { get { return _DeletedAt; } set { Set(ref this._DeletedAt, value); } }
    DateTime? _DeletedAt;
    public DateTime? LastPeekAt { get { return _LastPeekAt; } set { Set(ref this._LastPeekAt, value); } }
    public string LastPeekPC { get { return _LastPeekPC; } set { Set(ref this._LastPeekPC, value); } }
    DateTime? _LastPeekAt; //last time of partial viewing (potentially different from the file's LastWriteTime).
    public bool? PassedQA { get { return _PassedQA; } set { Set(ref this._PassedQA, value); } }
    bool? _PassedQA = null; string _LastPeekPC; //last time of partial viewing (potentially different from the file's LastWriteTime).

    internal void DeleteFileFixMetaFile(string file)
    {
      DeletedAt = DateTime.Now;
      SaveMetaData(this);

      Task.Factory.StartNew(() =>
      {
        while (File.Exists(file))
        {
          System.Threading.Thread.Sleep(999);
          try
          {
            var mdfn = metadataFilename(file);
            var mdel = metadataFilenameDeleted(file);
            var fdel = Path.GetDirectoryName(mdel);
            try { if (!Directory.Exists(fdel)) Directory.CreateDirectory(fdel); }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
            if (File.Exists(file))
            {
#if !DEBUG
              File.Delete(file);
              if (Directory.Exists(fdel) && File.Exists(mdfn)) File.Move(mdfn, mdel);
#endif
            }
          }
          catch (Exception ex)
          {
            Debug.Write(ex);            //Logger.LogException(ex, MethodInfo.GetCurrentMethod());
          }
        }
      });
    }
    internal void MoveToFolder(string subfolder)
    {
      SaveMetaData(this);

      Task.Factory.StartNew(() =>
      {
        var trgFldr = Path.Combine(Path.GetDirectoryName(PathFileCur), subfolder);
        var trgFile = Path.Combine(trgFldr, Path.GetFileName(PathFileCur));

        while (File.Exists(PathFileCur))
        {
          System.Threading.Thread.Sleep(999);
          try
          {
            try { if (!Directory.Exists(trgFldr)) Directory.CreateDirectory(trgFldr); }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
            if (File.Exists(PathFileCur))
              if (Directory.Exists(trgFldr))
                File.Move(PathFileCur, trgFile);
              else
                return; ///unable to create directory: exit
					}
          catch (Exception ex)
          {
            Debug.Write(ex);            //Logger.LogException(ex, MethodInfo.GetCurrentMethod());
            System.Media.SystemSounds.Hand.Play();
          }
        }

        PathFileCur = trgFile;
        SaveMetaData(this);
      });
    }
    internal void SaveLink(string s)
    {
      OrgHttpLink = s;
      SaveMetaData(this);
    }
    internal void Rename(string newFilename)
    {
      SaveMetaData(this);

      var pathFileNew = Path.Combine(Path.GetDirectoryName(PathFileCur), newFilename + Path.GetExtension(PathFileCur));
      var metadataOld = metadataFilename(PathFileCur);
      var metadataNew = metadataFilename(pathFileNew);

      if (File.Exists(PathFileCur)) File.Move(PathFileCur, pathFileNew);
      if (File.Exists(metadataOld)) File.Move(metadataOld, metadataNew);

      PathFileCur = pathFileNew;
    }
    internal void AddAuditionResetPosToStart()
    {
      Position = TimeSpan.Zero;
      LastPeekAt = DateTime.Now;
      LastPeekPC = Environment.MachineName;
      Auditions.Add(new MuAudition { DoneAt = DateTime.Now, DoneBy = Environment.UserName, ID = Auditions.Count, PartyMode = false });
      //SaveMetaData(this);
    }

    static bool isMetadata(string f) { return f != null && f.EndsWith(_vpc, StringComparison.InvariantCultureIgnoreCase); }

    public const string _key = @"HKEY_CURRENT_USER\Software\Gabest\Media Player Classic\Settings";

    static double getMediaPlayerCalssicPositionInSec(string fn)
    {
      long mkSec = 0;
      for (int i = 0; i < 10; i++)
      {
        var s = (string)Registry.GetValue(_key, $"File Name {i}", null);
        if (s == null) break;

        if (s.ToLower().Contains(fn.ToLower()))
        {
          var p = (string)Registry.GetValue(_key, $"File PrgPosition {i}", null);
          if (p == null) break;
          if (long.TryParse(p, out mkSec))
            break;
          else
            return 0;
        }
      }
      return mkSec * .0000001;
    }
    static void setMediaPlayerCalssicPositionInSec(string fn, double posInSeconds)
    {
      long mkSec = 0;
      for (int i = 0; i < 10; i++)
      {
        var s = (string)Registry.GetValue(_key, $"File Name {i}", null);
        if (s == null) break;

        if (s.ToLower().Contains(fn.ToLower()))
        {
          var p = (string)Registry.GetValue(_key, $"File PrgPosition {i}", null);
          if (p == null) break;
          if (long.TryParse(p, out mkSec)) // only if value already there (no point to set if not).
          {
            Registry.SetValue(_key, $"File PrgPosition {i}", posInSeconds / .0000001);
            break;
          }
        }
      }
    }

    internal void OrderByPos()
    {
      var tmp = Bookmarks.OrderBy(r => r.PositionSec).ToArray();

      for (int i = 1; i < tmp.Length; i++)
      {
        tmp[i].DeltaSec = tmp[i].PositionSec - tmp[i - 1].PositionSec;
        EventTime = TimeSpan.FromSeconds(tmp[i].PositionSec - tmp[0].PositionSec);
      }

      Bookmarks = new ObservableCollection<MuBookmark>(tmp);
    }

  }
}
