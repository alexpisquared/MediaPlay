using System.Collections.ObjectModel;
using VpxCmn.Model;
using Windows.UI.Xaml.Controls;

namespace ABR.VMs
{
    public partial class AbrVM
  {
    ObservableCollection<MediaInfoDto> _mrulst = new ObservableCollection<MediaInfoDto>(); public ObservableCollection<MediaInfoDto> MruLst { get => _mrulst; /*set => mrulst = value; */}
    ObservableCollection<MediaInfoDto> _liblst = new ObservableCollection<MediaInfoDto>(); public ObservableCollection<MediaInfoDto> LibLst { get => _liblst; /*set => liblst = value; */}
    //OervableCollection<MediaInfoDto> _plylst = new ObservableCollection<MediaInfoDto>(); public ObservableCollection<MediaInfoDto> PLsLst { get => _plylst; /*set => plylst = value; */}

    string _CpuUse;             /**/ public string CpuUse { get => _CpuUse; set => Set(ref _CpuUse, value); }
    string _tbInfo;             /**/ public string TbInfo { get => _tbInfo; set => Set(ref _tbInfo, value); }
    bool canRmvDlt = true;      /**/ public bool CanElimi { get => canRmvDlt; set => Set(ref canRmvDlt, value); }
    bool delOnEnd = true;       /**/ public bool DelOnEnd { get => delOnEnd; set => Set(ref delOnEnd, value); }
    uint _PageCrnt = 050;       /**/ public uint PageCrnt { get => _PageCrnt; set => Set(ref _PageCrnt, value); }
    uint _PagesTtl = 100;       /**/ public uint PagesTtl { get => _PagesTtl; set => Set(ref _PagesTtl, value); }
    double _PlayRate = 1;       /**/ public double PlayRate { get => _PlayRate; set => Set(ref _PlayRate, value); }
    string _PlyPsCap;           /**/ public string PlyPsCap { get => _PlyPsCap; set => Set(ref _PlyPsCap, value); }
    bool canLoadMusLib = true;  /**/ public bool CanLoadLibs { get => canLoadMusLib; set => Set(ref canLoadMusLib, value); }
    int _selPivotIdx;           /**/ public int SelectedTabIndex { get => _selPivotIdx; set { if (Set(ref _selPivotIdx, value)) loadTabList(value); } }
    MediaInfoDto _SlctLib;      /**/ public MediaInfoDto SlctLib { get => _SlctLib; set { if (Set(ref _SlctLib, value) && value != null) { addUpdateSave_Mru(value); /*SlctPLs = selectFromList(value.PathFile, PLsLst); genPlayListFromFile_void(value);*/ } } }
    MediaInfoDto _SlctMru;      /**/ public MediaInfoDto SlctMru
    {
      get => _SlctMru;
      set
      {
        if (_SlctMru != null && value != null && !Equals(_SlctMru, value)) { }// if both non-null - do what?

        if ((CanElimi = (Set(ref _SlctMru, value) && value != null)))
        {
          startPlayingCurSelMid_void(value);
          SlctLib = selectFromList(value.PathFile, LibLst);
          //SlctPLs = selectFromList(value.PathFile, PLsLst);
        }
      }
    }
    SymbolIcon _PlPsIcon = new SymbolIcon(Symbol.Play); public SymbolIcon PlPsIcon { get => _PlPsIcon; set => Set(ref _PlPsIcon, value); }
  }
}
