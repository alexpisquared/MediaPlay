using ABR.VMs;
using ABR.Vws;
using AsLink;
using MVVM.Common;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ABR
{
    public sealed partial class AbrMainPg : Page
    {
        AbrVM _abrVM;
        public AbrMainPg()
        {
            this.InitializeComponent();
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;

            _abrVM = ViewModelDispatcher.AbrVM;
            btnPinTile.Visibility = /*WinTileHelper.IsPinned ? Visibility.Collapsed : */Visibility.Visible;

#if DEBUG
            ApplicationView.GetForCurrentView().Title = /*tbVer.Text =*/ $@"Dbg: built {(DateTime.Now - DevOp.BuildTime(typeof(App))).TotalDays:N1} days ago";
#else
            ApplicationView.GetForCurrentView().Title = /*tbVer.Text =*/ $@"Rls: {DevOp.BuildTime(typeof(App))}";
#endif
        }
        public Pg1_PlrUC Pg1_PlrUC1 { get => pg1_PlrUC1; }

        async void onPinTile(object sender, RoutedEventArgs e) { t4.Text = await WinTileHelper.PinTile(sender); ((AppBarButton)sender).Visibility = Visibility.Collapsed; }

    }
}