using System.Windows;
using System.Windows.Input;

namespace DDJ.Main.Views
{
  public partial class MainPlayerView : AAV.WPF.Base.WindowBase
  {
    public MainPlayerView() => InitializeComponent();

    void onClick1(object sender, RoutedEventArgs e) => new KeyViewer().Show();
    void onClick2(object sender, RoutedEventArgs e) { new xPositionCircularSlider().ShowDialog(); bFcs.Focus(); }
    void onClick3(object sender, RoutedEventArgs e) => Title = $"{FocusManager.GetFocusedElement(this)} - {Keyboard.FocusedElement}";
  }
}
