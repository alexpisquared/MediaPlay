using DDJ.AudioCompare.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using io = System.IO;

namespace AudioCompare
{
	public partial class MediaPlayerInfoUserControl : UserControl
	{
		List<string> _renameAutoCompleteCustomSource = new List<string>();

		public MediaPlayerInfoUserControl()
		{
			InitializeComponent();

			new DispatcherTimer(TimeSpan.FromSeconds(0.1), DispatcherPriority.Normal, new EventHandler(onTimerTick), Dispatcher.CurrentDispatcher);
			//DataContext = this;
		}

		public double MuPosn { get { return (double)GetValue(MuPosnProperty); } set { SetValue(MuPosnProperty, value); } }		public static readonly DependencyProperty MuPosnProperty = DependencyProperty.Register("MuPosn", typeof(double), typeof(MediaPlayerInfoUserControl), new PropertyMetadata(0.5));
		public double MuDurn { get { return (double)GetValue(MuDurnProperty); } set { SetValue(MuDurnProperty, value); } }		public static readonly DependencyProperty MuDurnProperty = DependencyProperty.Register("MuDurn", typeof(double), typeof(MediaPlayerInfoUserControl), new PropertyMetadata(1.0));

		void onTimerTick(object sender, EventArgs e)
		{
			MuPosn = me1.Position.TotalSeconds;
			MuDurn = me1.NaturalDuration.HasTimeSpan ? me1.NaturalDuration.TimeSpan.TotalSeconds : 100;
		}

		void storeNameForRename(string newName)
		{
			_renameAutoCompleteCustomSource.Add(newName);
			//?_renameToNameOnly = newName;
		}
		bool smartRename(string curName, string newName)
		{
			while (true)
			{
				try
				{
					File.Move(curName, newName);
					return true;
				}
				catch (Exception ex) //AP: 23Nov2009
				{
					System.Diagnostics.Trace.WriteLine(
					    $"{DateTime.Now.ToString("MMM yyyy HH:mm")}  in  {System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}():\n\t{ex.Message}\n{(ex.InnerException == null ? "" : ex.InnerException.Message)}");

					string msg =
					    $"\r\nError in {MethodInfo.GetCurrentMethod().DeclaringType.Name}.{MethodInfo.GetCurrentMethod().Name}():\r\n{ex.Message}\r\n{(ex.InnerException == null ? "" : ex.InnerException.Message)}\r\n";

					if (MessageBox.Show(msg, "Retry ?", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) != MessageBoxResult.OK)
					{
						return false;
					}

					me1.Close();
				}
			}
		}
		void rename()
		{
			var rd = new RenameDialog();
			rd.FileName = System.IO.Path.GetFileNameWithoutExtension(PathFileName);
			var dr = rd.ShowDialog();
			if (dr == true && rd.FileName != System.IO.Path.GetFileNameWithoutExtension(PathFileName))
			{
				string fullNewName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(PathFileName), rd.FileName, System.IO.Path.GetExtension(PathFileName));
				if (!smartRename(PathFileName, fullNewName))
					return;
			}
		}

		public void DeleteFS(string file)
		{
			var ddir = @"C:\1\M\zDuplicates";
			if (!Directory.Exists(ddir)) Directory.CreateDirectory(ddir);
			var dest = io.Path.Combine(ddir, io.Path.GetFileName(file));
			Task.Run(() => Task.Delay(5000)).ContinueWith(_ => File.Move(file, dest));
		}
		public void DeleteLoadedFile_NOT_USED()
		{
			while (true)
			{
				try
				{

					me1.Stop();
					me1.Close();
					for (int i = 0; i < 5; i++)
					{
						File.Delete(PathFileName);
						if (!File.Exists(PathFileName))
						{
							fileOnDiskPanel.Visibility = Visibility.Hidden;
							return;
						}
						Debug.WriteLine(":> failed to delete in sec: " + i.ToString());
						System.Threading.Thread.Sleep(100);
					}
				}
				catch (Exception ex) //AP: 23Nov2009
				{
					System.Diagnostics.Trace.WriteLine(
					    $"{DateTime.Now.ToString("MMM yyyy HH:mm")}  in  {System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}():\n\t{ex.Message}\n{(ex.InnerException == null ? "" : ex.InnerException.Message)}");

					string msg =
					    $"\r\nError in {MethodInfo.GetCurrentMethod().DeclaringType.Name}.{MethodInfo.GetCurrentMethod().Name}():\r\n{ex.Message}\r\n{(ex.InnerException == null ? "" : ex.InnerException.Message)}\r\n";

					if (MessageBox.Show(msg, "Retry ?", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) != MessageBoxResult.OK)
					{
						return;
					}
				}
			}
		}

		public double Volume
		{
			get { return me1.Volume; }
			set { me1.Volume = value; }
		}
		public int MediaUnitID { get { return Convert.ToInt32(tbkMediaUnitID.Text); } }
		public string PathFileName { get { return tbkPathFileName.Text; } }

		void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.F2: rename(); break;
				default: break;
			}
		}
		void Button_Rename_Click(object sender, RoutedEventArgs e)
		{
			rename();
		}
		void onPlayChecked(object sender, RoutedEventArgs e) { me1.Play(); }
		void onPlayUnchekd(object sender, RoutedEventArgs e) { me1.Pause(); }
		void Button_Delete_Click(object sender, RoutedEventArgs e)
		{
			DeleteFS(PathFileName);
		}

		void p_MediaOpened(object sender, RoutedEventArgs e)
		{
			bool fExist = File.Exists(PathFileName);

			fileOnDiskPanel.Visibility = fExist ? Visibility.Visible : Visibility.Hidden;

			tbkDuration.Text = $"{me1.NaturalDuration.TimeSpan:h\\:mm\\:ss} ";
			tbkDuratio2.Text = $"{(fExist ? new FileInfo(PathFileName).Length / (double)(1024 * 1024) : -1):N2}";

			Debug.WriteLine("::>" + tbkDuration.Text);
		}
		void Move60secLeft(object sender, RoutedEventArgs e) { me1.Position = me1.Position.Add(TimeSpan.FromSeconds(-15)); }
		void Move10secLeft(object sender, RoutedEventArgs e) { me1.Position = me1.Position.Add(TimeSpan.FromSeconds(-5)); }
		void MoveMidCenter(object sender, RoutedEventArgs e) { me1.Position = me1.NaturalDuration.HasTimeSpan ? TimeSpan.FromSeconds(me1.NaturalDuration.TimeSpan.TotalSeconds / 2) : new TimeSpan(0); }
		void Move10secRigt(object sender, RoutedEventArgs e) { me1.Position = me1.Position.Add(TimeSpan.FromSeconds(5)); }
		void Move60secRigt(object sender, RoutedEventArgs e) { me1.Position = me1.Position.Add(TimeSpan.FromSeconds(15)); }
	}
}
