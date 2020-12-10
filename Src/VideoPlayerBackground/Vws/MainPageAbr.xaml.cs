using ApxCmn;
using AsLink;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VpxCmn.Model;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
namespace VideoPlayerBackground
{
    public sealed partial class MainPageAbr : Page
    {
        public MainPageAbr()
        {
            this.InitializeComponent();
            DataContext = this;

            pg2.MP = this;
            pg3.MP = this;
            pg4.MP = this;
            pg5.MP = this;

            Application.Current.Suspending += onAppSuspendg;
            Application.Current.Resuming += onAppResuming;

            mp_Xm = new MediaPlayer();
            _sp = new MediaPlayer();
            mp_Xm.CurrentStateChanged += onStateChd;
            mp_Xm.MediaPlayerRateChanged += onRateChgd;
            mp_Xm.PlaybackSession.PositionChanged += (s, args) => tbDbg3.Text = tbInfo.Text += $"\r\ntc.pc: {args}"; //_mp.TimelineController.PositionChanged += (s, args) => tbInf2.Text = tbInfo.Text += $"\r\ntc.pc: {args}";

            _timer.Tick += (sender, e) =>
            {
                if (mp_Xm.PlaybackSession.NaturalDuration.TotalDays != 0) PageCrnt = Math.Round(PagesTtl * mp_Xm.PlaybackSession.Position.TotalDays / mp_Xm.PlaybackSession.NaturalDuration.TotalDays, 1);
                if ((++_c) % 5 == 0) updateSaveSettings();
            };
            _timer.Start();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e) // OnNavigatedFrom will NEVER fire as this sample only includes one page
        {
            base.OnNavigatedTo(e);
            tbDbg3.Text = tbInfo.Text = $"·nt";

            ReadFromSettingsMRU();
            Cur = await existingTopMru();
            if (Cur != null)
                await loadPlayExistingSF(await StorageFile.GetFileFromPathAsync(Cur.PathFile));
            else
            {
                var msg = $"Apparently, none of the {MruLst.Count} files from MRU exist on this PC.\r\n\nOpen/Pick one from the libraries.";
                await Speak(msg);
                await new MessageDialog(msg, "File does not exist").ShowAsync();
            }
        }
        void onAppSuspendg(object sender, SuspendingEventArgs e)
        {
            if (Frame.CurrentSourcePageType == typeof(MainPageAbr))       // Handle global application events only if this page is active
            {
                var deferral = e.SuspendingOperation.GetDeferral();

                tbDbg3.Text = tbInfo.Text += "·s";

                deferral.Complete();
            }
        }
        void onAppResuming(object sender, object o)
        {
            if (Frame.CurrentSourcePageType == typeof(MainPageAbr))       // Handle global application events only if this page is active
            {
                tbDbg3.Text = tbInfo.Text += "·r";
                // loadMru(); update cur pos only for now needed.
            }
        }
        async void onRateChgd(MediaPlayer sender, MediaPlayerRateChangedEventArgs e) { await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => sb.Text = $"{mp_Xm.PlaybackSession.PlaybackRate:0.0}x"); }      // Media callbacks use a worker thread so dispatch to UI as needed
        async void onStateChd(MediaPlayer sender, object args) { await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { tbDbg3.Text = tbInfo.Text += $"\r\ntc.pc: {mp_Xm.PlaybackSession.PlaybackState}"; updateSaveSettings(); }); }
        async void onOpen(object sender, RoutedEventArgs e) { await pickOpen(); }
        void onDeleteCurSel(object sender, RoutedEventArgs e) { MruLst.Remove(Cur); }
        void onRefreshList(object sender, RoutedEventArgs e) { ReadFromSettingsMRU(); } // fs is the truth, as it is updated every 15 sec at least.
        void onPivotSelChngd(object sender, SelectionChangedEventArgs e)
        {
            switch ((string)((PivotItem)((FrameworkElement)((Pivot)sender).SelectedItem)).Header)
            {
                case "Play": break;
                case "MruLst": ReadFromSettingsMRU(); break;
                case "Dbg": break;
                default: ReadFromSettingsMRU(); break;
            }
        }
        void OnGoPg(object sender, RoutedEventArgs e) { if (PagesTtl != 0 && PageCrnt < PagesTtl) mp_Xm.PlaybackSession.Position = TimeSpan.FromDays(mp_Xm.PlaybackSession.NaturalDuration.TotalDays * PageCrnt / PagesTtl); _timer.Start(); }
        void onJump(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Label)
            {
                case "-5.": jump(-5.00); break;
                case "-1.": jump(-1.00); break;
                case "-.3": jump(-.300); break;
                case "+.3": jump(+.300); break;
                case "+1.": jump(+1.00); break;
                case "+5.": jump(+5.00); break;
                default: break;
            }
        }
        void onFaster(object sender, RoutedEventArgs e) { mp_Xm.PlaybackSession.PlaybackRate += .25; updateSaveSettings(); }
        void onSlower(object sender, RoutedEventArgs e) { mp_Xm.PlaybackSession.PlaybackRate -= .25; updateSaveSettings(); }
        void tbCurPg_GotFocus(object sender, RoutedEventArgs e) { _timer.Stop(); } //void tbCurPg_LostFocus(object sender, RoutedEventArgs e) { _timer.Start(); }
        void onCleadLog(object sender, RoutedEventArgs e) => tbInfo.Text = "";
        async void onSpeakTest(object sender, RoutedEventArgs e) => await Speak("Just kidding!");

        async Task checkIfLocal()
        {
            foreach (var mid in MruLst)
            {
                if (await MidExists(mid))
                {
                    mid.MuExists = "+";
                    mid.PcBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                }
                else
                {
                    mid.MuExists = "-";
                    mid.PcBrush = new SolidColorBrush(Color.FromArgb(255, 0, 128, 128));
                }
            }
        }
        public async void ReadFromSettingsMRU()
        {
            var jsn = AppSettingsHelper.ReadVal(AppSetConst.Mru4Roam);
            if (jsn != null && jsn is string)
            {
                var mru = JsonHelper.FromJson<ObservableCollection<MediaInfoDto>>((string)jsn);

                MruLst.Clear();
                mru.OrderByDescending(r => r.LastUsed).ToList().ForEach(MruLst.Add);
                await checkIfLocal();
            }
        }
        public async Task<bool> MidExists(MediaInfoDto mid)
        {
            bool exists = false;
            try
            {
                var sf = await StorageFile.GetFileFromPathAsync(mid.PathFile);
                if (sf != null)
                    exists = true;
            }
            catch (UnauthorizedAccessException) { return false; }
            catch (FileNotFoundException) { return false; }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, mid.PathFile); }
            finally
            {
                //mid.MuExists
                //mid.PcBrush = exists ?
                //  new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)) :
                //  new SolidColorBrush(Color.FromArgb(255, 0, 128, 128));
            }

            return exists;
        }
        async Task<MediaInfoDto> existingTopMru()
        {
            if (!MruLst.Any())
                return null;

            foreach (var mu in MruLst.OrderByDescending(r => r.LastUsed))
            {
                if (await MidExists(mu))
                    return mu;
            }

            return null;
        }

        public async Task LoadPlay1_pathToStoreFile(string path)
        {
            try { await loadPlayExistingSF(await StorageFile.GetFileFromPathAsync(path)); }
            catch (FileNotFoundException ex) { tbDbg3.Text = tbInfo.Text += $"\r\n{path} - {ex.Message}"; }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, path); }
        }
        async Task loadPlayExistingSF(StorageFile sf)
        {
            if (sf == null) return;

            tbDbg3.Text = tbInfo.Text += $"\r\n{sf.DisplayName}";
            try
            {
                setThumbnailFromSfToImgAndCurMid(sf);

                mp_Xm.Source = MediaSource.CreateFromStream(await sf.OpenAsync(FileAccessMode.Read), sf.ContentType); // 
                mp_Xm.PlaybackSession.PlaybackRate = (double)(AppSettingsHelper.ReadVal(AppSetConst.PlayRate) ?? 1d);
                mp_Xm.PlaybackSession.Position = (TimeSpan)Cur.PlayPosn; // if (AppSettingsHelper.ReadVal(AppSetConst.Position) != null) _mp.PlaybackSession.Position = (TimeSpan)AppSettingsHelper.ReadVal(AppSetConst.Position);
                if (!mp_Xm.AutoPlay)
                    mp_Xm.Play();

                if (AppSettingsHelper.ReadVal(AppSetConst.PagesTtl) != null) PagesTtl = (uint)AppSettingsHelper.ReadVal(AppSetConst.PagesTtl);
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, sf.Path); }
        }

        async void setThumbnailFromSfToImgAndCurMid(StorageFile sf)
        {
            await Cur.SetThumbnail(sf);
            if (Cur.Thumbnail != null)
            {
                img1.Source = Cur.Thumbnail;
                img1.Width = 2 * Cur.Thumbnail.PixelWidth;
                img1.Height = 2 * Cur.Thumbnail.PixelHeight;
            }
        }

        public void Mru_FindAdd_MakeCur(StorageFile sf)
        {
            if (MruLst.Any(r => sf.Name.Equals(r.FileOnly, StringComparison.OrdinalIgnoreCase)))
            {
                Cur = MruLst.First(r => sf.Name.Equals(r.FileOnly, StringComparison.OrdinalIgnoreCase));

                if (!Cur.PathFile.Equals(sf.Path, StringComparison.OrdinalIgnoreCase)) // if from another PC: update path to this one.
                    Cur.PathFile = sf.Path;
            }
            else
            {
                MruLst.Add((Cur = new MediaInfoDto(sf) { LastUsed = DateTime.Now }));
            }
        }
        void mruFindAdd(string fileonly)
        {
            if (MruLst.Any(r => fileonly.Equals(r.FileOnly, StringComparison.OrdinalIgnoreCase)))
            {
                Cur = MruLst.First(r => fileonly.Equals(r.FileOnly, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                //?? _mru.Add((_cur = new MediaInfoDto(fileonly) { LastUsed = DateTime.Now }));
            }
        }

        async void updateSaveSettings()
        {
            try
            {
                if (Cur == null)
                    return;

                try
                {
                    var cur = MruLst.FirstOrDefault(r => r.FileOnly.Equals(Cur.FileOnly, StringComparison.OrdinalIgnoreCase));
                    if (cur == null)
                        return;

                    if (!Cur.Equals(cur))
                        Cur = cur;

                    if (
                      Cur.PlayPosn == mp_Xm.PlaybackSession.Position &&
                      Cur.PlayLeng == mp_Xm.PlaybackSession.NaturalDuration &&
                      Cur.LastPcNm == DevOp.MachineName)
                        return;

                    Cur.PlayPosn = mp_Xm.PlaybackSession.Position;
                    Cur.PlayLeng = mp_Xm.PlaybackSession.NaturalDuration;
                    Cur.LastPcNm = DevOp.MachineName;
                    Cur.LastUsed = DateTime.Now;

                    //AppSettingsHelper.RemoVal(AppSetConst.Mru4Roam);
                    AppSettingsHelper.SaveVal(AppSetConst.Mru4Roam, JsonHelper.ToJson(MruLst));
                    AppSettingsHelper.SaveVal(AppSetConst.PagesTtl, PagesTtl);
                    AppSettingsHelper.SaveVal(AppSetConst.PlayRate, mp_Xm.PlaybackSession.PlaybackRate);

                    tbDbg3.Text = tbInfo.Text += ".";
                }
                catch (COMException ex)
                {
                    Debug.WriteLine($"$#~>{ex.Message}");
                    var min = MruLst.Min(x => x.LastUsed);
                    if (MruLst.Any(r => r.LastUsed == min))
                    {
                        var mru = MruLst.FirstOrDefault(r => r.LastUsed == min);
                        MruLst.Remove(mru);
                        await Speak($"Max limit exceeded. Removing {mru.FileOnly}");
                        updateSaveSettings();
                    }
                    else
                        await Speak($"the history has {MruLst.Count} files. i.e.: nothing to remove.");
                }
                catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, "updateStngs"); }
            }
            finally { Debug.WriteLine("--- UpdtStng "); }
        }
        public async Task popEx(Exception ex, string note = null)
        {
            tbInfo.Text = $"Exn: {ex.Message}\r\n{note}"; tbInfo.Foreground = new SolidColorBrush(Colors.Red);
            await new MessageDialog($"{ex.Message}\r\n\n{note}", "Exception").ShowAsync();
        }
        void jump(double v)
        {
            if (v < 0)
                if (mp_Xm.PlaybackSession.Position.TotalMinutes > -v)
                    mp_Xm.PlaybackSession.Position = mp_Xm.PlaybackSession.Position + TimeSpan.FromMinutes(v);
                else
                    mp_Xm.PlaybackSession.Position = TimeSpan.Zero;
            else
              if (mp_Xm.PlaybackSession.NaturalDuration > mp_Xm.PlaybackSession.Position + TimeSpan.FromMinutes(v))
                mp_Xm.PlaybackSession.Position = mp_Xm.PlaybackSession.Position + TimeSpan.FromMinutes(v);
            else
                Debug.WriteLine("...654");
            updateSaveSettings();
        }
        async Task pickOpen()
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
                foreach (var ext in MediaHelper.AllMediaExtensionsAry) picker.FileTypeFilter.Add(ext);

                var sf = await picker.PickSingleFileAsync();
                if (sf == null) return;

                StorageApplicationPermissions.FutureAccessList.Add(sf); //from http://krzyskowk.postach.io/post/files-in-uwp - var picker1 = new FolderPicker(); var folder = await picker1.PickSingleFolderAsync(); if (folder != null) StorageApplicationPermissions.FutureAccessList.Add(folder);  [//tu: sems to be working ]
                                                                        // ales see  http://codezero.one/Details?d=1613&a=9&f=224&l=0&v=d&t=UWP,-NoGo:-allow-file-access  as a helpful workaround for future.
                ApplicationView.GetForCurrentView().Title = sf.Name;

                Mru_FindAdd_MakeCur(sf);

                await loadPlayExistingSF(sf);
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, "pickOpen"); }
        }
        async void __StartPlayback_demoOfMultiItemPlayListHandling()
        {
            try
            {
                if (playbackStartedPreviously) // If playback was already started once we can just resume playing.
                    mp_Xm.Play();
                else
                {
                    playbackStartedPreviously = true;

                    var currentTrackId = AppSettingsHelper.ReadVal(AppSetConst.TrackId); // If the task was cancelled we would have saved the current track and its position. We will try playback from there.
                    if (currentTrackId == null)
                        mp_Xm.Play();
                    else
                    {
                        //Debug.WriteLine($"**>Getting {currentTrackId}     from:"); _mp.Items.ToList().ForEach(r => Debug.WriteLine($"**>        {GetTrackId(r)}"));

                        //var index = _mp.Items.ToList().FindIndex(item => GetTrackId(item).ToString() == (string)currentTrackId); // Find the index of the item by name

                        var currentTrackPosition = Cur.PlayPosn;
                        if (currentTrackPosition == null)
                        {
                            //Debug.WriteLine("StartPlayback: Switching to track " + index); // Play from start if we dont have position
                            //_mp.MoveTo((uint)index);

                            mp_Xm.Play();
                        }
                        else
                        {
                            // Play from exact position otherwise
                            TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;
                            handler = (MediaPlaybackList list, CurrentMediaPlaybackItemChangedEventArgs args) =>
                            {
                                //if (args.NewItem == _mp.Items[index])
                                //{
                                //  // Unsubscribe because this only had to run once for this item
                                //  _mp.CurrentItemChanged -= handler;

                                // Set position
                                Debug.WriteLine("StartPlayback: Setting Position " + currentTrackPosition);
                                mp_Xm.PlaybackSession.Position = currentTrackPosition;

                                mp_Xm.Play();
                                //}
                            };
                            //_mp.CurrentItemChanged += handler;

                            //Debug.WriteLine("StartPlayback: Switching to track " + index); // Switch to the track which will trigger an item changed event
                            playbackStartedPreviously = false;
                            //_mp.MoveTo((uint)index);
                            playbackStartedPreviously = true;
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, "__Start...Demo"); }
        }
        bool playbackStartedPreviously = false;
        ObservableCollection<MediaInfoDto> mru = new ObservableCollection<MediaInfoDto>(); public ObservableCollection<MediaInfoDto> MruLst { get => mru; set => mru = value; }
        MediaInfoDto cur; public MediaInfoDto Cur { get => cur; set => cur = value; }
        MediaPlayer mp_Xm, _sp;    //?3? AppState _state;
        int _c = 0;
        DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        public MediaPlayerElement MpeXm { get { return mpe_Xm; } }
        public MediaPlayer Mp_Xm { get { return mp_Xm; } }
        SpeechSynthesizer _synth = new SpeechSynthesizer();
        public async Task Speak(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            updateSaveSettings();
            var isPlaying = mp_Xm.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

            try
            {
                var speechSynthesisStream = await _synth.SynthesizeTextToStreamAsync(text); // Create a stream from the text. This will be played using a media element.
                _sp.Source = MediaSource.CreateFromStream(speechSynthesisStream, speechSynthesisStream.ContentType);

                TypedEventHandler<MediaPlayer, object> h = null;
                _sp.MediaEnded += h = (s, a) =>
                 {
                     _sp.MediaEnded -= h;
                     if (isPlaying)
                         mp_Xm.Play();
                 };

                if (isPlaying)
                    mp_Xm.Pause();
                _sp.Play();
            }
            catch (FileNotFoundException ex) /**/ { await new MessageDialog(ex.Message, "Media player components unavailable").ShowAsync(); }   // If media player components are unavailable, (eg, using a N SKU of windows), we won't be able to start media playback. Handle this gracefully
            catch (Exception ex)             /**/ { await new MessageDialog(ex.Message, "Unable to synthesize text").ShowAsync(); }             // If the text is unable to be synthesized, throw an error message to the user.
            finally { }
        }
        public static readonly DependencyProperty PageCrntProperty = DependencyProperty.Register("PageCrnt", typeof(double), typeof(MainPageAbr), new PropertyMetadata(1d)); public double PageCrnt { get { return (double)GetValue(PageCrntProperty); } set { SetValue(PageCrntProperty, value); } }
        public static readonly DependencyProperty PagesTtlProperty = DependencyProperty.Register("PagesTtl", typeof(uint), typeof(MainPageAbr), new PropertyMetadata(100u)); public uint PagesTtl { get { return (uint)GetValue(PagesTtlProperty); } set { SetValue(PagesTtlProperty, value); } }


    }
}
