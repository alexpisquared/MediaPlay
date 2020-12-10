using ApxCmn;
using AsLink;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VpxCmn.Model;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ABR.VMs
{
    public partial class AbrVM
    {
        public Image _img1 { get; private set; } = new Image();

        async void onMediaOpened(MediaPlayer s, object args)                   /**/ { await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => mediaOpened()); }                                 // Media callbacks use a worker thread so dispatch to UI as needed
        async void onMediaEnded(MediaPlayer s, object args)                    /**/ { await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => mediaEnded()); }                                  // Media callbacks use a worker thread so dispatch to UI as needed
        async void onRateChgd(MediaPlayer s, MediaPlayerRateChangedEventArgs e)/**/ { await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PlayRate = mp_Vm.PlaybackSession.PlaybackRate); } // Media callbacks use a worker thread so dispatch to UI as needed
        async void onStateChd(MediaPlayer s, object args)                      /**/ { await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => stateChd(s, args)); }

        void stateChd(MediaPlayer s, object args)
        {
            TbInfo += $" {mp_Vm.PlaybackSession.PlaybackState.ToString().Substring(0, 3)} ";
            PlyPsCap = s.PlaybackSession.PlaybackState == MediaPlaybackState.Playing ? "Pause" : "Play";
            PlPsIcon = new SymbolIcon(s.PlaybackSession.PlaybackState == MediaPlaybackState.Playing ? Symbol.Pause : Symbol.Play);
            Debug.WriteLine($"{SlctMru?.NameOnly,-22}\t{s.PlaybackSession.PlaybackState,-9} - {PlyPsCap}");
        }


        async void mediaOpened()
        {
            try
            {
                while (mp_Vm.PlaybackSession.NaturalDuration == TimeSpan.Zero) /**/{ await Task.Delay(44); Debug.WriteLine($"{SlctMru.NameOnly,-22}\tOpened -- No Duration yet. --"); }
                while (mp_Vm.PlaybackSession.CanSeek == false)                 /**/{ await Task.Delay(44); Debug.WriteLine($"{SlctMru.NameOnly,-22}\tOpened -- No Can seek yet. --"); }

                //if (SlctMru.PlayPosn > mp_Vm.PlaybackSession.Position)          await setPosnSafe(SlctMru, "Open");

                if (!mp_Vm.AutoPlay) mp_Vm.Play();
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, GetType().FullName); }
        }
        async void mediaEnded()
        {
            if (SlctMru == null) return;

            Debug.WriteLine($"{SlctMru.NameOnly,-22}\tEnded --");

            var msg = "";
            var file2delete = SlctMru.PathFile;
            var secondsLeft = (mp_Vm.PlaybackSession.NaturalDuration - mp_Vm.PlaybackSession.Position).TotalSeconds;

            mp_Vm.PlaybackSession.Position = TimeSpan.Zero; //aug2017: reset player to the beginning to prevent bleeding the current position into the next piece

            try
            {
                var rv = await trySetNextFromDir(SlctMru);
                msg += rv.Item2;
                if (rv.Item1)
                    return;

                rv = await trySetNextFromMru(SlctMru);
                msg += rv.Item2;
                if (rv.Item1)
                    return;
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, GetType().FullName); }
            finally { await deleteIfSetTo(file2delete, secondsLeft, msg); }
        }

        async Task<Tuple<bool, string>> trySetNextFromDir(MediaInfoDto mid)
        {
            var curDirLst = await genPlayListFromFile(mid);
            if (curDirLst.Count <= 1)
                return Tuple.Create(false, "Nothing to play in the folder/playlist"); //if (Dispatcher.HasThreadAccess) Debug.WriteLine($"<><> Thread is OK <><>"); else await Speak($"WRONG Thread: non-UI! ");

            var next = curDirLst.OrderBy(r => r.FileOnly).FirstOrDefault(r => !r.FileOnly.Equals(SlctMru.FileOnly, StringComparison.OrdinalIgnoreCase));
            if (next == null)
            {
                return Tuple.Create(false, "This is the last of the playlist.");
            }
            else
            {
                TbInfo += ($" \r\n\r\n>>{SlctMru.NameOnly}\r\n>>{next.NameOnly}>>");
                Debug.WriteLine($"\r\n>>{SlctMru.NameOnly}\r\n>>{next.NameOnly}>>");

                await setNext(next);
                return Tuple.Create(true, "");
            }

            ////this works only for deleting listened mode:
            //var nxtFromPLs = PLsLst.FirstOrDefault(r => !r.FileOnly.Equals(mid.FileOnly, StringComparison.OrdinalIgnoreCase));
            //if (nxtFromPLs == null)
            //{
            //  return Tuple.Create(false, "This is the last file in the folder.");
            //}
            //else
            //{
            //  setNext(nxtFromPLs);
            //  return Tuple.Create(true, "");
            //}
        }

        async Task addToMruListIfNotThere(MediaInfoDto nxtPLt)
        {
            if (!MruLst.Any(r => r.FileOnly.Equals(nxtPLt.FileOnly, StringComparison.OrdinalIgnoreCase)))
            {
                await nxtPLt.SetThumbnail(/*sf*/); //sep13
                MruLst.Add(nxtPLt);
            }
        }

        async Task<Tuple<bool, string>> trySetNextFromMru(MediaInfoDto mid)
        {
            if (MruLst.Count <= 1)
                return Tuple.Create(false, "Nothing to play in the MRU list");

            var nxtFromMru = MruLst.FirstOrDefault(r => !r.FileOnly.Equals(mid.FileOnly, StringComparison.OrdinalIgnoreCase));
            if (nxtFromMru == null)
            {
                return Tuple.Create(false, "This is the last file in the MRU list.");
            }
            else
            {
                await setNext(nxtFromMru);
                return Tuple.Create(true, "");
            }
        }
        async Task setNext(MediaInfoDto mid)
        {
            await addToMruListIfNotThere(mid);

            //SlctPLs = PLsLst.FirstOrDefault(r => r.FileOnly.Equals(mid.FileOnly, StringComparison.OrdinalIgnoreCase));
            SlctMru = MruLst.FirstOrDefault(r => r.FileOnly.Equals(mid.FileOnly, StringComparison.OrdinalIgnoreCase));
            SlctLib = LibLst.FirstOrDefault(r => r.FileOnly.Equals(mid.FileOnly, StringComparison.OrdinalIgnoreCase));
        }
        async Task deleteIfSetTo(string dlt, double secsLeft, string msg)
        {
            if (DelOnEnd)
            {
                if (secsLeft > 5)
                    await Speak($"{msg} Deleting suspended since as many as {secsLeft} seconds left to play.");
                else
                {
                    await Speak($"{msg} Deleting in one minute...");
                    await Task.Run(async () => await Task.Delay(60999)).ContinueWith(async _ => await deleteCurMruMid(dlt), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }


        async Task setPosnSafe(MediaInfoDto mid, string msg)
        {
            if (Dispatcher.HasThreadAccess)
                setPosn(mid, msg);
            else
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => setPosn(mid, msg));
        }
        async void setPosn(MediaInfoDto mid, string msg)
        {
            while (mp_Vm.PlaybackSession.NaturalDuration == TimeSpan.Zero) /**/{ await Task.Delay(44); Debug.WriteLine($"{mid.NameOnly,-22}\tSetPos -- No Duration yet. --"); }

            if ((mp_Vm.PlaybackSession.NaturalDuration - mid.PlayPosn).TotalSeconds < 15)  // if practically at the end
            {
                mid.PlayPosn = TimeSpan.Zero;
                await Speak("Rewinded");
            }

            mp_Vm.PlaybackSession.Position = mid.PlayPosn;

            var s = $"{mid.NameOnly,-22}\tmdl:{mid.PlayPosn:h\\:mm\\:ss} plr:{mp_Vm.PlaybackSession.Position:h\\:mm\\:ss} drn:{mp_Vm.PlaybackSession.NaturalDuration:h\\:mm\\:ss}  {msg,-6}\t";
            Debug.WriteLine(s);
            TbInfo += $"\r\n{s}";

            if ((mid.PlayPosn - mp_Vm.PlaybackSession.Position).TotalSeconds > 0)
            {
                var p = $" ==unable to set position==";
                Debug.WriteLine(p);
                TbInfo += p;
                await Speak(p);
            }
        }

        async void startPlayingCurSelMid_void(MediaInfoDto mid) => await startPlayingCurSelMid_Task(mid);
        async Task startPlayingCurSelMid_Task(MediaInfoDto mid)
        {
            try
            {
                if (!await mid.FileExists()) { await Speak($"Removed since does not exist."); MruLst.Remove(mid); return; }

                var sf = await StorageFile.GetFileFromPathAsync(mid.PathFile); if (sf == null) { await Speak($"Unable to GetFileFromPathAsync."); return; }

                mid.MuExists = "+++";
                mid.PcBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

                await setCentralImageToThumbnail(sf);

                Debug.WriteLine($"{mid.NameOnly,-22}\t>>>Starting ");

                mp_Vm.Source = MediaSource.CreateFromStream(await sf.OpenAsync(FileAccessMode.Read), sf.ContentType); // 
                mp_Vm.PlaybackSession.PlaybackRate = (double)(AppSettingsHelper.ReadVal(AppSetConst.PlayRate) ?? 1d);

                await setPosnSafe(mid, "Start");

                if (AppSettingsHelper.ReadVal(AppSetConst.PagesTtl) != null) PagesTtl = (uint)AppSettingsHelper.ReadVal(AppSetConst.PagesTtl);
            }
            catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); else await popEx(ex, GetType().FullName); }
        }

        async Task setCentralImageToThumbnail(StorageFile sf)
        {
            await SlctMru.SetThumbnail(sf);
            if (SlctMru.Thumbnail != null)
            {
                _img1.Source = SlctMru.Thumbnail;
                //_img1.Width = 2 * SlctMru.Thumbnail.PixelWidth;
                //_img1.Height = 2 * SlctMru.Thumbnail.PixelHeight;
            }
        }

        void jump(double min = -.03)
        {
            if (min < 0)
                if (mp_Vm.PlaybackSession.Position.TotalMinutes > -min)
                    mp_Vm.PlaybackSession.Position = mp_Vm.PlaybackSession.Position + TimeSpan.FromMinutes(min);
                else
                    mp_Vm.PlaybackSession.Position = TimeSpan.Zero;
            else if (mp_Vm.PlaybackSession.NaturalDuration > mp_Vm.PlaybackSession.Position + TimeSpan.FromMinutes(min))
                mp_Vm.PlaybackSession.Position = mp_Vm.PlaybackSession.Position + TimeSpan.FromMinutes(min);
            else
                mp_Vm.PlaybackSession.Position = mp_Vm.PlaybackSession.NaturalDuration - TimeSpan.FromSeconds(.1);

            updateSaveSettings("j");
        }


        //    C:\gh\WUS2017\Samples\SystemMediaTransportControls\cs\Scenario1.xaml.cs
        SystemMediaTransportControls systemMediaControls = null;
        bool _isThisPageActive = true;
        //MediaPlaybackItem _mediaPlaybackItem;

        /// <summary>
        /// Invoked from this scenario page's OnNavigatedTo event handler.  Retrieve and initialize the SystemMediaTransportControls object.
        /// </summary>
        void SetupSystemMediaTransportControls()
        {
            // Retrieve the SystemMediaTransportControls object associated with the current app view
            // (ie. window).  There is exactly one instance of the object per view, instantiated by
            // the system the first time GetForCurrentView() is called for the view.  All subsequent 
            // calls to GetForCurrentView() from the same view (eg. from different scenario pages in 
            // this sample) will return the same instance of the object.
            systemMediaControls = SystemMediaTransportControls.GetForCurrentView();

            // This scenario will always start off with no media loaded, so we will start off disabling the 
            // system media transport controls.  Doing so will hide the system UI for media transport controls
            // from being displayed, and will prevent the app from receiving any events such as ButtonPressed 
            // from it, regardless of the current state of event registrations and button enable/disable states.
            // This makes IsEnabled a handy way to turn system media transport controls off and back on, as you 
            // may want to do when the user navigates to and away from certain parts of your app.
            //systemMediaControls.IsEnabled = false;
            systemMediaControls.IsEnabled = true; //Nov 2017 - what if restore to false?

            // To receive notifications for the user pressing media keys (eg. "Stop") on the keyboard, or 
            // clicking/tapping on the equivalent software buttons in the system media transport controls UI, 
            // all of the following needs to be true:
            //     1. Register for ButtonPressed event on the SystemMediaTransportControls object.
            //     2. IsEnabled property must be true to enable SystemMediaTransportControls itself.
            //        [Note: IsEnabled is initialized to true when the system instantiates the
            //         SystemMediaTransportControls object for the current app view.]
            //     3. For each button you want notifications from, set the corresponding property to true to
            //        enable the button.  For example, set IsPlayEnabled to true to enable the "Play" button 
            //        and media key.
            //        [Note: the individual button-enabled properties are initialized to false when the
            //         system instantiates the SystemMediaTransportControls object for the current app view.]
            //
            // Here we'll perform 1, and 3 for the buttons that will always be enabled for this scenario (Play,
            // Pause, Stop).  For 2, we purposely set IsEnabled to false to be consistent with the scenario's 
            // initial state of no media loaded.  Later in the code where we handle the loading of media
            // selected by the user, we will enable SystemMediaTransportControls.
            systemMediaControls.ButtonPressed += systemMediaControls_ButtonPressed;

            //////// Add event handlers to support requests from the system to change our playback state. 
            //////systemMediaControls.PlaybackRateChangeRequested += systemMediaControls_PlaybackRateChangeRequested;
            //////systemMediaControls.AutoRepeatModeChangeRequested += systemMediaControls_AutoRepeatModeChangeRequested;
            //////systemMediaControls.PlaybackPositionChangeRequested += systemMediaControls_PlaybackPositionChangeRequested;

            //////// Subscribe to property changed events to get SoundLevel changes. 
            //////systemMediaControls.PropertyChanged += systemMediaControls_PropertyChanged;

            // Note: one of the prerequisites for an app to be allowed to play audio while in background, 
            // is to enable handling Play and Pause ButtonPressed events from SystemMediaTransportControls.
            systemMediaControls.IsPlayEnabled = true;
            systemMediaControls.IsPauseEnabled = true;
            systemMediaControls.IsStopEnabled = true;
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;

            //Nov 2017:
            systemMediaControls.IsRewindEnabled = true;         // do not show up
            systemMediaControls.IsFastForwardEnabled = true;    // do not show up ==>
            systemMediaControls.IsNextEnabled = true;           //                ==> using these two instead then.
            systemMediaControls.IsPreviousEnabled = true;
        }
        async void systemMediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            // The system media transport control's ButtonPressed event may not fire on the app's UI thread.  XAML controls 
            // (including the MediaPlayerElement control in our page as well as the scenario page itself) typically can only be 
            // safely accessed and manipulated on the UI thread, so here for simplicity, we dispatch our entire event handling 
            // code to execute on the UI thread, as our code here primarily deals with updating the UI and the MediaPlayerElement.
            // 
            // Depending on how exactly you are handling the different button presses (which for your app may include buttons 
            // not used in this sample scenario), you may instead choose to only dispatch certain parts of your app's 
            // event handling code (such as those that interact with XAML) to run on UI thread.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Because the handling code is dispatched asynchronously, it is possible the user may have
                // navigated away from this scenario page to another scenario page by the time we are executing here.
                // Check to ensure the page is still active before proceeding.
                //    if (_isThisPageActive)
                {
                    switch (args.Button)
                    {
                        case SystemMediaTransportControlsButton.Play: mp_Vm.Play(); break;
                        case SystemMediaTransportControlsButton.Pause: mp_Vm.Pause(); break;
                        case SystemMediaTransportControlsButton.Stop: mp_Vm.Pause(); mp_Vm.PlaybackSession.Position = TimeSpan.Zero; break;

                        //case SystemMediaTransportControlsButton.Next: await SetNewMediaItem(currentItemIndex + 1); break;              // range-checking will be performed in SetNewMediaItem()
                        //case SystemMediaTransportControlsButton.Previous: await SetNewMediaItem(currentItemIndex - 1); break;
                        case SystemMediaTransportControlsButton.ChannelDown: await SetNewMediaItem(currentItemIndex - 1); break;

                        case SystemMediaTransportControlsButton.Next:           /**/ mp_Vm.PlaybackSession.Position = mp_Vm.PlaybackSession.Position.Add(TimeSpan.FromSeconds(60)); break;
                        case SystemMediaTransportControlsButton.Previous:       /**/ mp_Vm.PlaybackSession.Position = mp_Vm.PlaybackSession.Position.Subtract(TimeSpan.FromSeconds(20)); break;

                        case SystemMediaTransportControlsButton.FastForward:    /**/ mp_Vm.PlaybackSession.Position = mp_Vm.PlaybackSession.Position.Add(TimeSpan.FromSeconds(60)); break;      // the buttons are not shown
                        case SystemMediaTransportControlsButton.Rewind:         /**/ mp_Vm.PlaybackSession.Position = mp_Vm.PlaybackSession.Position.Subtract(TimeSpan.FromSeconds(20)); break; // the buttons are not shown
                    }
                }
            });
        }
        async Task SetNewMediaItem(int newItemIndex)
        {
            await Speak($"Number {newItemIndex} requested...");
        }
        //  // enable Next button unless we're on last item of the playlist
        //  if (newItemIndex >= PLsLst.Count - 1)
        //  {
        //    systemMediaControls.IsNextEnabled = false;
        //    newItemIndex = PLsLst.Count - 1;
        //  }
        //  else
        //  {
        //    systemMediaControls.IsNextEnabled = true;
        //  }

        //  // enable Previous button unless we're on first item of the playlist
        //  if (newItemIndex <= 0)
        //  {
        //    systemMediaControls.IsPreviousEnabled = false;
        //    newItemIndex = 0;
        //  }
        //  else
        //  {
        //    systemMediaControls.IsPreviousEnabled = true;
        //  }

        //  // note that the Play, Pause and Stop buttons were already enabled via SetupSystemMediaTransportControls() 
        //  // invoked during this scenario page's OnNavigateToHandler()


        //  currentItemIndex = newItemIndex;
        //  StorageFile mediaFile = PLsLst[newItemIndex].SFile;
        //  IRandomAccessStream stream = null;
        //  try
        //  {
        //    stream = await mediaFile.OpenAsync(FileAccessMode.Read);
        //  }
        //  catch (Exception e)
        //  {
        //    if (_isThisPageActive) // User may have navigated away from this scenario page to another scenario page before the async operation completed.
        //    {
        //      // If the file can't be opened, for this sample we will behave similar to the case of
        //      // setting a corrupted/invalid media file stream on the MediaPlayer (which triggers a 
        //      // MediaFailed event).  We abort any ongoing playback by nulling the MediaPlayer's 
        //      // source.  The user must press Next or Previous to move to a different media item, 
        //      // or use the file picker to load a new set of files to play.
        //      mp_Vm.Source = null;

        //      string errorMessage = String.Format(@"Cannot open {0} [""{1}""]. \nPress Next or Previous to continue, or select new files to play.", mediaFile.Name, e.Message.Trim());
        //      //rootPage.NotifyUser(errorMessage, NotifyType.ErrorMessage);
        //    }
        //  }

        //  // User may have navigated away from this scenario page to another scenario page 
        //  // before the async operation completed. Check to make sure page is still active.
        //  //if (!_isThisPageActive)        return;

        //  if (stream != null)
        //  {
        //    // We're about to change the MediaPlayer's source media, so put ourselves into a 
        //    // "changing media" state.  We stay in that state until the new media is playing,
        //    // loaded (if user has currently paused or stopped playback), or failed to load.
        //    // At those points we will call OnChangingMediaEnded().
        //    //
        //    // Note that the SMTC visual state may not update until assigning a source or
        //    // beginning playback. If using a different API than MediaPlayer, such as AudioGraph,
        //    // you will need to begin playing a stream to see SMTC update.
        //    _mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStream(stream, mediaFile.ContentType));
        //    mp_Vm.Source = _mediaPlaybackItem;
        //  }

        //  try
        //  {
        //    // Updates the system UI for media transport controls to display metadata information
        //    // reflecting the file we are playing (eg. track title, album art/video thumbnail, etc.)
        //    // We call this even if the mediaFile can't be opened; in that case the method can still 
        //    // update the system UI to remove any metadata information previously displayed.
        //    await UpdateSystemMediaControlsDisplayAsync(mediaFile);
        //  }
        //  catch //(Exception e)
        //  {
        //    // Check isThisPageActive as user may have navigated away from this scenario page to another scenario page before the async operations completed.
        //    //if (_isThisPageActive)
        //    {
        //      //rootPage.NotifyUser(e.Message, NotifyType.ErrorMessage);
        //    }
        //  }
        //}

        /// <summary>
        /// Updates the system UI for media transport controls to display media metadata from the given StorageFile.
        /// </summary>
        /// <param name="mediaFile">
        /// The media file being loaded.  This method will try to extract media metadata from the file for use in
        /// the system UI for media transport controls.
        /// </param>
        async Task UpdateSystemMediaControlsDisplayAsync(StorageFile mediaFile)
        {
            MediaPlaybackType mediaType = GetMediaTypeFromFileContentType(mediaFile);

            bool copyFromFileAsyncSuccessful = false;
            if (MediaPlaybackType.Unknown != mediaType)
            {
                // Use the SystemMediaTransportControlsDisplayUpdater's CopyFromFileAsync() API method to try extracting
                // from the StorageFile the relevant media metadata information (eg. track title, artist and album art 
                // for MediaPlaybackType.Music), copying them into properties of DisplayUpdater and related classes.
                //
                // In place of using CopyFromFileAsync() [or, you can't use that method because your app's media playback
                // scenario doesn't involve StorageFiles], you can also use other properties in DisplayUpdater and 
                // related classes to explicitly set the MediaPlaybackType and media metadata properties to values of 
                // your choosing.
                //
                // Usage notes:
                //     - the first argument cannot be MediaPlaybackType.Unknown
                //     - API method may throw an Exception on certain file errors from the passed in StorageFile, such as 
                //       file-not-found error (eg. file was deleted after user had picked it from file picker earlier).
                try
                {
                    copyFromFileAsyncSuccessful = await systemMediaControls.DisplayUpdater.CopyFromFileAsync(mediaType, mediaFile);
                }
                catch (Exception)

                {
                    // For this sample, we will handle this case same as CopyFromFileAsync returning false, 
                    // though we could provide our own metadata here.
                }
            }
            else
            {
                // If we are here, it means we are unable to determine a MediaPlaybackType based on the StorageFile's
                // ContentType (MIME type).  CopyFromFileAsync() requires a valid MediaPlaybackType (ie. cannot be
                // MediaPlaybackType.Unknown) for the first argument.  One way to handle this is to just pick a valid 
                // MediaPlaybackType value to call CopyFromFileAsync() with, based on what your app does (eg. Music
                // if your app is a music player).  The MediaPlaybackType directs the API to look for particular sets
                // of media metadata information from the StorageFile, and extraction is best-effort so in worst case
                // simply no metadata information are extracted.  
                //
                // For this sample, we will handle this case the same way as CopyFromFileAsync() returning false
            }

            if (!_isThisPageActive)
            {
                // User may have navigated away from this scenario page to another scenario page 
                // before the async operation completed.
                return;
            }

            if (!copyFromFileAsyncSuccessful)
            {
                // For this sample, if CopyFromFileAsync() didn't work for us for whatever reasons, we will just 
                // clear DisplayUpdater of all previously set metadata, if any.  This makes sure we don't end up 
                // displaying in the system UI for media transport controls any stale metadata from the media item 
                // we were previously playing.
                systemMediaControls.DisplayUpdater.ClearAll();
            }

            // Finally update the system UI display for media transport controls with the new values currently
            // set in the DisplayUpdater, be it via CopyFrmoFileAsync(), ClearAll(), etc.
            systemMediaControls.DisplayUpdater.Update();
        }
        /// <summary>
        /// Returns an appropriate MediaPlaybackType value based on the given StorageFile's ContentType (MIME type).
        /// </summary>
        /// <returns>
        /// One of the three valid MediaPlaybackType enum values, or MediaPlaybackType.Unknown if the ContentType 
        /// is not a media type (audio, video, image) or cannot be determined.
        //  </returns>
        /// <remarks>
        /// For use with SystemMediaTransportControlsDisplayUpdater.CopyFromFileAsync() in UpdateSystemMediaControlsDisplayAsync().
        /// </remarks>
        MediaPlaybackType GetMediaTypeFromFileContentType(StorageFile file)
        {
            // Determine the appropriate MediaPlaybackType of the media file based on its ContentType (ie. MIME type).
            // The MediaPlaybackType determines the information shown in the system UI for the system media transport
            // controls.  For example, the CopyFromFileAsync() API method will look for different metadata properties 
            // from the file to be extracted for eventual display in the system UI, depending on the MediaPlaybackType 
            // passed to the method.

            // NOTE: MediaPlaybackType.Unknown is *not* a valid value to use in SystemMediaTransportControls APIs.  
            // This method will return MediaPlaybackType.Unknown to indicate no valid MediaPlaybackType exists/can be 
            // determined for the given StorageFile.
            MediaPlaybackType mediaPlaybackType = MediaPlaybackType.Unknown;
            string fileMimeType = file.ContentType.ToLowerInvariant();

            if (fileMimeType.StartsWith("audio/"))
            {
                mediaPlaybackType = MediaPlaybackType.Music;
            }
            else if (fileMimeType.StartsWith("video/"))
            {
                mediaPlaybackType = MediaPlaybackType.Video;
            }
            else if (fileMimeType.StartsWith("image/"))
            {
                mediaPlaybackType = MediaPlaybackType.Image;
            }

            return mediaPlaybackType;
        }


    }
}
