using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VpxCmn.Model;
using Windows.Storage;
using Windows.UI.Popups;

namespace ABR.VMs
{
    public partial class AbrVM
    {
        const double _rateStep = .5;

        async void onDoSmth() { await Task.Delay(9); }
        void onItemMenuPoc00(object mid) { if (Debugger.IsAttached) Debugger.Break(); }
        async void onPlayNowThsMid(object oMid)
        {
            switch (oMid) //tu: C# Build 2017
            {
                case null: break;
                case object[] a when a.Length > 0: break;
                case string s when int.TryParse(s, out var i): break;
                case MediaInfoDto mid:
                    addUpdateSave_Mru(mid);
                    SlctMru = mid;
                    await startPlayingCurSelMid_Task(mid); break;
                default: throw new ArgumentException("Not MediaInfoDto");
            }
        }
        async void onSetThumbsMrus() { foreach (var mid in MruLst) { await mid.SetThumbnail(); } }
        async void onSetThumbsLibs() { foreach (var mid in LibLst) { await mid.SetThumbnail(); } }
        void onRefreshFromFS() { CanElimi = false; }
        void onRemoveCurSlct() { MruLst.Remove(SlctMru); CanElimi = false; }
        void onRemoveThisMid(object mid) { MruLst.Remove((MediaInfoDto)mid); CanElimi = false; }
        async void onDeleteCurSlct() => await onDeleteDialog(SlctMru);
        async void onDeleteThisMid(object mid) => await onDeleteDialog((MediaInfoDto)mid);
        async Task onDeleteDialog(MediaInfoDto mid)
        {
            var messageDialog = new MessageDialog($"Delete: {mid.PathFile}", "Are you sure?");

            messageDialog.Commands.Add(new UICommand("Yes", null, 0));
            messageDialog.Commands.Add(new UICommand("No", null, 1));
            messageDialog.DefaultCommandIndex = 1;  // Set the command that will be invoked by default
            messageDialog.CancelCommandIndex = 1;   // Set the command to be invoked when escape is pressed

            var rv = await messageDialog.ShowAsync();
            if ((int)((UICommand)rv).Id != 0)
                return;

            await deleteCurMruMid(mid.PathFile);

            //? onRefreshList(sender, e);
        }
        void onJumpArnd(object min)
        {
            switch (min as string)
            {
                case string s when double.TryParse(s, out var m): jump(m); break;
                default: Debug.WriteLine($"{min} is not double."); break;
            }
        }
        void onGoSpeedX(double x) { jump(); mp_Vm.PlaybackSession.PlaybackRate = PlayRate = x; updateSaveSettings("x"); }
        void onGoSlower() { jump(); mp_Vm.PlaybackSession.PlaybackRate = PlayRate > .4 ? PlayRate -= _rateStep : PlayRate = .10; updateSaveSettings("s"); }
        void onGoFaster() { jump(); mp_Vm.PlaybackSession.PlaybackRate = PlayRate <= 8 ? PlayRate += _rateStep : PlayRate = 8; updateSaveSettings("f"); }
        void onPlayRate() { updateSaveSettings("r"); }
        void onGoToPage() { updateSaveSettings("g"); }
        void onOpenPick() { updateSaveSettings("o"); }

        async Task deleteCurMruMid(string pathFile)
        {
            if (!DelOnEnd)
                return;

            try
            {
                //await Speak($"Deleted."); //await Speak($"Deleting in 15 seconds: {Path.GetFileNameWithoutExtension(pathFile)}.");        await Task.Delay(15000);

                removFromList(pathFile, MruLst);
                //removFromList(pathFile, PLsLst);
                removFromList(pathFile, LibLst);

                var sf = await StorageFile.GetFileFromPathAsync(pathFile);
                await sf.DeleteAsync(StorageDeleteOption.Default);

                CanElimi = false;
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, GetType().FullName, pathFile); }
            finally { updateSaveSettings("d"); }
        }
        static void removFromList(string pathFile, System.Collections.ObjectModel.ObservableCollection<MediaInfoDto> lst)
        {
            var mid = lst.FirstOrDefault(r => r.PathFile.Equals(pathFile, StringComparison.OrdinalIgnoreCase));
            if (mid != null)
                lst.Remove(mid);
        }
        static MediaInfoDto selectFromList(string pathFile, System.Collections.ObjectModel.ObservableCollection<MediaInfoDto> lst) => lst.FirstOrDefault(r => r.PathFile.Equals(pathFile, StringComparison.OrdinalIgnoreCase));
    }
}
