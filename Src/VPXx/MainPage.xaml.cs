using AsLink;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VpxCmn.Model;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VPXx
{
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);

      tbInfo.Text = $" ... ";

      try
      {
        _state = JsonHelper.FromJson<AppState>((string)AppSettingsHelper.ReadVal(typeof(AppState).Name));

        var args = e.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
        if (args?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
        {
          var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
          var strFilePath = fileArgs?.Files[0].Path;
          if (strFilePath != null)
          {
            me_Xm.AutoPlay = _state.AutoPlay;
            var sf = await StorageFile.GetFileFromPathAsync(strFilePath);
            await loadPlay(sf);
            return;
          }
        }

        if (_state.LastLocalMedia != null)
        {
          me_Xm.AutoPlay = _state.AutoPlay;
          var sf = await StorageFile.GetFileFromPathAsync(_state.LastLocalMedia);
          await loadPlay(sf);
          return;
        }

        var rv = await IsoStorePoc.LoadFromLibFolder(KnownFolderId.VideosLibrary);
        foreach (var storageFile in rv)
        {
          await loadPlay(storageFile);
          break;
        }
      }
      catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ }
    }
    protected override async void OnKeyDown(KeyRoutedEventArgs e)
    {
      base.OnKeyDown(e);

      updateAppSettings();

      switch (e.Key)
      {
        default:
          Debug.WriteLine($"case VirtualKey.{e.Key}:            break;");
          break;
        case VirtualKey.Space: if (me_Xm.CurrentState == MediaElementState.Playing) me_Xm.Pause(); else me_Xm.Play(); break;
        case VirtualKey.Escape: Windows.ApplicationModel.Core.CoreApplication.Exit(); break;
        case VirtualKey.O: await pickOpen(); break;
      }

    }
    async void mediaElement_Drop(object sender, DragEventArgs e)
    {
      if (e.DataView.Contains(StandardDataFormats.StorageItems))
      {
        var files = await e.DataView.GetStorageItemsAsync();
        foreach (var file in files)
        {
          //TextBlock t = new TextBlock();					//t.Text = file.Name;					//OutputDPFiles.Children.Add(t);

          me_Xm.Source = new Uri(((Windows.Storage.StorageFile)file).Path);
          break;
        }
      }

    }

    void me1_DragOver(object sender, DragEventArgs e)
    {
      // using Windows.ApplicationModel.DataTransfer;
      e.AcceptedOperation = DataPackageOperation.Copy;

      // Drag adorner ... change what mouse / icon looks like as you're dragging the file into the app: // http://igrali.com/2015/05/15/drag-and-drop-photos-into-windows-10-universal-apps/
      e.DragUIOverride.Caption = "drop to create a custom sound and tile";
      e.DragUIOverride.IsCaptionVisible = true;
      e.DragUIOverride.IsContentVisible = true;
      e.DragUIOverride.IsGlyphVisible = true;
      //e.DragUIOverride.SetContentFromBitmapImage(new BitmapImage(ne		}
    }

    void onMediaEnded(object sender, RoutedEventArgs e) { Debug.WriteLine(DevOp.GetCaller()); }
    void onMediaFailed(object sender, ExceptionRoutedEventArgs e) { Debug.WriteLine(DevOp.GetCaller()); }
    void onMediaSeeked(object sender, RoutedEventArgs e) { Debug.WriteLine(DevOp.GetCaller()); }
    void onMediaCoStChd(object sender, RoutedEventArgs e) { Debug.WriteLine(DevOp.GetCaller()); }
    void onMediaPmfd(MediaElement sender, PartialMediaFailureDetectedEventArgs args) { Debug.WriteLine(DevOp.GetCaller()); }
    void onMediaOpened(object sender, RoutedEventArgs e)
    {
      Debug.WriteLine(DevOp.GetCaller());

      try
      {
        if (_mid != null && me_Xm.CanSeek && me_Xm.NaturalDuration > _mid.PlayPosn)
        {
          me_Xm.Position = _mid.PlayPosn; // 				me1.Position = new TimeSpan(0, 1, 0, 0, 0);
        }

      }
      catch (Exception ex) { Debug.WriteLine(ex.Message); if (Debugger.IsAttached) Debugger.Break(); else throw; }
    }
    async void OnOpen(object sender, RoutedEventArgs e) { await pickOpen(); }
    void onJump(object sender, RoutedEventArgs e)
    {
      switch (((AppBarButton)sender).Label)
      {
        case "-5.": jump(-5.00); break;
        case "-1.": jump(-1.00); break;
        case "-.3": jump(-.300); break;
        case "+.3": jump(+.300); break;
        case "+1.": jump(+1.00); break;
        case "+5.": jump(+5.00); break;
        default:
          break;
      }
    }
    void onFaster(object sender, RoutedEventArgs e) { me_Xm.PlaybackRate += .25; }
    void onSlower(object sender, RoutedEventArgs e) { me_Xm.PlaybackRate -= .25; }

    void jump(double v)
    {
      if (v < 0)
        if (me_Xm.Position.TotalMinutes > -v)
          me_Xm.Position = me_Xm.Position + TimeSpan.FromMinutes(v);
        else
          me_Xm.Position = TimeSpan.Zero;
      else
        if (me_Xm.NaturalDuration > me_Xm.Position + TimeSpan.FromMinutes(v))
        me_Xm.Position = me_Xm.Position + TimeSpan.FromMinutes(v);
      else
        Debug.WriteLine("...654");
    }
    void updateAppSettings()
    {
      if (_mid != null)
      {
        _mid.PlayPosn = me_Xm.Position;
        _mid.PlayLeng = me_Xm.NaturalDuration.TimeSpan;
        AppSettingsHelper.SaveVal(_mid.FileOnly, JsonHelper.ToJson(_mid));

        _state.LastLocalMedia = Path.Combine(_mid.PathOnly, _mid.FileOnly);
        AppSettingsHelper.SaveVal(typeof(AppState).Name, JsonHelper.ToJson(_state));
      }
    }
    async Task pickOpen()
    {
      var picker = new FileOpenPicker();
      foreach (var ext in MediaHelper.AllMediaExtensionsAry) picker.FileTypeFilter.Add(ext);

      var storageFile = await picker.PickSingleFileAsync();
      if (storageFile == null) return;

      StorageApplicationPermissions.FutureAccessList.Add(storageFile); //from http://krzyskowk.postach.io/post/files-in-uwp - var picker1 = new FolderPicker(); var folder = await picker1.PickSingleFolderAsync(); if (folder != null) StorageApplicationPermissions.FutureAccessList.Add(folder);  [//tu: sems to be working ]
                                                                       // ales see  http://codezero.one/Details?d=1613&a=9&f=224&l=0&v=d&t=UWP,-NoGo:-allow-file-access  as a helpful workaround for future.
      await loadPlay(storageFile);
    }
    async Task loadPlay(StorageFile storageFile)
    {
      if (storageFile == null) return;

      tbInfo.Text += $"\r\n{storageFile.Name}";

      ApplicationView.GetForCurrentView().Title = storageFile.Name;

      var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
      me_Xm.SetSource(stream, storageFile.ContentType);

      var sval = (string)AppSettingsHelper.ReadVal(storageFile.Name);
      if (sval != null)
      {
        _mid = JsonHelper.FromJson<MediaInfoDto>(sval);
        if (_mid.FileOnly != null)
        {
          //nogo: not loaded yet: if (mid.PlayPosn.TotalSeconds > 15) me1.Position = mid.PlayPosn;

          me_Xm.Play(); //? does it work?
          return;
        }
      }

      _mid = new MediaInfoDto(storageFile);

      AppSettingsHelper.SaveVal(_mid.FileOnly, JsonHelper.ToJson(_mid));

      me_Xm.Play(); //? does it work?
    }
    MediaInfoDto _mid;
    AppState _state;
  }
}

/// https://msdn.microsoft.com/en-us/library/windows/apps/mt187272.aspx?f=255&MSPPError=-2147217396
/// 