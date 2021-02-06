using AAV.Sys.Helpers;
using AsLink;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;

namespace VPC.Models
{
  public class FolderViewModel
  {
    readonly string[] _exts;
    string _CurFile;
    int _CurIndx = -1;
    ObservableCollection<MediaUnit> _MediaUnits = new ObservableCollection<MediaUnit>();

    public FolderViewModel()
    {
      _exts = AsLink.MediaHelper.AllMediaExtensionsAry;
    }
    public string CurFile
    {
      get { return _CurFile; }
      set
      {
        _CurFile = (value);
      }
    }


    public int CurIndx { get { return _CurIndx; } set { _CurIndx = value; } }
    public ObservableCollection<MediaUnit> MediaUnits { get { return _MediaUnits; } set { _MediaUnits = value; } }

    public bool CanGetNext { get { return _CurIndx < _MediaUnits.Count - 1; } }
    public bool CanGetPrev { get { return _CurIndx > 0; } }
    public string GetNext { get { return CanGetNext ? _MediaUnits[_CurIndx + 1].PathFileCur : null; } }
    public string GetPrev { get { return CanGetPrev ? _MediaUnits[_CurIndx - 1].PathFileCur : null; } }

    public bool CanGetUpDr { get { return true; } }
    public string GetUpDr
    {
      get
      {
        if (CanGetUpDr)
        {
          var upDir = Path.GetDirectoryName(_CurFile).Substring(0, Path.GetDirectoryName(_CurFile).LastIndexOf('\\'));
          _CurIndx = 0;
          PopulateFromFolder(upDir, false);
        }

        return _MediaUnits.Count > 0 ? _MediaUnits[_CurIndx].PathFileCur : _CurFile;
      }
    }

    public void LoadDirFromFile(string file, bool inclSubDirs)
    {
      if (Directory.Exists(file))
        PopulateFromFolder(file, inclSubDirs); // 				
      else
      {
        PopulateFromFolder(Path.GetDirectoryName(file), inclSubDirs); // 				
        _CurFile = file;
      }
    }
    public void PopulateFromFolder(string folder, bool inclSubDirs)
    {
      if (string.IsNullOrEmpty(folder)) return;

      new DirectoryInfo(folder).GetFiles("*.*", inclSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
        .Where(f => _exts.Contains(Path.GetExtension(f.Extension.ToLower())))
        .OrderByDescending(f => f.LastWriteTime)
        .ToList()
        .ForEach(fi => addTrySelect(fi));


      //var lst = new List<CrntMU>();
      //foreach (var file in Directory.GetFiles(folder).Where(f => _exts.Contains(Path.GetExtension(f).ToLower())))
      //{
      //	if (string.Compare(file, _CurFile, true) == 0) _CurIndx = _MediaUnits.Count;
      //	lst.Add(CrntMU.LoadMetaData(file));
      //}

      //_MediaUnits.Clear();
      //lst
      //	.OrderBy(r => r.Auditions.Count)
      //	.ThenByDescending(r => r.PositionSec)
      //	.ThenBy(r => r.FileName)
      //	.ToList().ForEach(r => _MediaUnits.Add(r)); // == foreach (var l in lst.OrderBy(r => r.Auditions.Count).ThenBy(r => r.FileName)) _MediaUnits.Add(l);
    }
    public void PopulateFromSkyCache(bool inclSubDirs)
    {
      //////var fis = new List<FileInfo>(); // foreach (var filename in Directory.GetFiles(OneDrive.AlexsVpdbFolder))				fis.Add(new FileInfo(filename));
      //////Array.ForEach(Directory.GetFiles(OneDrive.AlexsVpdbFolder), f => fis.Add(new FileInfo(f))); // Directory.GetFiles(OneDrive.AlexsVpdbFolder).ToList().ForEach(f => fis.Add(new FileInfo(f)));
      //////fis.OrderByDescending(fi => fi.LastWriteTime).ToList().ForEach(fi => Debug.Write(string.Format("{0} - {1}\n", fi.LastWriteTime, fi.Name)));

      _MediaUnits.Clear();
      new DirectoryInfo(OneDrive.VpdbFolder).GetFiles("*.VPC", inclSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
        .OrderByDescending(f => f.LastWriteTime)
        .ToList()
        .ForEach(fi => addTrySelect(fi));


      Task.Run(async delegate
      {
        await Task.Delay(20); //??
        SystemSounds.Beep.Play();
        foreach (var mu in MediaUnits) mu.TmpMsg = File.Exists(mu.PathFileCur) ? "██" : "░░";
        SystemSounds.Beep.Play();
      }).ContinueWith(_ =>
      {
        SystemSounds.Hand.Play();
      }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    void addTrySelect(FileInfo fi)
    {
      var mu = MediaUnit.LoadMetaData(fi.FullName);
      _MediaUnits.Add(mu);
      if (string.Compare(mu.PathFileCur, _CurFile, true) == 0) _CurIndx = _MediaUnits.Count;
    }
  }
}
