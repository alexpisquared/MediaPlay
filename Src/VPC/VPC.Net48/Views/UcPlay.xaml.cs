using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VPC.Views
{
  public partial class UcPlay : UserControl
  {
    public UcPlay() { InitializeComponent(); }

    static void callbk(DependencyObject d, DependencyPropertyChangedEventArgs e) { if (e.NewValue is bool && (bool)e.NewValue == true) (d as UcPlay).runAnimation(); }

    void runAnimation()
    {
      Visibility = Visibility.Visible;
      //(FindResource("sbFlashMeA") as Storyboard).TargetName = "ucFlasher";
      (FindResource("sbFlashMeA") as Storyboard).Begin();
    }

    public bool FlashOnTrue { get { return (bool)GetValue(FlashProperty); } set { SetValue(FlashProperty, value); } }
    public static readonly DependencyProperty FlashProperty = DependencyProperty.Register("FlashOnTrue", typeof(bool), typeof(UcPlay), new PropertyMetadata(false, new PropertyChangedCallback(callbk)));
  }
}
