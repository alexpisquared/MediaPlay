using AsLink;
using ApxCmn;
using ApxCmn.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace ApxBgt
{
    /// <summary>
    /// Impletements IBackgroundTask to provide an entry point for app code to be run in background. 
    /// Also takes care of handling UVC and communication channel with foreground
    /// </summary>
    public sealed partial class Bgt : IBackgroundTask
	{
		SystemMediaTransportControls _smtc;
		MediaPlaybackList _mpl = new MediaPlaybackList();
		BackgroundTaskDeferral deferral; // Used to keep task alive
		AppState foregroundAppState = AppState.Unknown;
		ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
		bool playbackStartedPreviously = false;

		Uri GetCurrentTrackId()
		{
			if (_mpl == null)
				return null;

			return GetTrackId(_mpl.CurrentItem);
		}
		Uri GetTrackId(MediaPlaybackItem item)
		{
			if (item == null)
				return null; // no track playing

			return item.Source.CustomProperties[ApplicationSettingsConstants.TrackId] as Uri;
		}



		/// <summary>
		/// Update Universal Volume Control (UVC) using SystemMediaTransPortControl APIs
		/// </summary>
		void UpdateUVCOnNewTrack(MediaPlaybackItem item)
		{
			try
			{
				if (item == null)
				{
					_smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
					_smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
					_smtc.DisplayUpdater.Update();
					return;
				}

				_smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
				_smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
				_smtc.DisplayUpdater.MusicProperties.Title = item.Source.CustomProperties[ApplicationSettingsConstants.TitleKey] as string;

				//if (item.Source.CustomProperties.ContainsKey(ApplicationSettingsConstants.ThumbNl) && item.Source.CustomProperties[ApplicationSettingsConstants.ThumbNl] is BitmapImage && item.Source.CustomProperties[ApplicationSettingsConstants.ThumbNl] != null)
				//	_smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference// .CreateFromFile()
				//		.CreateFromUri(((BitmapImage)item.Source.CustomProperties[ApplicationSettingsConstants.ThumbNl]).UriSource);
				////leaves prev img; better keep as is i.e.: show App icon.
				////else if (item.Source.CustomProperties.ContainsKey(ApplicationSettingsConstants.SFile) && item.Source.CustomProperties[ApplicationSettingsConstants.SFile] is StorageFile && item.Source.CustomProperties[ApplicationSettingsConstants.SFile] != null)
				////	_smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromFile((StorageFile)item.Source.CustomProperties[ApplicationSettingsConstants.SFile]);
				//else
				{
					var artUri = item.Source.CustomProperties[ApplicationSettingsConstants.ArtKey] as Uri;
					if (artUri != null)
						_smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(artUri);
					else
						_smtc.DisplayUpdater.Thumbnail = null;
				}
			}
			catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ MessageService.SendMessageToForeground(new ExceptionCaughtMessage(ex.Message)); }

			_smtc.DisplayUpdater.Update();
		}

		/// <summary>
		/// Fires when any SystemMediaTransportControl property is changed by system or user
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
		{
			// If soundlevel turns to muted, app can choose to pause the music
		}

		/// <summary>
		/// This function controls the button events from UVC.
		/// This code if not run in background process, will not be able to handle button pressed events when app is suspended.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
		{
			switch (args.Button)
			{
				case SystemMediaTransportControlsButton.Play:
					Debug.WriteLine("UVC play button pressed");

					// When the background task has been suspended and the SMTC
					// starts it again asynchronously, some time is needed to let
					// the task startup process in Run() complete.

					// Wait for task to start. 
					// Once started, this stays signaled until shutdown so it won't wait
					// again unless it needs to.
					bool result = backgroundTaskStarted.WaitOne(5000);
					if (!result)
						throw new Exception("Background Task didnt initialize in time");

					StartPlayback();
					break;
				case SystemMediaTransportControlsButton.Pause:
					Debug.WriteLine("UVC pause button pressed");
					try
					{
						BackgroundMediaPlayer.Current.Pause();
					}
					catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ MessageService.SendMessageToForeground(new ExceptionCaughtMessage(ex.Message)); }
					break;
				case SystemMediaTransportControlsButton.Next:
					Debug.WriteLine("UVC next button pressed");
					SkipToNext();
					break;
				case SystemMediaTransportControlsButton.Previous:
					Debug.WriteLine("UVC previous button pressed");
					SkipToPrevious();
					break;
			}
		}



		//>>

		//<< Playlist management functions and handlers
		/// <summary>
		/// Start playlist and change UVC state
		/// </summary>
		void StartPlayback()
		{
			try
			{
				if (playbackStartedPreviously) // If playback was already started once we can just resume playing.
					BackgroundMediaPlayer.Current.Play();
				else
				{
					playbackStartedPreviously = true;

					var currentTrackId = AppSettingsHelper.ReadVal(ApplicationSettingsConstants.TrackId); // If the task was cancelled we would have saved the current track and its position. We will try playback from there.
					if (currentTrackId == null)
						BackgroundMediaPlayer.Current.Play();
					else
					{
						Debug.WriteLine($"**>Getting {currentTrackId}     from:"); _mpl.Items.ToList().ForEach(r => Debug.WriteLine($"**>        {GetTrackId(r)}"));

						var index = _mpl.Items.ToList().FindIndex(item => GetTrackId(item).ToString() == (string)currentTrackId); // Find the index of the item by name

						var currentTrackPosition = AppSettingsHelper.ReadVal(ApplicationSettingsConstants.Position);
						if (currentTrackPosition == null)
						{
							Debug.WriteLine("StartPlayback: Switching to track " + index); // Play from start if we dont have position
							_mpl.MoveTo((uint)index);

							BackgroundMediaPlayer.Current.Play();
						}
						else
						{
							// Play from exact position otherwise
							TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;
							handler = (MediaPlaybackList list, CurrentMediaPlaybackItemChangedEventArgs args) =>
							{
								if (args.NewItem == _mpl.Items[index])
								{
									// Unsubscribe because this only had to run once for this item
									_mpl.CurrentItemChanged -= handler;

									// Set position
									var position = TimeSpan.Parse((string)currentTrackPosition);
									Debug.WriteLine("StartPlayback: Setting Position " + position);
									BackgroundMediaPlayer.Current.Position = position;

									BackgroundMediaPlayer.Current.Play();
								}
							};
							_mpl.CurrentItemChanged += handler;

							Debug.WriteLine("StartPlayback: Switching to track " + index); // Switch to the track which will trigger an item changed event
							playbackStartedPreviously = false;
							_mpl.MoveTo((uint)index);
							playbackStartedPreviously = true;
						}
					}
				}
			}
			catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ MessageService.SendMessageToForeground(new ExceptionCaughtMessage(ex.Message)); }
		}

		void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args) // Raised when playlist changes to a new track
		{
			// Get the new item
			var item = args.NewItem;
			Debug.WriteLine("PlaybackList_CurrentItemChanged: " + (item == null ? "null" : GetTrackId(item).ToString()));

			// Update the system view
			UpdateUVCOnNewTrack(item);

			// Get the current track
			Uri currentTrackId = null;
			if (item != null)
				currentTrackId = item.Source.CustomProperties[ApplicationSettingsConstants.TrackId] as Uri;

			// Notify foreground of change or persist for later
			if (foregroundAppState == AppState.Active)
				MessageService.SendMessageToForeground(new TrackChangedMessage(currentTrackId));
			else
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.TrackId, currentTrackId == null ? null : currentTrackId.ToString());
		}

		/// <summary>
		/// Skip track and update UVC via SMTC
		/// </summary>
		void SkipToPrevious()
		{
			_smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
			_mpl.MovePrevious();
		}

		/// <summary>
		/// Skip track and update UVC via SMTC
		/// </summary>
		void SkipToNext()
		{
			_smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
			_mpl.MoveNext();
		}
		//>>

		//<< Background Media Player Handlers
		void Current_CurrentStateChanged(MediaPlayer sender, object args)
		{
			if (sender.CurrentState == MediaPlayerState.Playing)
			{
				_smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
			}
			else if (sender.CurrentState == MediaPlayerState.Paused)
			{
				_smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
			}
			else if (sender.CurrentState == MediaPlayerState.Closed)
			{
				_smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
			}
		}

		/// <summary>
		/// Raised when a message is recieved from the foreground app
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
		{
			AppSuspendedMessage appSuspendedMessage;
			if (MessageService.TryParseMessage(e.Data, out appSuspendedMessage))
			{
				Debug.WriteLine("App suspending"); // App is suspended, you can save your task state at this point
				foregroundAppState = AppState.Suspended;
				var currentTrackId = GetCurrentTrackId();
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.TrackId, currentTrackId == null ? null : currentTrackId.ToString());
				return;
			}

			AppResumedMessage appResumedMessage;
			if (MessageService.TryParseMessage(e.Data, out appResumedMessage))
			{
				Debug.WriteLine("App resuming"); // App is resumed, now subscribe to message channel
				foregroundAppState = AppState.Active;
				return;
			}

			StartPlaybackMessage startPlaybackMessage;
			if (MessageService.TryParseMessage(e.Data, out startPlaybackMessage))
			{
				//Foreground App process has signalled that it is ready for playback
				Debug.WriteLine("Starting Playback");
				StartPlayback();
				return;
			}

			SkipNextMessage skipNextMessage;
			if (MessageService.TryParseMessage(e.Data, out skipNextMessage))
			{
				// User has chosen to skip track from app context.
				Debug.WriteLine("Skipping to next");
				SkipToNext();
				return;
			}

			SkipPreviousMessage skipPreviousMessage;
			if (MessageService.TryParseMessage(e.Data, out skipPreviousMessage))
			{
				// User has chosen to skip track from app context.
				Debug.WriteLine("Skipping to previous");
				SkipToPrevious();
				return;
			}

			TrackChangedMessage trackChangedMessage;
			if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
			{
				var index = _mpl.Items.ToList().FindIndex(i => (Uri)i.Source.CustomProperties[ApplicationSettingsConstants.TrackId] == trackChangedMessage.TrackId);
				Debug.WriteLine("Skipping to track " + index);
				_smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
				_mpl.MoveTo((uint)index);
				return;
			}

			UpdatePlaylistMessage updatePlaylistMessage;
			if (MessageService.TryParseMessage(e.Data, out updatePlaylistMessage))
			{
				CreatePlaybackList(updatePlaylistMessage.Songs);
				return;
			}
		}

		/// <summary>
		/// Create a playback list from the list of songs received from the foreground app.
		/// </summary>
		/// <param name="songs"></param>
		async void CreatePlaybackList(IEnumerable<MediaInfo> songs)
		{
			//todo: later: await Speak("Are we there yet?");
			_mpl = new MediaPlaybackList(); // Make a new list and enable looping
			_mpl.AutoRepeatEnabled = true;

			foreach (var song in songs) // Add playback items to the list
			{
				MediaSource mSrc = null;

				if (string.IsNullOrEmpty(song.FullPath))
				{
					mSrc = MediaSource.CreateFromUri(song.MediaUri);

					mSrc.CustomProperties[ApplicationSettingsConstants.TrackId] = song.MediaUri;
				}
				else
				{
					try
					{
						var sFile = await song.GetStorageFile();

						mSrc = MediaSource.CreateFromStorageFile(sFile);
						mSrc.CustomProperties[ApplicationSettingsConstants.SFile] = sFile;

						/* todo: supply thorugh other means
						var tn = await MediaInfo.GetThumbnailBI(sFile);
						if (tn != null)
							mSrc.CustomProperties[ApplicationSettingsConstants.ThumbNl] = tn;
						else
							Debug.WriteLine($"//> Unable to get thumbnail for {sFile.Name}");
							*/

						mSrc.CustomProperties[ApplicationSettingsConstants.TrackId] = new Uri(song.FullPath);
					}
					catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ MessageService.SendMessageToForeground(new ExceptionCaughtMessage(ex.Message)); }
				}

				mSrc.CustomProperties[ApplicationSettingsConstants.TitleKey] = song.Title;
				mSrc.CustomProperties[ApplicationSettingsConstants.ArtKey] = song.ArtUri;

				_mpl.Items.Add(new MediaPlaybackItem(mSrc));
			}

			// Don't auto start
			BackgroundMediaPlayer.Current.AutoPlay = false;

			// Assign the list to the player
			BackgroundMediaPlayer.Current.Source = _mpl;

			// Add handler for future playlist item changes
			_mpl.CurrentItemChanged += PlaybackList_CurrentItemChanged;
		}
		//>>
	}
}
