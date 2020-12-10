using ApxCmn;
using System;
using System.Linq;
using System.Threading.Tasks;
using AsLink;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using MVVM.Common;

namespace xPocBits.VMs
{
  public partial class MainVM : ViewModelBase
  {
    async Task libLoad(KnownFolderId library)
    {
      IsReady = false;
      try
      {
        var max = 200;
        var sfs = await IsoStorePoc.LoadFromLibFolder(library);
        MediaInfos.Clear();
        sfs.Take(max).ToList().ForEach(sf => MediaInfos.Add(new MediaInfo(sf)));
        ApplicationView.GetForCurrentView().Title = Info = $@"Ttl {sfs.Count} in {library}, taking first {max}";
      }
      finally
      {
        IsReady = true;
      }
    }


    internal void ScenarioCleanup() { }
    async void setPlrSrc()
    {
      MediaElement.SetSource(await _SelectMI.SFile.OpenAsync(FileAccessMode.Read), _SelectMI.SFile.ContentType);
    }


  }
}
