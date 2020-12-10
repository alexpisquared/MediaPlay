using DDJ.DB.Models;
using DDJ.DB.Old.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDJ.DataConsoleLoader
{
	internal class FileSysProcessor
	{
		DateTime _now = DateTime.Now;
		DDJContext _dbOld = null;//new DDJContext();
		DdjEf4DBContext _dbNew = new DdjEf4DBContext();
		Stopwatch sw = Stopwatch.StartNew();

		public int FsToDb()
		{
			Console.WriteLine("{0:mm\\:ss} - Starting... \a", sw.Elapsed);

		agn:
			try
			{
				if (_dbOld != null)				_dbOld.AudioPieces.Load();
				_dbNew.MediaUnits.Load();
			}
			catch (Exception ex) { Console.WriteLine("{0:mm\\:ss} - connection problem: {1}", sw.Elapsed, ex.Message); goto agn; }

			Console.Clear();
			Console.WriteLine("{0:mm\\:ss} - Loading FS files", sw.Elapsed);

			foreach (var ext in new string[] { "*.wav", "*.ape", "*.flac", "*.ogg", "*.wma", "*.mp3" })
			{
				sw = Stopwatch.StartNew();
				int i = 0;
				var fls = Directory.GetFiles(@"C:\1\M", ext, SearchOption.AllDirectories);  //M:\
				Console.WriteLine("{0:mm\\:ss} - Loading of {1} {2} files is done", sw.Elapsed, fls.Count(), ext);

				sw = Stopwatch.StartNew();
				foreach (var file in fls)
					chkAddFile(file, ++i, fls.Count());
			}

			Console.WriteLine("{0:mm\\:ss} - B) rows saved: {1} \a", sw.Elapsed, TrySave(_dbNew));

			return 0;
		}

		private void chkAddFile(string file, int i, int ttl)
		{
			var fps = i / (sw.Elapsed.TotalSeconds + .000001);
			var secleft = (ttl - i) / fps;
			Console.WriteLine("{0:mm\\:ss} - {1,6}/{2} ==> {3:dd HH:mm} - {4} ", sw.Elapsed, i, ttl, DateTime.Now.AddSeconds(secleft), file);
			try
			{
				var rNew = _dbNew.MediaUnits.FirstOrDefault(r => string.Compare(r.PathFileExtOrg, file, true) == 0);
				if (rNew == null)
				{
					rNew = _dbNew.MediaUnits.Add(new DB.Models.MediaUnit
						{
							AddedAt = _now,
							FileLength = new FileInfo(file).Length,
							DurationSec = 0,
							FileHashMD5 = 0,
							FileHashQck = 0,
							FileName = Path.GetFileName(file),
							PathName = Path.GetDirectoryName(file),
							PathFileExtOrg = file,
							GenreID = 1
						});

					Debug.WriteLine("rows saved: {0}", TrySave(_dbNew));
				}


				if (_dbOld == null) return;
				
				var rOld = _dbOld.AudioPieces.FirstOrDefault(r => string.Compare(r.PathFileName, file, true) == 0);
				if (rOld == null) return;

				rNew.FileHashMD5 = rOld.File8bSignature;

				rNew.Notes = rOld.Nootes;

				rNew.GenreID =
					rOld.nGenre == 0 ? 1 :
					rOld.nGenre == 1 ? 6 :
					rOld.nGenre == 2 ? 5 :
					rOld.nGenre == 4 ? 4 :
					rOld.nGenre == 8 ? 3 :
					rOld.nGenre == 16 ? 6 :
					1;

				if (rOld.AddedAt.HasValue) rNew.AddedAt = rOld.AddedAt.Value;
				else if (rOld.LastEditDate.HasValue) rNew.AddedAt = rOld.LastEditDate.Value;
				else if (rOld.CreationDate.HasValue) rNew.AddedAt = rOld.CreationDate.Value;

				foreach (var aud in rOld.PrevTimes) _dbNew.MuAuditions.Add(new MuAudition
				{
					MediaUnitID = rNew.ID,
					DoneAt = aud.DateTimePlayed.HasValue ? aud.DateTimePlayed.Value : aud.CreationDate.HasValue ? aud.CreationDate.Value : DateTime.Today.AddYears(-5),
					DoneBy = aud.UsedBy,
					PartyMode = aud.PartyMode.HasValue ? aud.PartyMode.Value : false
				});

				foreach (var aud in rOld.LikedHateds) _dbNew.MuRateHists.Add(new MuRateHist
				{
					MediaUnitID = rNew.ID,
					DoneAt =
					aud.LikedAt.HasValue ? aud.LikedAt.Value :
					aud.HatedAt.HasValue ? aud.HatedAt.Value :
					aud.LastEditDate.HasValue ? aud.LastEditDate.Value :
					aud.CreationDate.HasValue ? aud.CreationDate.Value :
					DateTime.Today.AddYears(-5),
					DoneBy = string.IsNullOrEmpty(aud.EvaluatedBy) ? Environment.UserName : aud.EvaluatedBy,
					LikeUnit = aud.LikedAt.HasValue ? +1 : aud.HatedAt.HasValue ? -1 : 1
				});

			}
			catch (Exception ex) { Console.WriteLine("{0:mm\\:ss} - problem: {1}", sw.Elapsed, ex.Message); }
		}


		public int TrySave(DbContext db)
		{
		retry:
			try
			{
				int rowsSaved = db.SaveChanges();
				Debug.WriteLine(":>{0} rows saved.", rowsSaved);
				return rowsSaved;
			}
			catch (DbEntityValidationException ex)
			{
				string e = "";
				foreach (var er in ex.EntityValidationErrors)
					foreach (var ve in er.ValidationErrors)
						e += string.Format("Validation Error:{0}\n", ve.ErrorMessage);

				Console.WriteLine("{0:mm\\:ss} - problem: {1}", sw.Elapsed, ex.Message);
				if (Debugger.IsAttached) Debugger.Break();
			}
			catch (Exception ex)
			{
				Console.WriteLine("{0:mm\\:ss} - problem: {1}", sw.Elapsed, ex.Message);
				if (Debugger.IsAttached) Debugger.Break();
			}

			return -2;
		}

	}
}



//truncate table MuAudition
//truncate table MuBookmark
//truncate table MuRateHist
//delete MediaUnit

