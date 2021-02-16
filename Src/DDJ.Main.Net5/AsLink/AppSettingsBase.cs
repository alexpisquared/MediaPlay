using AAV.Sys.Helpers;
using System;
using System.Diagnostics;
using System.Reflection;

namespace AsLink
{
  public enum StorageMode
  {
    IsoProgDt,
    IsoUsrLcl,
    IsoUsrRoa,
    OneDriveU,
    OneDrAlex,
  }

  public partial class AppSettings
  {
    static readonly string _subFolder = $@"Public\AppData\{Assembly.GetExecutingAssembly().GetName().Name}\{Environment.MachineName}.json";
    static readonly string _pathfile = OneDrive.Folder(_subFolder);
    static readonly StorageMode _storMode = StorageMode.IsoProgDt;

    public AppSettings() { }
    public static void Save()
    {
      if (_instance == null) return; //tu: ignore autosaves before fully rehydrated from the [iso] store.

      switch (_storMode)
      {
        default:
        case StorageMode.OneDriveU: JsonFileSerializer.Save(_instance, _pathfile); break;
        case StorageMode.OneDrAlex: JsonFileSerializer.Save(_instance, _pathfile); break;
        case StorageMode.IsoProgDt: JsonIsoFileSerializer.Save(_instance); break;
        case StorageMode.IsoUsrLcl: JsonIsoFileSerializer.Save(_instance, null, IsoConst.ULocA); break;
        case StorageMode.IsoUsrRoa: JsonIsoFileSerializer.Save(_instance, null, IsoConst.URoaA); break;
      }
    }

    #region Singletone
    public static AppSettings Instance
    {
      get
      {
        if (_instance == null)
        {
          lock (_syncRoot)
          {
            if (_instance == null)
            {
              _instance =
              _storMode == StorageMode.IsoProgDt ? JsonIsoFileSerializer.Load<AppSettings>() :
              _storMode == StorageMode.IsoUsrLcl ? JsonIsoFileSerializer.Load<AppSettings>(null, IsoConst.ULocA) :
              _storMode == StorageMode.IsoUsrRoa ? JsonIsoFileSerializer.Load<AppSettings>(null, IsoConst.URoaA) :
              _storMode == StorageMode.OneDriveU ? JsonFileSerializer.Load<AppSettings>(_pathfile) :
              _storMode == StorageMode.OneDrAlex ? JsonFileSerializer.Load<AppSettings>(_pathfile) : JsonIsoFileSerializer.Load<AppSettings>();
            }

            if (_instance == null)
              _instance = new AppSettings();
          }
        }

        return _instance;
      }
    }

    static volatile AppSettings _instance;
    static readonly object _syncRoot = new object(); // multithreading support
    #endregion
  }
}
