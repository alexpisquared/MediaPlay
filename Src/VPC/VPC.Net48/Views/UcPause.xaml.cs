using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VPC.Views
{
	public partial class UcPause : UserControl
	{
		public UcPause() { InitializeComponent(); }

		static void callbk(DependencyObject d, DependencyPropertyChangedEventArgs e) { if (e.NewValue is bool && (bool)e.NewValue == false) (d as UcPause).runAnimation(); }

		void runAnimation() { Visibility = Visibility.Visible; (FindResource("sbFlashMeB") as Storyboard).Begin(); }

		public bool FlashOnFalse { get { return (bool)GetValue(FlashProperty); } set { SetValue(FlashProperty, value); } }		public static readonly DependencyProperty FlashProperty = DependencyProperty.Register("FlashOnFalse", typeof(bool), typeof(UcPause), new PropertyMetadata(false, new PropertyChangedCallback(callbk)));
	}
}
