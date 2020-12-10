using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDJ.Main.Views
{
    public partial class xUcPositionCircularSlider : UserControl
	{
		bool _isMouseDown = false;
		double x0, y0, v0;
		const double lineHieght = 50, rectWidth = 100, marginX = 100, marginY = 100;

		public double Min { get { return (double)GetValue(MinProperty); } set { SetValue(MinProperty, value); } }		public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(xUcPositionCircularSlider), new UIPropertyMetadata(1.0));
		public double Max { get { return (double)GetValue(MaxProperty); } set { SetValue(MaxProperty, value); } }		public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(xUcPositionCircularSlider), new UIPropertyMetadata(9.9));
		public double Val { get { return (double)GetValue(ValProperty); } set { SetValue(ValProperty, value); } }		public static readonly DependencyProperty ValProperty = DependencyProperty.Register("Val", typeof(double), typeof(xUcPositionCircularSlider), new UIPropertyMetadata(7.7));
		public double Step { get { return (double)GetValue(StepProperty); } set { SetValue(StepProperty, value); } }		public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(double), typeof(xUcPositionCircularSlider), new UIPropertyMetadata(1.0));
		public double ValW { get { return (double)GetValue(ValWProperty); } set { SetValue(ValWProperty, value); } }		public static readonly DependencyProperty ValWProperty = DependencyProperty.Register("ValW", typeof(double), typeof(xUcPositionCircularSlider), new UIPropertyMetadata(1.0));

		public xUcPositionCircularSlider()
		{
			InitializeComponent();

			MouseLeftButtonDown += onDn;
			MouseLeftButtonUp += onUp;
			MouseMove += onMv;
			Loaded += (s, e) =>
			{
				//if (Max != Min && Val >= Min && Val <= Max) ValW = (recMouseAct.ActualWidth == 0.0 ? 150.0 : recMouseAct.ActualWidth) * (Val - Min) / (Max - Min);
			};
		}

		void setValueRectangleWidth()
		{
			if (Max != Min && Val >= Min && Val <= Max) ValW = (recMouseAct.ActualWidth == 0.0 ? 150.0 : recMouseAct.ActualWidth) * (Val - Min) / (Max - Min);
		}

		void onDn(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			UIElement el = (UIElement)sender; //			_point = e.MouseDevice.GetPosition(el);
			el.CaptureMouse();
			_isMouseDown = true;
			x0 = e.GetPosition((IInputElement)sender).X;
			y0 = e.GetPosition((IInputElement)sender).Y;
			v0 = Val;
			ln.Visibility = Visibility.Visible;
		}
		void onMv(object sender, MouseEventArgs e)
		{
			e.Handled = true;

			UIElement el = (UIElement)sender;
			if (el.IsMouseCaptured && _isMouseDown)
			{
				double x = e.GetPosition((IInputElement)sender).X;
				double y = e.GetPosition((IInputElement)sender).Y;

				ln.X2 = x;
				ln.Y2 = y;

				return;
			}
		}
		void onUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			UIElement el = (UIElement)sender;
			el.ReleaseMouseCapture();
			_isMouseDown = false;
			ln.Visibility = Visibility.Collapsed;
		}

	}
}
