using ABR.VMs;
using MVVM.Common;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws { public sealed partial class Pg3_LibUC : UserControl { AbrVM _abrVM; public Pg3_LibUC() { this.InitializeComponent(); if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return; _abrVM = ViewModelDispatcher.AbrVM; } } }