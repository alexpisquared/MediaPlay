namespace VPC.Models;

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

    MediaUnit? mu;
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

    MediaUnit? mu;

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
      if (mu.DurationSec < minLengthWorthSaving && (mu.Duration.TotalSeconds < minLengthWorthSaving))
        return;
    }

    SaveMetaData_ALWAYS(mu);
  }
  public static bool IsAudio(string filename) => (".m4a.mp3.wma.wav".Contains(Path.GetExtension(filename).ToLower()));

  public static void SaveMetaData_ALWAYS(MediaUnit mu)
  {
    var mdfn = metadataFilename(mu.PathFileCur);
    _ = Serializer.SaveToFile(mu, mdfn);

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
  static string metadataFilenameDeleted(string file) => metadataFilename(file, @"Deleted\");

  //ObservableCollection<TimeSpan> _TSBookmarks = new ObservableCollection<TimeSpan>(); public ObservableCollection<TimeSpan> TSBookmarks { get { return _TSBookmarks; } set { Set(ref this._TSBookmarks, value); } }
  ObservableCollection<MuBookmark> _Bookmarks = new(); public ObservableCollection<MuBookmark> Bookmarks { get => _Bookmarks; set => Set(ref _Bookmarks, value); }
  ObservableCollection<MuAudition> _Auditions = new(); public ObservableCollection<MuAudition> Auditions { get => _Auditions; set => Set(ref _Auditions, value); }

  //[Key]
  int _ID; public int ID { get => _ID; set => Set(ref _ID, value); }
  string? _PathFileCur; public string PathFileCur { get => _PathFileCur; set { _ = Set(ref _PathFileCur, value); FileName = Path.GetFileName(value); PathName = Path.GetDirectoryName(value); } }
  string? _OrgHttpLink; public string OrgHttpLink { get => _OrgHttpLink; set => Set(ref _OrgHttpLink, value); }
  string? _PathFileOrg; public string PathFileOrg { get => _PathFileOrg; set => Set(ref _PathFileOrg, value); }
  string? _PathName; public string PathName { get => _PathName; set => Set(ref _PathName, value); }
  string? _FileName; public string FileName { get => _FileName; set => Set(ref _FileName, value); }
  long _FileHashMD5; public long FileHashMD5 { get => _FileHashMD5; set => Set(ref _FileHashMD5, value); }
  long _FileHashQck; public long FileHashQck { get => _FileHashQck; set => Set(ref _FileHashQck, value); }
  long _FileLength; public long FileLength { get => _FileLength; set => Set(ref _FileLength, value); }
  int _VideoHeight; public int VideoHeight { get => _VideoHeight; set => Set(ref _VideoHeight, value); }
  int _VideoWidth; public int VideoWidth { get => _VideoWidth; set => Set(ref _VideoWidth, value); }
  int _SpeedIdx = 8; public int SpeedIdx { get => _SpeedIdx; set => Set(ref _SpeedIdx, value); }
  bool _IsLooping = true; public bool IsLooping { get => _IsLooping; set => Set(ref _IsLooping, value); }
  double _AuVolume = 1.00; public double AuVolume { get => _AuVolume; set => Set(ref _AuVolume, value); }

  [XmlIgnore] public TimeSpan Duration { get => _Duration; set { if (Set(ref _Duration, value)) DurationSec = value.TotalSeconds; } }
  [XmlIgnore] public TimeSpan Position { get => _Position; set { if (Set(ref _Position, value)) PositionSec = value.TotalSeconds; } }
  [XmlIgnore] public TimeSpan EventTime { get => _EventTime; set => Set(ref _EventTime, value); }
  TimeSpan _Duration = TimeSpan.Zero, _Position = TimeSpan.Zero, _EventTime = TimeSpan.Zero;

  public double DurationSec { get; set; } // { return _DurationSec; } set { Set(ref this._DurationSec, value); } }
  public double PositionSec { get; set; } // { return _PositionSec; } set { Set(ref this._PositionSec, value); } }

  public LkuGenre Genre { get => _Genre; set => Set(ref _Genre, value); }
  LkuGenre? _Genre;
  public string Notes { get => _Notes; set => Set(ref _Notes, value); }
  string? _Notes;
  public string TmpMsg { get => _TmpMsg; set => Set(ref _TmpMsg, value); }
  string? _TmpMsg;
  public DateTime AddedAt { get => _AddedAt; set => Set(ref _AddedAt, value); }
  DateTime _AddedAt;
  public DateTime? DeletedAt { get => _DeletedAt; set => Set(ref _DeletedAt, value); }
  DateTime? _DeletedAt;
  public DateTime? LastPeekAt { get => _LastPeekAt; set => Set(ref _LastPeekAt, value); }
  public string LastPeekPC { get => _LastPeekPC; set => Set(ref _LastPeekPC, value); }
  DateTime? _LastPeekAt; //last time of partial viewing (potentially different from the file's LastWriteTime).
  public bool? PassedQA { get => _PassedQA; set => Set(ref _PassedQA, value); }
  bool? _PassedQA = null; string? _LastPeekPC; //last time of partial viewing (potentially different from the file's LastWriteTime).

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

    _ = Task.Factory.StartNew(() =>
    {
      var trgFldr = Path.Combine(Path.GetDirectoryName(PathFileCur), subfolder);
      var trgFile = Path.Combine(trgFldr, Path.GetFileName(PathFileCur));

      while (File.Exists(PathFileCur))
      {
        System.Threading.Thread.Sleep(999);
        try
        {
          try { if (!Directory.Exists(trgFldr)) _ = Directory.CreateDirectory(trgFldr); }
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

  static bool isMetadata(string f) => f != null && f.EndsWith(_vpc, StringComparison.InvariantCultureIgnoreCase);

  public const string _key = @"HKEY_CURRENT_USER\Software\Gabest\Media Player Classic\Settings";

  static double getMediaPlayerCalssicPositionInSec(string fn)
  {
    long mkSec = 0;
    for (var i = 0; i < 10; i++)
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
    for (var i = 0; i < 10; i++)
    {
      var s = (string)Registry.GetValue(_key, $"File Name {i}", null);
      if (s == null) break;

      if (s.ToLower().Contains(fn.ToLower()))
      {
        var p = (string)Registry.GetValue(_key, $"File PrgPosition {i}", null);
        if (p == null) break;
        long mkSec;
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

    for (var i = 1; i < tmp.Length; i++)
    {
      tmp[i].DeltaSec = tmp[i].PositionSec - tmp[i - 1].PositionSec;
      EventTime = TimeSpan.FromSeconds(tmp[i].PositionSec - tmp[0].PositionSec);
    }

    Bookmarks = new ObservableCollection<MuBookmark>(tmp);
  }
}
