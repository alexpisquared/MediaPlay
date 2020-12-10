using AsLink;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VpxCmn.Model
{
    [PropertyChanged.AddINotifyPropertyChangedInterface] //fody Oct 2017: started getting odd errors after Nuget Update => do we even need it here?
    [DataContract]
  public class MediaInfoDto : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public MediaInfoDto(StorageFile sFile)
    {
      SFile = sFile;      //MediaPlayer.SetSource(await storageFile.OpenAsync(FileAccessMode.Read), contentType);      //MediaPlayerStoryboard.Begin();			// https://msdn.microsoft.com/en-us/library/windows/apps/mt187272.aspx
      PathFile = Path.GetFullPath(sFile.Path);
      MediaUri = new Uri(PathFile);
      PcBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
      //SetThumbnail(sFile);
      getAsync();
    }
    async void getAsync()
    {
      var a = await SFile.GetBasicPropertiesAsync();
      Size = a.Size;
      PublDate = a.ItemDate.DateTime;

			Debug.WriteLine($"::Dates> {SFile.DateCreated:yy-MM-dd HHmm} - {a.DateModified:yy-MM-dd HHmm} - {a.ItemDate:yy-MM-dd HHmm} = {a.DateModified-a.ItemDate:hhmm}");
    }
    async public static Task<MediaInfoDto> Create(FileInfo fi)
    {
      var rrv = new MediaInfoDto(await StorageFile.GetFileFromPathAsync(fi.FullName));
      rrv.Size = (ulong)fi.Length;
      return rrv;
    }

    public async Task SetThumbnail() {if(await FileExists()) Thumbnail = await IsoStorePoc.GetThumbnailBitmapImage(await SafeSFile()); }
    public async Task SetThumbnail(StorageFile sf) { if (sf != null) Thumbnail = await IsoStorePoc.GetThumbnailBitmapImage(sf); }

    public async Task<bool> FileExists()
    {
      bool exists = false;
      try
      {
        var sf = await StorageFile.GetFileFromPathAsync(PathFile);
        if (sf != null)
          exists = true;
      }
      catch (UnauthorizedAccessException) { return false; }
      catch (FileNotFoundException) { return false; }
      catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else throw; } // await doExn(ex, PathFile); }
      finally { ExistBrush = exists ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Magenta); }

      return exists;
    }

    async public Task<StorageFile> SafeSFile() => SFile ?? await StorageFile.GetFileFromPathAsync(PathFile);
    public StorageFile SFile { get; set; }
    public BitmapImage Thumbnail { get; set; }
    public string NameOnly
    {
      get
      {
        var fnwe = Path.GetFileNameWithoutExtension(PathFile);
        if (fnwe.Length > 32 && fnwe.Contains("__"))
        {
          var a = fnwe.Split(new[] { " ", "-" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
          if (a != null && a.Length < fnwe.Length)
          {
            return fnwe.Substring(0, fnwe.Length - a.Length);
          }
        }

        return fnwe;
      }
    }
    public string FileOnly { get { return Path.GetFileName(PathFile); } }
    public string PathOnly { get { return Path.GetDirectoryName(PathFile); } }
    public string PathAbrv { get { return Path.GetDirectoryName(PathFile).Replace(@"C:\Data\Users\Public\", "").Replace(@"Music\", "").Replace(@"Videos\", ""); } }
    public Uri MediaUri { get; private set; }
    public SolidColorBrush PcBrush { get; set; } = new SolidColorBrush(Colors.Yellow);
    public SolidColorBrush ExistBrush { get; set; } = new SolidColorBrush(Colors.White);
    public string MuExists { get; set; }
    public ulong Size { get; private set; }
    public double SizeMb => Size * .000001;

    [DataMember] public TimeSpan? PlayLeng { get; set; }
    [DataMember] public string PathFile { get; set; }
    [DataMember] public string LastPcNm { get; set; }
    [DataMember] public TimeSpan PlayPosn { get; set; }
    [DataMember] public DateTime? LastUsed { get; set; }
    [DataMember] public DateTime? PublDate { get; set; }
  }

  public class AppState
  {
    public bool AutoPlay { get; set; } = true;
    public string LastLocalMedia { get; set; }
  }
}
