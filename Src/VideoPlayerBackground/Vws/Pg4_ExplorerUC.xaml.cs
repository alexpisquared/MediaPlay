using AsLink;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VpxCmn.Model;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VideoPlayerBackground.Vws
{
  public sealed partial class Pg4_ExplorerUC : UserControl
  {
    public ObservableCollection<MediaInfoDto> MediaInfos { get; set; } = new ObservableCollection<MediaInfoDto>();

    public Pg4_ExplorerUC()
    {
      this.InitializeComponent();
      lv1.DataContext = MediaInfos;
    }
    MainPageAbr mp; public MainPageAbr MP { get => mp; set { mp = value; } }

    async Task libLoad(KnownFolderId library)
    {
      try
      {
        var max = 1000;
        var sfs = await IsoStorePoc.LoadFromLibFolder(library);
        MediaInfos.Clear();
        sfs.Take(max).OrderBy(r => r.Path).ToList().ForEach(sf => MediaInfos.Add(new MediaInfoDto(sf)));
        ApplicationView.GetForCurrentView().Title = tbInfo.Text = $@"{MediaInfos.Count} / {sfs.Count} in {library}";
      }
      catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await MP.popEx(ex, "LibLoad"); }
      finally { }
    }

    async void onAu(object sender, RoutedEventArgs e) { await libLoad(KnownFolderId.MusicLibrary); }
    async void onVi(object sender, RoutedEventArgs e) { await libLoad(KnownFolderId.VideosLibrary); }
    async void onSelChngd(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        if (e.AddedItems.Count <= 0) return;

        var sel = (MediaInfoDto)e.AddedItems[0];
        if (await MP.MidExists(sel))
        {
          MP.Mru_FindAdd_MakeCur(await StorageFile.GetFileFromPathAsync(sel.PathFile));
          await MP.LoadPlay1_pathToStoreFile(MP.Cur.PathFile);
        }
        else
          await MP.Speak($"{sel.FileOnly} does not exist on this PC (deleted since last lib scan). Choose another.");
      }
      catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await MP.popEx(ex, "SelChngd"); }
    }
  }
}
