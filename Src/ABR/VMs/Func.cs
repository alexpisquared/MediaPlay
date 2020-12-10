using ApxCmn;
using AsLink;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VpxCmn.Model;
using Windows.Storage;
using Windows.System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
namespace ABR.VMs
{
    public partial class AbrVM
    {
        DateTimeOffset _prevt = DateTimeOffset.MinValue;
        TimeSpan _prevu;

        //async void loadPlyLstFromSettings() { await loadJsonFromStngs(AppSetConst.PLt4Roam, PLsLst); }
        async void loadMruLstFromSettings() { await loadJsonFromStngs(AppSetConst.Mru4Roam, MruLst); }
        async void loadTabList(int tabIdx)
        {
            switch (tabIdx)
            {
                case 1: await markDeleted(MruLst); break;
                case 2: onRemovNonExst(LibLst); await onLoadLib_Task(KnownFolderId.AllAppMods); break; // no sense: case 2: if (LibLst.Count() < 1) await onLoadLib_Task(KnownFolderId.AllAppMods); else await markDeleted(LibLst); break;
                default: break;
            }
        }

        async Task markDeleted(ObservableCollection<MediaInfoDto> libLst)
        {
            foreach (var mid in libLst)
            {
                await mid.FileExists();
            }
        }

        async Task loadJsonFromStngs(string keyFile, ObservableCollection<MediaInfoDto> lst)
        {
            var jsn = await AppSettingsHelper.ReadStr(keyFile);
            if (string.IsNullOrEmpty(jsn))
                await Speak("Zero items in JSON.");
            else
            {
                lst.Clear();
                JsonHelper.FromJson<ObservableCollection<MediaInfoDto>>(jsn).OrderByDescending(r => r.LastUsed).ToList().ForEach(lst.Add);
            }
        }

        void saveMru() { AppSettingsHelper.SaveStr(AppSetConst.Mru4Roam, JsonHelper.ToJson(MruLst)); }
        //void savePlt() { AppSettingsHelper.SaveStr(AppSetConst.PLt4Roam, JsonHelper.ToJson(PLsLst)); }

        async Task<MediaInfoDto> existingTopMru()
        {
            if (!MruLst.Any())
                return null;

            foreach (var mu in MruLst.OrderByDescending(r => r.LastUsed))
            {
                if (await mu.FileExists())
                    return mu;
            }

            return null;
        }
        async Task<MediaInfoDto> nonExistingMru(ObservableCollection<MediaInfoDto> lst)
        {
            if (!lst.Any())
                return null;

            foreach (var mu in lst.OrderByDescending(r => r.LastUsed))
            {
                var exists = await mu.FileExists();
                if (!exists)
                    return mu;
            }

            return null;
        }

        async void onRemovNonExst(ObservableCollection<MediaInfoDto> lst)
        {
            var nonExistg = await nonExistingMru(lst);
            while (nonExistg != null)
            {
                lst.Remove(nonExistg);
                nonExistg = await nonExistingMru(lst);
            }
        }

        async void onClearAllAsk(ObservableCollection<MediaInfoDto> lst)
        {
            var messageDialog = new MessageDialog(content: $"Delete: all {lst.Count} mru data?"/*, title: "Are you sure?"*/);

            messageDialog.Commands.Add(new UICommand("Yes", null, 0));
            messageDialog.Commands.Add(new UICommand("No", null, 1));
            messageDialog.DefaultCommandIndex = 1;  // Set the command that will be invoked by default
            messageDialog.CancelCommandIndex = 1;   // Set the command to be invoked when escape is pressed

            var rv = await messageDialog.ShowAsync();
            if ((int)((UICommand)rv).Id == 0)
                lst.Clear();
        }



        async void onLoadLib_void(KnownFolderId library) { await onLoadLib_Task(library); }
        async Task onLoadLib_Task(KnownFolderId library)
        {
            try
            {
                CanLoadLibs = false;

                if (library == KnownFolderId.AllAppMods)
                {
                    await onLoadLib_Task(KnownFolderId.MusicLibrary);
                    await onLoadLib_Task(KnownFolderId.VideosLibrary);
                    return;
                }

                var max = 1000;
                var sfs = await IsoStorePoc.LoadFromLibFolder(library);

                var ll = sfs.Take(max).OrderBy(r => r.Path).ThenBy(r => r.Name);
                foreach (var sf in ll) await addNewOrFromMru(sf); //async ruins the order by: ll.ToList().ForEach(async sf => await add(sf));

                if (SlctMru != null)
                    SlctLib = selectFromList(SlctMru.PathFile, LibLst);

                ApplicationView.GetForCurrentView().Title = $@"{library}: {LibLst.Count} / {sfs.Count} ";
                TbInfo += $@"\r\n{ApplicationView.GetForCurrentView().Title} ";
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, GetType().FullName); }
            finally { CanLoadLibs = true; }
        }

        async Task addNewOrFromMru(StorageFile sf)
        {
            var sz = (await sf.GetBasicPropertiesAsync()).Size;
            if (sz > 10000) // 500,000 <== a typical pod.anons is 200k ==> anything below 10k is corrupt.
            {
                var fsMid = new MediaInfoDto(sf);
                var liMid = LibLst.FirstOrDefault(r => r.FileOnly.Equals(fsMid.FileOnly, StringComparison.OrdinalIgnoreCase));
                if (liMid != null) // already in the Lib list
                    return;

                var mrMid = MruLst.FirstOrDefault(r => r.FileOnly.Equals(fsMid.FileOnly, StringComparison.OrdinalIgnoreCase));
                if (mrMid != null)
                {
                    await mrMid.SetThumbnail(sf);
                    LibLst.Add(mrMid);
                }
                else
                {
                    await fsMid.SetThumbnail(sf);
                    LibLst.Add(fsMid);
                }
            }
            else
                Debug.WriteLine($"{sf.DisplayName}\t Too tiny to add: {(sz * .001):N0} kb ");
        }

