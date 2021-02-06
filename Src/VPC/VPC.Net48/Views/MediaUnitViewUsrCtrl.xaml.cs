using Common.UI.Lib.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using VPC.Models;

namespace VPC.Views
{
	public partial class MediaUnitViewUsrCtrl : UserControl
	{
		public MediaUnitViewUsrCtrl()
		{
			InitializeComponent();

			DataContext = getTestMU();
		}

		private static MediaUnit getTestMU()
		{
			var mu = new MediaUnit()
			{
				AddedAt                             /**/ = DateTime.Now,
				Auditions                           /**/ = new System.Collections.ObjectModel.ObservableCollection<MuAudition>(),
				Bookmarks                           /**/ = new System.Collections.ObjectModel.ObservableCollection<MuBookmark>(),
				DeletedAt                           /**/ = null,
				Duration                            /**/ = TimeSpan.FromMinutes(33.3),
				FileLength                          /**/ = 1234567,
				FileName                            /**/ = @"",
				Genre                               /**/ = new LkuGenre { Name = "Eve", Desc = "Evening" },
				LastPeekAt                          /**/ = DateTime.Now,
				LastPeekPC                          /**/ = "NUC1",
				Notes                               /**/ = "Notes",
				PassedQA                            /**/ = false,
				PathFileCur                         /**/ = "PathFileCur",
				PathFileOrg                         /**/ = "PathFileOrg",
				PathName                            /**/ = "PathName",
				PositionSec                         /**/ = 123,
				TmpMsg                              /**/ = "tmp msg"
			};

			mu.Bookmarks.Add(new MuBookmark { PositionSec = 123, Note = "test" });
			mu.Bookmarks.Add(new MuBookmark { PositionSec = 123, Note = "test" });
			mu.Bookmarks.Add(new MuBookmark { PositionSec = 123, Note = "test" });
			mu.Auditions.Add(new MuAudition { DoneAt = DateTime.Now, DoneBy = "tester" });
			mu.Auditions.Add(new MuAudition { DoneAt = DateTime.Now, DoneBy = "tester" });
			return mu;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{

			// Do not load your data at design time.
			// if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
			// {
			// 	//Load your data here and assign the result to the CollectionViewSource.
			// 	System.Windows.Data.CollectionViewSource myCollectionViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["Resource Key for CollectionViewSource"];
			// 	myCollectionViewSource.Source = your data
			// }
		}
	}
}
