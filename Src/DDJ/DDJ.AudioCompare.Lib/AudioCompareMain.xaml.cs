using DDJ.AudioCompare.Lib.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AudioCompare
{
	public partial class AudioCompareMain : Window
	{
		MediaInfoDbSource _mediaInfoDbSrc = new MediaInfoDbSource();
		List<MediaElement> _mes = new List<MediaElement>();

		public AudioCompareMain(string mp3)
			: this()
		{
			tbxFilterAnd.Text = Settings.Default.LastFilter = mp3;
		}
		AudioCompareMain()
		{
			InitializeComponent();
		}

		private void fix(bool deleteDuplcates)
		{
			//Step 1: Swap listenedTimes table ids of deleted media files with the selected id
			MediaPlayerInfoUserControl goodFileToStay = getMediaPlayerInfoUserControl(lbxMediaUnits.SelectedItem);
			if (goodFileToStay == null)
			{
				MessageBox.Show("No good file selected or smth...");
				return;
			}

			var duplicatesToDeleteIDs = new List<long>();
			var duplicatesToDeleteCtrls = new List<MediaPlayerInfoUserControl>();
			foreach (object lbxItem in lbxMediaUnits.Items)
			{
				MediaPlayerInfoUserControl mib = getMediaPlayerInfoUserControl(lbxItem);
				var l = mib.MediaUnitID;
				if (l != goodFileToStay.MediaUnitID)
				{
					duplicatesToDeleteIDs.Add(l);
					duplicatesToDeleteCtrls.Add(mib);
				}
			}

			_mediaInfoDbSrc.SmartCascadingDeleteOfAll(duplicatesToDeleteIDs, goodFileToStay.MediaUnitID);

			//Step 2: File.Delete the rest of the files
			if (deleteDuplcates == true)
				foreach (MediaPlayerInfoUserControl mib in duplicatesToDeleteCtrls)
					if (File.Exists(mib.PathFileName))
						mib.DeleteFS(mib.PathFileName);

			//Step 3: Reload the list 
			int idx = lbxMediaUnits.SelectedIndex;
			onFindMatches(null, null);
			lbxMediaUnits.SelectedIndex = idx;

			tbxFilterAnd.Focus();
		}

		MediaPlayerInfoUserControl getMediaPlayerInfoUserControl(object lbxItem)
		{
			//tu: Getting the currently selected  ListBoxItem  (ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/wpf_conceptual/html/bfcd564e-5e9e-451e-8641-a9b5c3cfac90.htm)
			//!!!: the ListBox must have IsSynchronizedWithCurrentItem set to True for this to work
			var listBoxItem = (ListBoxItem)(lbxMediaUnits.ItemContainerGenerator.ContainerFromItem(lbxItem)); //tu: ListBoxItem from bound data
			if (listBoxItem == null)
				return null;

			ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(listBoxItem);
			DataTemplate myDataTemplate = contentPresenter.ContentTemplate;
			MediaPlayerInfoUserControl mpi = (MediaPlayerInfoUserControl)myDataTemplate.FindName("mpi", contentPresenter);

			return mpi;
		}
		private void setVolume(object lbxItem, double v)
		{
			// Getting the currently selected  ListBoxItem  (ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/wpf_conceptual/html/bfcd564e-5e9e-451e-8641-a9b5c3cfac90.htm)
			// Note that the ListBox must have IsSynchronizedWithCurrentItem set to True for this to work
			ListBoxItem listBoxItem = (ListBoxItem)(lbxMediaUnits.ItemContainerGenerator.ContainerFromItem(lbxItem));

			if (listBoxItem == null)
				return;

			ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(listBoxItem);
			DataTemplate myDataTemplate = contentPresenter.ContentTemplate;
			MediaPlayerInfoUserControl mpi = (MediaPlayerInfoUserControl)myDataTemplate.FindName("mpi", contentPresenter);
			Slider sldVolume = (Slider)myDataTemplate.FindName("sldVolume", contentPresenter);
			mpi.Volume = v == 0 ? 0 : sldVolume == null ? 1 : sldVolume.Value;
		}
		private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is childItem)
					return (childItem)child;
				else
				{
					childItem childOfChild = FindVisualChild<childItem>(child);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			tbxFilterAnd.Focus();
			tbxFilterAnd.Text = DDJ.AudioCompare.Lib.Properties.Settings.Default.LastFilter;
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length == 3)
			{
				tbxFilterAnd.Text = args[1] + "|" + args[2];//must not be duplicates sharing the same crc! (Feb2009)
			}

			tbTitle.Text = Title;

			((System.Windows.Data.CollectionViewSource)(FindResource("vwDuplicateViewSource"))).Source = _mediaInfoDbSrc.vwDuplicates;
		}
		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			DDJ.AudioCompare.Lib.Properties.Settings.Default.LastFilter = tbxFilterAnd.Text;
			DDJ.AudioCompare.Lib.Properties.Settings.Default.Save();
		}
		private void lbxMediaUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			foreach (object lbxItem in lbxMediaUnits.Items) setVolume(lbxItem, 0);

			setVolume(lbxMediaUnits.SelectedItem, 1);

			br.IsEnabled = bd.IsEnabled = cmdFixDb2.IsEnabled = lbxMediaUnits.SelectedItems.Count > 0;
		}
		private void cmdCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		async void onFindMatches(object sender, TextChangedEventArgs e) { if (string.IsNullOrEmpty(tbxFilterAnd.Text) || tbxFilterAnd.Text.Length < 5)	return; await findLoadMatches(); }
		async void onFindMatches(object sender, RoutedEventArgs e) { await findLoadMatches(); }
		async Task findLoadMatches()
		{
			lbxMediaUnits.ItemsSource = _mediaInfoDbSrc.GetMatches(tbxFilterAnd.Text);
			await Task.Delay(200);
			var sw2 = Stopwatch.StartNew();
			_mes.Clear();
			foreach (var item in lbxMediaUnits.Items) _mes.Add(getMediaElement(item));
			Debug.WriteLine(">> {0} mes in {1} ticks ", _mes.Count, sw2.ElapsedTicks);

			chkDeleteDuplcates.IsChecked = false;//reset to false to save from accidental deletion of non-duplicate matches.
		}
		void movePos(int sec)
		{
			foreach (var item in lbxMediaUnits.Items)
			{
				var p = getMediaPlayerInfoUserControl(item).me1;
				p.Position = p.Position.Add(TimeSpan.FromSeconds(sec));
			}
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{

		}

		private void cmdDuplcs_Click(object sender, RoutedEventArgs e)
		{
			//?? new DuplicateList().ShowDialog();
		}
		private void cmdFixDb2_Click(object sender, RoutedEventArgs e)
		{
			fix(true);
		}

		private void OnUseThisFile(object sender, RoutedEventArgs e)
		{
			tbxFilterAnd.Text = vwDuplicateComboBox.SelectedValue.ToString();
		}


		private void Move60secLef2(object sender, RoutedEventArgs e) { foreach (var item in lbxMediaUnits.Items) { getMediaPlayerInfoUserControl(item).me1.Position = new TimeSpan(0); } }
		private void Move60secLeft(object sender, RoutedEventArgs e) { movePos(-60); }
		private void Move10secLeft(object sender, RoutedEventArgs e) { movePos(-10); }
		private void MoveCenter(object sender, RoutedEventArgs e)
		{
			foreach (var item in lbxMediaUnits.Items)
			{
				var p = getMediaPlayerInfoUserControl(item).me1;
				p.Position = p.NaturalDuration.HasTimeSpan ? TimeSpan.FromSeconds(p.NaturalDuration.TimeSpan.TotalSeconds / 2) : new TimeSpan(0);
			}
		}
		private void Move10secRight(object sender, RoutedEventArgs e) { movePos(10); }
		private void Move60secRight(object sender, RoutedEventArgs e) { movePos(60); }
		private void Move60secRigh2(object sender, RoutedEventArgs e)
		{
			foreach (var item in lbxMediaUnits.Items)
			{
				var p = getMediaPlayerInfoUserControl(item).me1;
				p.Position = p.NaturalDuration.HasTimeSpan ? TimeSpan.FromSeconds(p.NaturalDuration.TimeSpan.TotalSeconds - 15) : new TimeSpan(0);
			}
		}
		private void onPlayChecked(object sender, RoutedEventArgs e) { _mes.ForEach(r => r.Play()); }
		private void onPlayUnchekd(object sender, RoutedEventArgs e) { _mes.ForEach(r => r.Pause()); }

		private void Button_Rename_Click(object sender, RoutedEventArgs e)
		{
		}
		private void Button_Delete_Click(object sender, RoutedEventArgs e)
		{
		}

		MediaElement getMediaElement(object item)
		{
			var p = getMediaPlayerInfoUserControl(item).me1;
			if (p.LoadedBehavior != MediaState.Manual)
				p.LoadedBehavior = MediaState.Manual;
			return p;
		}

	}
}
