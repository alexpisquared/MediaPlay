using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AsLink;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace ApxCmn
{
    [DataContract]
  public partial class MediaInfo // Simple representation for songs in a playlist that can be used both for data model (across processes) and view model (foreground UI)
  {
    [DataMember] public string Title { get; set; }
    [DataMember] public Uri MediaUri { get; set; }
    [DataMember] public Uri ArtUri { get; set; }
    [DataMember] public string FInfo { get; set; }
    [DataMember] public string FName { get; set; } // = "Filename.ext only";
    [DataMember] public string PathO { get; set; } // = "Path Only";
    [DataMember] public string FullPath { get; set; } // = "Path File Ext";
    [DataMember] public double TotalSeconds { get; set; } = 123.45;
    public StorageFile SFile { get; set; }
    public BitmapImage Thumbnail { get; set; }

    public MediaInfo() { }
    public MediaInfo(StorageFile sFile)
    {
      //MediaPlayer.SetSource(await storageFile.OpenAsync(FileAccessMode.Read), contentType);      //MediaPlayerStoryboard.Begin();			// https://msdn.microsoft.com/en-us/library/windows/apps/mt187272.aspx

      SFile = sFile;
      FName = sFile.Name;
      PathO = Path.GetDirectoryName(sFile.Path);
      FullPath = Path.GetFullPath(sFile.Path);

      Title = sFile.Name;
      MediaUri = new Uri(FullPath);

      Debug.Assert(sFile != null);

      setThumbnail();
    }


    public async Task<StorageFile> GetStorageFile()
    {
      Debug.Write($"\r\n*/> Getting StorageFile from FullPath: {FullPath} ... \r\n");

      //StorageFile SFile = null;
      try { SFile = await StorageFile.GetFileFromPathAsync((FullPath)); } catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ }
      if (SFile != null) return SFile;
      try { SFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(FullPath)); } catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ }
      if (SFile != null) return SFile;
      try { SFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(FullPath, UriKind.Absolute)); } catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ }
      if (SFile != null) return SFile;
      try { SFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(FullPath, UriKind.Relative)); } catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ }
      if (SFile != null) return SFile;
      try { SFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(FullPath, UriKind.RelativeOrAbsolute)); } catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ }
      if (SFile != null) return SFile;

      return SFile;
      //return await StorageFile.GetFileFromPathAsync(FullPath);
    }
    async void setThumbnail(uint size = 100, ThumbnailMode mode = ThumbnailMode.MusicView)
    {
      Thumbnail = await IsoStorePoc.GetThumbnailBitmapImage(SFile);
      ArtUri = Thumbnail.UriSource ?? new Uri(FullPath);                      //FInfo = $"Thumbnail Mode: {mode}\n Requested/Returned size: {size} / {Thumbnail.OriginalWidth}x{Thumbnail.OriginalHeight}";
    }

    async void get__More(StorageFile file)
    {
      Debug.Write($"file:{file.Name,-44}  ");
      switch (file.ContentType)
      {
        default: Debug.WriteLine($"(*&: New type: '{file.ContentType}'."); break;
        case "audio": var musProps = await file.Properties.GetMusicPropertiesAsync(); Debug.WriteLine(($"Durn:{musProps?.Duration}")); break;// GetImagePropertiesAsync will return synchronously when prefetching has been able to retrieve the properties in advance.
        case "video": var vidProps = await file.Properties.GetVideoPropertiesAsync(); Debug.WriteLine(($"Hght:{vidProps?.Height}")); break;
      }

      // Thumbnails can also be retrieved and used. // var thumbnail = await file.GetThumbnailAsync(thumbnailMode, requestedSize, thumbnailOptions);
    }
  }
}
