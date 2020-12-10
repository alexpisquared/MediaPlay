using ABR.VMs;
using MVVM.Common;
using Windows.UI.Xaml.Controls;
namespace ABR.Vws { public sealed partial class Pg3_VideoUC : UserControl { AbrVM _abrVM; public Pg3_VideoUC() { this.InitializeComponent(); _abrVM = ViewModelDispatcher.AbrVM; } } }