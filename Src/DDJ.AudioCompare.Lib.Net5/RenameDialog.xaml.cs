using System.Windows;

namespace DDJ.AudioCompare.Lib
{
    public partial class RenameDialog : Window
	{
		public RenameDialog()
		{
			InitializeComponent();
		}


		public string FileName { get { return (string)GetValue(FileNameProperty); } set { SetValue(FileNameProperty, value); } }		public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register("FileName", typeof(string), typeof(RenameDialog), new PropertyMetadata("123abc.mp3"));

		private void Button_Click(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }
		private void Button_Click_1(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
	}
}
