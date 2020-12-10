using AAV.Sys.Helpers;
using DDJ.DB.Models;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DDJ.Proto
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		DdjEf4DBContext _db = new DdjEf4DBContext();

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var sw = Stopwatch.StartNew();

			_db.MediaUnits.Take(7).Load();

			((System.Windows.Data.CollectionViewSource)(FindResource("mediaUnitViewSource"))).Source = _db.MediaUnits.Local;

			Title = sw.ElapsedMilliseconds.ToString();
      Bpr.BeepOk();
			System.Windows.Data.CollectionViewSource muAuditionViewSource = ((System.Windows.Data.CollectionViewSource)(FindResource("muAuditionViewSource")));
			// Load data by setting the CollectionViewSource.Source property:
			// muAuditionViewSource.Source = [generic data source]
		}
	}
}
