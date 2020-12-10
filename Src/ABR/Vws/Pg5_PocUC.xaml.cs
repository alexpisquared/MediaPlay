using ABR.VMs;
using MVVM.Common;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws { public sealed partial class Pg5_PocUC : UserControl { AbrVM _abrVM; public Pg5_PocUC() { this.InitializeComponent(); if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return; _abrVM = ViewModelDispatcher.AbrVM; } } }