        async void updateSaveSettings(string s)
        {
            try
            {
                if (SlctMru == null) return;

                try
                {
                    if (mp_Vm.PlaybackSession.NaturalDuration == TimeSpan.Zero || (
                      SlctMru.PlayPosn.TotalSeconds >= mp_Vm.PlaybackSession.Position.TotalSeconds &&
                      SlctMru.PlayLeng == mp_Vm.PlaybackSession.NaturalDuration &&
                      SlctMru.LastPcNm == DevOp.MachineName))
                        return;

                    if (SlctMru.PlayPosn < mp_Vm.PlaybackSession.Position)
                        SlctMru.PlayPosn = mp_Vm.PlaybackSession.Position;
                    SlctMru.PlayLeng = mp_Vm.PlaybackSession.NaturalDuration;
                    SlctMru.LastPcNm = DevOp.MachineName;
                    SlctMru.LastUsed = DateTime.Now;

                    AppSettingsHelper.SaveStr(AppSetConst.Mru4Roam, JsonHelper.ToJson(MruLst));
                    AppSettingsHelper.SaveVal(AppSetConst.PagesTtl, PagesTtl);
                    AppSettingsHelper.SaveVal(AppSetConst.PlayRate, mp_Vm.PlaybackSession.PlaybackRate);

                    if (Dispatcher.HasThreadAccess) TbInfo += $"{s}"; else await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => TbInfo += $"{s}°");
                }
                catch (COMException ex)
                {
                    Debug.WriteLine($"$#~>{ex.Message}");
                    var min = MruLst.Min(x => x.LastUsed);
                    if (MruLst.Any(r => r.LastUsed == min))
                    {
                        var mru = MruLst.FirstOrDefault(r => r.LastUsed == min);
                        await Speak($"Max limit exceeded. Removing {mru.FileOnly}");
                        MruLst.Remove(mru);
                        updateSaveSettings("x");
                    }
                    else
                        await Speak($"the history has {MruLst.Count} files. i.e.: nothing to remove.");
                }
                catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, GetType().FullName); }
            }
            finally { Debug.WriteLine($"{s}·"); }
        }

        void logEx(Exception ex, string note)
        {
            TbInfo += $"\r\nExn: {ex.Message}\r\n{note}";
            if (Debugger.IsAttached) Debugger.Break();
        }

        async Task popEx(Exception ex, string note = null, [CallerMemberName] string callerName = null)
        {
            TbInfo += $"\r\nExn: {ex.Message}\r\n{note} - {callerName}";
            await new MessageDialog($"{ex.Message}\r\n\n", $"Exception - {note} - {callerName}").ShowAsync();
        }

        public void RemoveFromMruOnly(MediaInfoDto mid) { MruLst.Remove(mid); }
        public async void ReThumbFile(MediaInfoDto mid) { await Speak("Not implemented."); }
        public async void DeleteMedia(MediaInfoDto mid) { await onDeleteDialog(mid); }


        async void addUpdateSave_Mru(MediaInfoDto mid)
        {
            try
            {
                if (!_mrulst.Any(r => r.FileOnly.Equals(mid.FileOnly, StringComparison.OrdinalIgnoreCase)))
                {
                    _mrulst.Add(mid);
                }
                else
                {
                    var mru = _mrulst.FirstOrDefault(r => r.FileOnly.Equals(mid.FileOnly, StringComparison.OrdinalIgnoreCase));
                    if (mru.LastPcNm != DevOp.MachineName)
                        mru.LastPcNm = DevOp.MachineName;
                    else
                        return;
                }

                saveMru();
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, GetType().FullName); }
        }
        async void genPlayListFromFile_void(MediaInfoDto mid) => await genPlayListFromFile(mid);
        async Task<List<MediaInfoDto>> genPlayListFromFile(MediaInfoDto mid)
        {
            var lst = new List<MediaInfoDto>();
            try
            {
                foreach (var file in Directory.EnumerateFiles(mid.PathOnly, "*.*", SearchOption.TopDirectoryOnly).ToList().OrderBy(r => r))
                {
                    var sf = await StorageFile.GetFileFromPathAsync(file); //sep13
                    var mid2 = new MediaInfoDto(sf);                       //sep13
                    await mid2.SetThumbnail(sf);                           //sep13
                    lst.Add(mid2); //org: PLsLst.Add(new MediaInfoDto(await StorageFile.GetFileFromPathAsync(file)));
                }

                SlctLib = lst.FirstOrDefault(r => r.FileOnly.Equals(mid.FileOnly, StringComparison.OrdinalIgnoreCase));

                //savePlt();
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, GetType().FullName); }

            return lst;
        }

        string cpuLoadReport()
        {
            var rv = "...";
            var pdi = ProcessDiagnosticInfo.GetForCurrentProcess();
            var cpu = pdi.CpuUsage.GetReport().UserTime;
            var now = DateTimeOffset.Now;
            var pst = now - pdi.ProcessStartTime;
            if (pst == TimeSpan.Zero) return "now == prcs start";

            if (_prevt > DateTimeOffset.MinValue)
            {
                var t = now - _prevt;
                var u = cpu - _prevu;
                if (t.TotalMilliseconds > 0)
                    rv = $"{(100d * u.TotalMilliseconds / t.TotalMilliseconds),6:N0} / {(100d * cpu.TotalMilliseconds / pst.TotalMilliseconds):N0} ";
            }

            _prevt = now;
            _prevu = cpu;

            return rv;
        }
    }
}
