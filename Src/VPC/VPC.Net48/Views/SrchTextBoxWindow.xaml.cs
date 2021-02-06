using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VPC.ViewModels;

namespace VPC.Views
{
	public partial class SrchTextBoxWindow : Window
	{
		VPViewModel _vm;
		public SrchTextBoxWindow(VPViewModel vm)
		{
			InitializeComponent();
			_vm = vm;
			MouseLeftButtonDown += (s, e) => { DragMove(); e.Handled = true; };
			KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };

			DataContext = this;
			Loaded += RenameWindow_Loaded;
		}
		async void RenameWindow_Loaded(object sender, RoutedEventArgs e) { await Task.Delay(99); tbSrch0.Focus(); }
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) { base.OnClosing(e); Loaded -= RenameWindow_Loaded; }

		public static readonly DependencyProperty SrchProperty = DependencyProperty.Register("Srch", typeof(string), typeof(SrchTextBoxWindow), new PropertyMetadata("", propChngdCallback)); public string Srch { get { return (string)GetValue(SrchProperty); } set { SetValue(SrchProperty, value); } }
		public static readonly DependencyProperty InclSubDirsProperty = DependencyProperty.Register("InclSubDirs", typeof(bool), typeof(SrchTextBoxWindow), new PropertyMetadata(false, propChngdCallback)); public bool InclSubDirs { get { return (bool)GetValue(InclSubDirsProperty); } set { SetValue(InclSubDirsProperty, value); } }
		static void propChngdCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is string)
				((SrchTextBoxWindow)d)._vm.Srch = ((string)e.NewValue);
			else if (e.NewValue is bool)
				((SrchTextBoxWindow)d)._vm.Sdir = ((bool)e.NewValue);
		}

	}
}

