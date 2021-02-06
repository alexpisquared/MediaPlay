using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace VPC.Views
{
	public partial class UsedMediaChoicesWindow : Window
	{
		public UsedMediaChoicesWindow() { InitializeComponent(); }

		public string SubFolder { get { return (string)GetValue(SubFolderProperty); } set { SetValue(SubFolderProperty, value); } }		public static readonly DependencyProperty SubFolderProperty = DependencyProperty.Register("SubFolder", typeof(string), typeof(UsedMediaChoicesWindow), new UIPropertyMetadata("???"));
		public string MediaFile { get { return (string)GetValue(MediaFileProperty); } set { SetValue(MediaFileProperty, value); } }		public static readonly DependencyProperty MediaFileProperty = DependencyProperty.Register("MediaFile", typeof(string), typeof(UsedMediaChoicesWindow), new UIPropertyMetadata("???"));
		public Dcsn Decision { get { return (Dcsn)GetValue(DecisionProperty); } set { SetValue(DecisionProperty, value); } }		public static readonly DependencyProperty DecisionProperty = DependencyProperty.Register("Decision", typeof(Dcsn), typeof(UsedMediaChoicesWindow));

		void onDelete(object sender, RoutedEventArgs e) { Decision = Dcsn.Delete; Close(); }
		void noNoMore(object sender, RoutedEventArgs e) { Decision = Dcsn.NoMore; Close(); }
		void onReplay(object sender, RoutedEventArgs e) { Decision = Dcsn.Replay; Close(); }
		void onEntrKy(object sender, RoutedEventArgs e) { Close(); }
		void btnMove_Click(object sender, RoutedEventArgs e)
		{
			SubFolder = ((Button)sender).Content.ToString().Replace("_", "").Trim();
			//r trgPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(MediaFile), SubFolder);
			var trgPath = System.IO.Path.Combine(@"C:\1\v\4Mei", SubFolder);
			if (!Directory.Exists(trgPath)) Directory.CreateDirectory(trgPath);
			MediaFile = System.IO.Path.Combine(trgPath, System.IO.Path.GetFileName(MediaFile));
			Decision = Dcsn.MoveTo;
			Close();
		}
	}

	public enum Dcsn	{		Delete = -7,		MoveTo = 3,		NoMore = 7,		ShutDn = 9,		Replay,	}
}
