using ApxCmn;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace xPocBits.UCs
{
	public sealed partial class GhsPlaylistUC : UserControl
	{
		ObservableCollection<MediaInfo> songs = new ObservableCollection<MediaInfo>();

		public GhsPlaylistUC()
		{
			this.InitializeComponent();

			listView.ItemsSource = Songs;
		}

		public int SelectedIndex { get { return listView.SelectedIndex; } set { listView.SelectedIndex = value; } }

		public event ItemClickEventHandler ItemClick { add { listView.ItemClick += value; } remove { listView.ItemClick -= value; } }
		public event SelectionChangedEventHandler SelectionChanged { add { listView.SelectionChanged += value; } remove { listView.SelectionChanged -= value; } }

		public ObservableCollection<MediaInfo> Songs { get { return songs; } }

		public MediaInfo GetSongById(Uri id) { return songs.Single(s => s.MediaUri == id); }
		public int GetSongIndexById(Uri id) { return songs.ToList().FindIndex(s => s.MediaUri == id); }
	}
}