using VpxCmn.Model;
using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace VideoPlayerBackground.Vws
{
  public sealed partial class Pg2_MruListUC : UserControl
  {
    public Pg2_MruListUC()
    {
      this.InitializeComponent();
      gvMini1.DataContext = MP?.MruLst;
    }
    MainPageAbr mp; public MainPageAbr MP { get => mp; set { mp = value; gvMini1.DataContext = mp?.MruLst; } }

    void onRemoveCurSel(object sender, RoutedEventArgs e) { MP.MruLst.Remove(MP.Cur); }
    async void onDeleteCurSel(object sender, RoutedEventArgs e)
    {
      var messageDialog = new MessageDialog($"Delete: {MP.Cur.PathFile}", "Are you sure?");

      messageDialog.Commands.Add(new UICommand("Yes", null, 0));
      messageDialog.Commands.Add(new UICommand("No", null, 1));

      messageDialog.DefaultCommandIndex = 1;
      messageDialog.CancelCommandIndex = 1;

      var rv = await messageDialog.ShowAsync();
      if ((int)((UICommand)rv).Id != 0)
        return;

      var sf = await StorageFile.GetFileFromPathAsync(MP.Cur.PathFile);
      await sf.DeleteAsync(StorageDeleteOption.Default);
      MP.MruLst.Remove(MP.Cur);
      onRefreshList(sender, e);
    }
    void onRefreshList(object sender, RoutedEventArgs e) { ((Button)sender).IsEnabled = false; try { MP.ReadFromSettingsMRU(); } finally { ((Button)sender).IsEnabled = true; } } // fs is the truth, as it is updated every 15 sec at least.
    async void onRefreshThumbs(object sender, RoutedEventArgs e)
    {
      ((Button)sender).IsEnabled = false;
      try
      {
        foreach (var mid in MP?.MruLst)
        {
          if (!await MP.MidExists(mid))
            continue;

          var sf = await StorageFile.GetFileFromPathAsync(mid.PathFile);
          await mid.SetThumbnail(sf);
        }
      }
      finally { ((Button)sender).IsEnabled = true; }
    }
    async void onSelChngd(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        if (e.AddedItems.Count <= 0) return;

        var sel = (MediaInfoDto)e.AddedItems[0];
        MP.Cur = sel;
        if (await MP.MidExists(sel))
        {
          btnDel.IsEnabled = true;
          MP.Cur.MuExists = "+++";
          MP.Cur.PcBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
          await MP.LoadPlay1_pathToStoreFile(MP.Cur.PathFile);
        }
        else
        {
          btnDel.IsEnabled = false;
          MP.Cur.MuExists = "---";
          MP.Cur.PcBrush = new SolidColorBrush(Color.FromArgb(255, 255, 64, 0));
          await MP.Speak($"File does not exist on this PC.");
        }
      }
      catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await MP.popEx(ex, "Pg2.SelChngd"); }
    }
    async void onChkLocal(object sender, RoutedEventArgs e)
    {
      ((Button)sender).IsEnabled = false;
      try
      {
        foreach (var mid in MP?.MruLst)
        {
          if (await MP.MidExists(mid))
          {
            mid.MuExists = "++";
            mid.PcBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
          }
          else
          {
            mid.MuExists = "--";
            mid.PcBrush = new SolidColorBrush(Color.FromArgb(255, 255, 64, 0));
          }
        }
      }
      finally { ((Button)sender).IsEnabled = true; }
    }
  }
}
