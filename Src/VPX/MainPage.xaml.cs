using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace VPX
{
    public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			//me1.Source = new Uri(e.Uri);

			tb1.Text = $" e?.Parameter:{e?.Parameter} -- e?.Content:{e?.Content} -- App.Arguments:{App.Arguments}.";

		}

		protected override void OnKeyDown(KeyRoutedEventArgs e)
		{
			base.OnKeyDown(e);

			switch (e.Key)
			{
				case VirtualKey.Escape: CoreApplication.Exit(); break;
				default: Debug.WriteLine($"case VirtualKey.{e.Key}:            break;"); break;
			}
		}
	}
}
