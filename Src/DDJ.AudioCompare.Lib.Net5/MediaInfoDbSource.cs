using System;
using System.Data.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DDJ.DB.Models;
using AsLink;

namespace AudioCompare
{
    public class MediaInfoDbSource
	{
		DdjEf4DBContext _db = new DdjEf4DBContext();

		public MediaInfoDbSource()
		{
			_db.View_AudioPiecesUsedTimes.Load();
			_db.vwDuplicates.Load();
		}

		public IOrderedEnumerable<vwDuplicate> vwDuplicates { get { return _db.vwDuplicates.Local.OrderBy(r => r.FileName); } } //.ToList<vwDuplicate>().as as ObservableCollection<vwDuplicate>; } }

		public IEnumerable GetMatches(string filter)
		{			

			//ring[] andFilter = filter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			string[] orFilter = filter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

			if (orFilter.Length > 1)
			{
				//!!: OR filter MUST contain single one part at the left and multiple AND filters at the right of the '|' !!!
				string[] andFilter = orFilter[1].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

				var query =
					andFilter.Length == 1 ?
						from ap in _db.View_AudioPiecesUsedTimes.Local
						where ap.PathFileName.ToLower().Contains(orFilter[0].ToLower())
							 || ap.PathFileName.ToLower().Contains(andFilter[0].ToLower())
						orderby ap.FileName
						select new
						{
							ap.MediaUnitID,
							ap.PathFileName,
							ap.GenreID,

							ap.UsedTimesCount
						}
										:
					andFilter.Length == 2 ?
						from ap in _db.View_AudioPiecesUsedTimes.Local
						where ap.PathFileName.ToLower().Contains(orFilter[0].ToLower())
							 || ap.PathFileName.ToLower().Contains(andFilter[0].ToLower())
							 && ap.PathFileName.ToLower().Contains(andFilter[1].ToLower())
						orderby ap.FileName
						select new
						{
							ap.MediaUnitID,
							ap.PathFileName,
							ap.GenreID,

							ap.UsedTimesCount
						}
										:
					andFilter.Length == 3 ?
						from ap in _db.View_AudioPiecesUsedTimes.Local
						where ap.PathFileName.ToLower().Contains(orFilter[0].ToLower())
							 || ap.PathFileName.ToLower().Contains(andFilter[0].ToLower())
							 && ap.PathFileName.ToLower().Contains(andFilter[1].ToLower())
							 && ap.PathFileName.ToLower().Contains(andFilter[2].ToLower())
						orderby ap.FileName
						select new
						{
							ap.MediaUnitID,
							ap.PathFileName,
							ap.GenreID,

							ap.UsedTimesCount
						}
										:
						from ap in _db.View_AudioPiecesUsedTimes.Local
						where ap.PathFileName.ToLower().Contains(orFilter[0].ToLower())
							 || ap.PathFileName.ToLower().Contains(andFilter[0].ToLower())
							 && ap.PathFileName.ToLower().Contains(andFilter[1].ToLower())
							 && ap.PathFileName.ToLower().Contains(andFilter[2].ToLower())
							 && ap.PathFileName.ToLower().Contains(andFilter[3].ToLower())
						orderby ap.FileName
						select new
						{
							ap.MediaUnitID,
							ap.PathFileName,
							ap.GenreID,

							ap.UsedTimesCount
						};

				return query.Take(8);
			}
			else
			{
				string[] andFilter = filter.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

				var query =
						andFilter.Length == 1 ?
							from ap in _db.View_AudioPiecesUsedTimes.Local
							where ap.PathFileName.ToLower().Contains(andFilter[0].ToLower())
							orderby ap.FileName
							select new
							{
								ap.MediaUnitID,
								ap.PathFileName,
								ap.GenreID,

								ap.UsedTimesCount
							}
											:
						andFilter.Length == 2 ?
							from ap in _db.View_AudioPiecesUsedTimes.Local
							where ap.PathFileName.ToLower().Contains(andFilter[0].ToLower())
								 && ap.PathFileName.ToLower().Contains(andFilter[1].ToLower())
							orderby ap.FileName
							select new
							{
								ap.MediaUnitID,
								ap.PathFileName,
								ap.GenreID,

								ap.UsedTimesCount
							}
											:
						andFilter.Length == 3 ?
							from ap in _db.View_AudioPiecesUsedTimes.Local
							where ap.PathFileName.ToLower().Contains(andFilter[0].ToLower())
								 && ap.PathFileName.ToLower().Contains(andFilter[1].ToLower())
								 && ap.PathFileName.ToLower().Contains(andFilter[2].ToLower())
							orderby ap.FileName
							select new
							{
								ap.MediaUnitID,
								ap.PathFileName,
								ap.GenreID,

								ap.UsedTimesCount
							}
											:
							from ap in _db.View_AudioPiecesUsedTimes.Local
							where ap.PathFileName.ToLower().Contains(andFilter[0].ToLower())
								 && ap.PathFileName.ToLower().Contains(andFilter[1].ToLower())
								 && ap.PathFileName.ToLower().Contains(andFilter[2].ToLower())
								 && ap.PathFileName.ToLower().Contains(andFilter[3].ToLower())
							orderby ap.FileName
							select new
							{
								ap.MediaUnitID,
								ap.PathFileName,
								ap.GenreID,

								ap.UsedTimesCount
							};

				return query.Take(8);
			}
		}

		public void SmartCascadingDelete(int idToLeave, int idToStay)
		{
			foreach (MuAudition pt in from t in _db.MuAuditions where t.MediaUnitID == idToLeave select t) pt.MediaUnitID = idToStay;

			foreach (MuRateHist lh in from t in _db.MuRateHists where t.MediaUnitID == idToLeave select t) lh.MediaUnitID = idToStay;

			var muToLeave = _db.MediaUnits.FirstOrDefault(r => r.ID == idToLeave);
			if (muToLeave != null)
			{
				muToLeave.DeletedAt = DateTime.Now;
				muToLeave.Notes += $" deleted in favour of a beter version with ID={idToStay}.";
			}

		}
		public void SmartCascadingDeleteOfAll(List<long> duplicateCrcsToRemove, int idToStay)
		{
			foreach (int idToLeave in duplicateCrcsToRemove)
				SmartCascadingDelete(idToLeave, idToStay);

			int rows = DbSaveMsgBox_OldRestoredInDec2023.CheckAskSave(_db);
		}

		public static void SetNewName(int ID, string fullNewName, DdjEf4DBContext db)
		{
			//DdjEf4DBContext db = new DdjEf4DBContext();

			var query = from ap in db.MediaUnits
									where ap.ID == ID
									select ap;

			foreach (MediaUnit ap in query)
			{
				ap.PathFileExtOrg = fullNewName;
			}

			int rows = DbSaveMsgBox_OldRestoredInDec2023.CheckAskSave(db);
		}
	}
}