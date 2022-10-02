using MVVM.Common;
using System.Collections.ObjectModel;

namespace VPC.Models
{
  public class VPModel : BindableBase//, IComparable<Split>
	{
		MediaUnit _crntMu; public MediaUnit CrntMU { get => _crntMu; set => Set(ref _crntMu, value); }

    internal static string MovUpDir(string loadedFile) => new FolderViewModel { CurFile = loadedFile }.GetUpDr;
    internal string MovePrev(string loadedFile)
		{
			var mus = getFileList(loadedFile);

			int curIdx = -1, i = 0; foreach (var mu in mus) { if (mu.PathFileCur == loadedFile) break; i++; }
			curIdx = i;     //??no visible results: mediaUnitsDataGrid.Items.MoveCurrentToPosition(i);

			if (curIdx < 1) return null;

			loadedFile = mus[curIdx - 1].PathFileCur;

			return loadedFile;
		}
		internal string MoveNext(string loadedFile)
		{
			var mus = getFileList(loadedFile);

			var i = 0; foreach (var mu in mus) { if (mu.PathFileCur == loadedFile) break; i++; }
			var curIdx = i;     //??no visible results: mediaUnitsDataGrid.Items.MoveCurrentToPosition(i);

			if (curIdx >= mus.Count - 1) 
				curIdx = -1; //go to the first item

			return mus[curIdx + 1].PathFileCur;
		}

		static ObservableCollection<MediaUnit> getFileList(string loadedFile)
		{
			var fvm = new FolderViewModel { CurFile = loadedFile };
			fvm.LoadDirFromFile(loadedFile, false);
			var mus = fvm.MediaUnits;
			return mus;
		}

		internal void ShowHelp()		{		}
		internal void MoveLeft()		{		}
		internal void MoveRght()		{		}
		internal void Highesst()		{		}
	}
}