using ABR.VMs;
using MVVM.Common;
using VpxCmn.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws
{
  public sealed partial class Pg2_MruUC : UserControl
  {
    AbrVM _abrVM;
    public Pg2_MruUC()
    {
      this.InitializeComponent();
      if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;

      _abrVM = ViewModelDispatcher.AbrVM;
    }

    MediaInfoDto getDataModelForCurrentListViewFlyout()
    {
      //var listViewItem = menuFlyout1.Target;                   // Obtain the ListViewItem for which the user requested a context menu.
      //return (MediaInfoDto)lv1.ItemFromContainer(listViewItem); // Get the data model for the ListViewItem.
      return (MediaInfoDto)lv1.ItemFromContainer(menuFlyout1.Target);
    }

    void onRemove(object sender, RoutedEventArgs e) => _abrVM.RemoveFromMruOnly((MediaInfoDto)lv1.ItemFromContainer(menuFlyout1.Target));
    void onThumbR(object sender, RoutedEventArgs e) => _abrVM.ReThumbFile((MediaInfoDto)lv1.ItemFromContainer(menuFlyout1.Target));
    void onDelete(object sender, RoutedEventArgs e) => _abrVM.DeleteMedia((MediaInfoDto)lv1.ItemFromContainer(menuFlyout1.Target));
	}
}