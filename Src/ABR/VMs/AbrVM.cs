using MVVM.Common;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace ABR.VMs
{
    public partial class AbrVM : ViewModelBase
    {
        //Obsolete: use <lazy> instead static AbrVM _AbrVM; public static AbrVM GetDefault() { lock (typeof(AbrVM)) { if (_AbrVM == null) _AbrVM = new AbrVM(); } return _AbrVM; }
        CoreDispatcher Dispatcher;
        DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        int _c = 0, _i = 0, _instCnt = 0, currentItemIndex = 0;

        public AbrVM()
        {
            if (_instCnt > 0) throw new Exception("Uh crap _)(*&^%$#@!~!@#$%^&*()_");

            _instCnt++;

            Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher; // m_rootPage = MainPage.Current;

            sp_Vm = new MediaPlayer();
            mp_Vm = new MediaPlayer();
            mp_Vm.MediaEnded += onMediaEnded;
            mp_Vm.MediaOpened += onMediaOpened;
            mp_Vm.CurrentStateChanged += onStateChd;
            mp_Vm.MediaPlayerRateChanged += onRateChgd;
            //mp_Vm.PlaybackSession.PositionChanged += (s, args) => Debug.Write($"|{s.Position:h\\:mm\\:ss\\.f} "); // ps.pc:> {s.Position:h\\:mm\\:ss\\.f}"); //  tbInfo += $"\r\n ps.pc: {s.Position}";

            SetupSystemMediaTransportControls();

            _timer.Tick += (s, e) =>
            {
                InpcTestFody.Perc = InpcTestImpl.Perc = InpcTestBase.Perc = InpcTestNone.Perc = ++_i;
                if (mp_Vm.PlaybackSession.NaturalDuration.TotalDays != 0) PageCrnt = (uint)(PagesTtl * mp_Vm.PlaybackSession.Position.TotalDays / mp_Vm.PlaybackSession.NaturalDuration.TotalDays);
                if (SlctMru == null) return;

                if (mp_Vm.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                    if (SlctMru.PlayPosn < mp_Vm.PlaybackSession.Position)                // forward only.
                        SlctMru.PlayPosn = mp_Vm.PlaybackSession.Position;

                if ((++_c) % 10 == 0) updateSaveSettings("t");
                if ((_c) % 10 == 0) CpuUse = cpuLoadReport();
            };
            _timer.Start();

            loadMruLstFromSettings();
            //loadPlyLstFromSettings();

            async_from_ctor_wow();
        }

        async void async_from_ctor_wow()
        {
            for (int i = 0; i < 25 && SlctMru == null; i++)
            {
                SlctMru = await existingTopMru();
                if (SlctMru != null)
                    return;
            }

            //todo: auto-play 1st file from the media lib.

            var msg = $"Apparently, MRU list is empty or none of the {MruLst.Count} files from MRU exist on this PC.\r\n\nOpen/Pick one from the libraries.";
            await Speak(msg);
            await new MessageDialog(msg, "Files do not exist").ShowAsync();
        }

        MediaPlayerElement mpeVm; public MediaPlayerElement MpeVm { get { return mpeVm; } set { mpeVm = value; } }
        MediaPlayer sp_Vm, mp_Vm; public MediaPlayer Mp_Vm { get { return mp_Vm; } }    //?3? AppState _state;
    }
}