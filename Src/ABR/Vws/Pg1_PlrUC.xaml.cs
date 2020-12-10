using ABR.VMs;
using MVVM.Common;
using VpxCmn.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws
{
    public sealed partial class Pg1_PlrUC : UserControl
    {
        AbrVM _abrVM;
        public Pg1_PlrUC()
        {
            this.InitializeComponent();
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;
            _abrVM = ViewModelDispatcher.AbrVM;
            _abrVM.MpeVm = mpeXm;
        }
        public MediaPlayerElement MpeXm { get { return mpeXm; } }

        void onRemove(object sender, RoutedEventArgs e) => _abrVM.RemoveFromMruOnly((MediaInfoDto)lv1.ItemFromContainer(menuFlyout1.Target));
        void onDelete(object sender, RoutedEventArgs e) => _abrVM.DeleteMedia((MediaInfoDto)lv1.ItemFromContainer(menuFlyout1.Target));
        void onThumbR(object sender, RoutedEventArgs e) => _abrVM.ReThumbFile((MediaInfoDto)lv1.ItemFromContainer(menuFlyout1.Target));
        //void onThmnailZoom(object s, RoutedEventArgs e) => img1.Margin = ((AppBarToggleButton)s).IsChecked == null ? new Thickness(96, 48, 154, 48) : ((AppBarToggleButton)s).IsChecked == true ? new Thickness(48, 0, 96, 0) : new Thickness(0, -48, 0, -48);
    }
}