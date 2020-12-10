using ABR.VMs;
using MVVM.Common;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws { public sealed partial class Pg3_MusicUC : UserControl { AbrVM _abrVM; public Pg3_MusicUC() { this.InitializeComponent(); _abrVM = ViewModelDispatcher.AbrVM; } } }