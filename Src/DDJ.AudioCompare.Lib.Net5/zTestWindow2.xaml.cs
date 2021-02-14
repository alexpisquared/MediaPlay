using System.Windows;

namespace DDJ.AudioCompare.Lib
{
    /// <summary>
    /// Interaction logic for zTestWindow2.xaml
    /// </summary>
    public partial class zTestWindow2 : Window
	{
		public zTestWindow2()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{

			System.Windows.Data.CollectionViewSource vwDuplicateViewSource = ((System.Windows.Data.CollectionViewSource)(FindResource("vwDuplicateViewSource")));
			// Load data by setting the CollectionViewSource.Source property:
			// vwDuplicateViewSource.Source = [generic data source]
		}
	}
}
