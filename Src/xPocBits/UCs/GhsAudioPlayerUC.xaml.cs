using ApxCmn;
using AsLink;
using ApxCmn.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using xPocBits.VMs;

namespace xPocBits.UCs
{
	public sealed partial class GhsAudioPlayerUC : UserControl //public sealed partial class Scenario1 : Page
	{
		MainVM _vm = null;

		Dictionary<string, BitmapImage> albumArtCache = new Dictionary<string, BitmapImage>();
		AutoResetEvent backgroundAudioTaskStarted;
		const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
		bool _isMyBackgroundTaskRunning = false; public bool IsMyBackgroundTaskRunning // Gets the information about background task is running or not by reading the setting saved by background task. This is used to determine when to start the task and also when to avoid sending messages.
		{
			get
			{
				if (_isMyBackgroundTaskRunning)
					return true;

				string value = AppSettingsHelper.ReadVal(ApplicationSettingsConstants.BackgroundTaskState) as string;
				if (value == null)
				{
					return false;
				}
				else
				{
					try
					{
						_isMyBackgroundTaskRunning = EnumHelper.Parse<BackgroundTaskState>(value) == BackgroundTaskState.Running;
					}
					catch (ArgumentException)
					{
						_isMyBackgroundTaskRunning = false;
					}
					return _isMyBackgroundTaskRunning;
				}
			}
		}

		public GhsPlaylistUC PlaylistView { get { return ghsPlaylistUC; } }

		public GhsAudioPlayerUC()
		{
			this.InitializeComponent();

			_vm = MainVM.Instance;
			DataContext = _vm;

			InitializeSongs();      // Create a static song list

			backgroundAudioTaskStarted = new AutoResetEvent(false); // Setup the initialization lock
		}

		void InitializeSongs()
		{
			//var song1 = new MediaInfo(); song1.Title = "Ring 1"; song1.MediaUri = new Uri("ms-appx:///Assets/Media/Ring01.wma"); song1.ArtUri = new Uri("ms-appx:///Assets/Media/Ring01.jpg"); ghsPlaylistUC.Songs.Add(song1);
			//var song2 = new MediaInfo(); song2.Title = "Ring 2"; song2.MediaUri = new Uri("ms-appx:///Assets/Media/Ring02.wma"); song2.ArtUri = new Uri("ms-appx:///Assets/Media/Ring02.jpg"); ghsPlaylistUC.Songs.Add(song2);
			for (int i = 1; i <= 3; ++i) { var segment = new MediaInfo(); segment.Title = "Ring 3 Part " + i; segment.MediaUri = new Uri("ms-appx:///Assets/Media/Ring03Part" + i + ".wma"); segment.ArtUri = new Uri("ms-appx:///Assets/Media/Ring03Part" + i + ".jpg"); ghsPlaylistUC.Songs.Add(segment); }

			foreach (var mi in _vm.MediaInfos)
			{
				mi.Title = mi.FName;
				//mi.MediaUri = new Uri(mi.FullPath); 
				//mi.ArtUri = new Uri("ms-appx:///Assets/Media/sintel.jpg"); // crashes without it

				ghsPlaylistUC.Songs.Add(mi);
			}

			foreach (var song in ghsPlaylistUC.Songs) // Pre-cache all album art to facilitate smooth gapless transitions. A production app would have a more sophisticated object cache.
			{
				if (song.Thumbnail != null)
					albumArtCache[song.SFile.Name] = song.Thumbnail;
				else if (song.ArtUri != null)
				{
					var bitmap = new BitmapImage();
					bitmap.UriSource = song.ArtUri;
					albumArtCache[song.ArtUri.ToString()] = bitmap;
				}
			}
		}

		/// <summary>
		/// Read persisted current track information from application settings
		/// </summary>
		Uri GetCurrentTrackIdAfterAppResume()
		{
			object value = AppSettingsHelper.ReadVal(ApplicationSettingsConstants.TrackId);
			if (value != null)
				return new Uri((String)value);
			else
				return null;
		}




		/// <summary>
		/// You should never cache the MediaPlayer and always call Current. It is possible
		/// for the background task to go away for several different reasons. When it does
		/// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
		/// and restart the background task.
		/// </summary>
		public MediaPlayer CurrentPlayer
		{
			get
			{
				MediaPlayer mp = null;
				int retryCount = 2;

				while (mp == null && --retryCount >= 0)
				{
					try
					{
						mp = BackgroundMediaPlayer.Current;
					}
					catch (Exception ex)
					{
						if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
						{
							// The foreground app uses RPC to communicate with the background process.
							// If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
							// is returned when calling Current. We must restart the task, the while loop will retry to set mp.
							ResetAfterLostBackground();
							StartBackgroundAudioTask();
						}
						else
						{
							throw;
						}
					}
				}

				if (mp == null)
				{
					throw new Exception("Failed to get a MediaPlayer instance.");
				}

				return mp;
			}
		}

		/// <summary>
		/// The background task did exist, but it has disappeared. Put the foreground back into an initial state. Unfortunately,
		/// any attempts to unregister things on BackgroundMediaPlayer.Current will fail with the RPC error once the background task has been lost.
		/// </summary>
		public void ResetAfterLostBackground()
		{
			BackgroundMediaPlayer.Shutdown();
			_isMyBackgroundTaskRunning = false;
			backgroundAudioTaskStarted.Reset();
			prevButton.IsEnabled = true;
			nextButton.IsEnabled = true;
			AppSettingsHelper.SaveVal(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Unknown.ToString());
			playButton.Content = "| |";

			try
			{
				BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
			}
			catch (Exception ex)
			{
				if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
				{
					throw new Exception("Failed to get a MediaPlayer instance.");
				}
				else
				{
					throw;
				}
			}
		}




		/// <summary>
		/// Sends message to background informing app has resumed
		/// Subscribe to MediaPlayer events
		/// </summary>
		public void ForegroundApp_Resuming(object sender, object e)
		{
			AppSettingsHelper.SaveVal(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

			// Verify the task is running
			if (IsMyBackgroundTaskRunning)
			{
				// If yes, it's safe to reconnect to media play handlers
				AddMediaPlayerEventHandlers();

				// Send message to background task that app is resumed so it can start sending notifications again
				MessageService.SendMessageToBackground(new AppResumedMessage());

				updateTransportControls(CurrentPlayer.CurrentState);

				var trackId = GetCurrentTrackIdAfterAppResume();
				txtCurrentTrack.Text = trackId == null ? string.Empty : ghsPlaylistUC.GetSongById(trackId).Title;
				txtCurrentState.Text = $"{CurrentPlayer.CurrentState} x{_vm.PlaybackRate}";
			}
			else
			{
				playButton.Content = ">";     // Change to play button
				txtCurrentTrack.Text = string.Empty;
				txtCurrentState.Text = "Background Task Not Running";
			}
		}

		/// <summary>
		/// Send message to Background process that app is to be suspended
		/// Stop clock and slider when suspending
		/// Unsubscribe handlers for MediaPlayer events
		/// </summary>
		public void ForegroundApp_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();

			// Only if the background task is already running would we do these, otherwise
			// it would trigger starting up the background task when trying to suspend.
			if (IsMyBackgroundTaskRunning)
			{
				// Stop handling player events immediately
				RemoveMediaPlayerEventHandlers();

				// Tell the background task the foreground is suspended
				MessageService.SendMessageToBackground(new AppSuspendedMessage());
			}

			// Persist that the foreground app is suspended
			AppSettingsHelper.SaveVal(ApplicationSettingsConstants.AppState, AppState.Suspended.ToString());

			deferral.Complete();
		}



		/// <summary>
		/// MediaPlayer state changed event handlers. 
		/// Note that we can subscribe to events even if Media Player is playing media in background
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
		{
			var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				txtCurrentState.Text = $"{currentState} x{_vm.PlaybackRate}";

				updateTransportControls(currentState);
			});
		}

		/// <summary>
		/// This event is raised when a message is recieved from BackgroundAudioTask
		/// </summary>
		async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
		{
			ExceptionCaughtMessage exceptionCaughtMessage;
			if (MessageService.TryParseMessage(e.Data, out exceptionCaughtMessage))
			{
				await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => // When foreground app is active change track based on background message
				{
					_vm.ExnMsg = 
					txtCurException.Text = exceptionCaughtMessage.ExceptionMsg;
				});
				return;
			}

			TrackChangedMessage trackChangedMessage;
			if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
			{
				await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => // When foreground app is active change track based on background message
				{
					if (trackChangedMessage.TrackId == null) // If playback stopped then clear the UI
					{
						ghsPlaylistUC.SelectedIndex = -1;
						curArtImg.Source = null;
						txtCurrentTrack.Text = string.Empty;
						prevButton.IsEnabled = false;
						nextButton.IsEnabled = false;
						return;
					}

					var songIndex = ghsPlaylistUC.GetSongIndexById(trackChangedMessage.TrackId);
					var song = ghsPlaylistUC.Songs[songIndex];

					ghsPlaylistUC.SelectedIndex = songIndex;

					curArtImg.Source = song.Thumbnail ?? albumArtCache[song.ArtUri.ToString()];

					txtCurrentTrack.Text = song.Title;

					prevButton.IsEnabled = true;
					nextButton.IsEnabled = true;
				});
				return;
			}

			BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
			if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
			{
				Debug.WriteLine("BackgroundAudioTask started");
				backgroundAudioTaskStarted.Set(); // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running and ready to receive messages
				return;
			}
		}

		//>>

		//<< Button and Control Click Event Handlers
		public void PlaylistView_ItemClick(object sender, ItemClickEventArgs e)
		{
			var song = e.ClickedItem as MediaInfo;
			Debug.WriteLine("Clicked item from App: " + song.MediaUri.ToString());

			// Start the background task if it wasn't running
			if (!IsMyBackgroundTaskRunning || MediaPlayerState.Closed == CurrentPlayer.CurrentState)
			{
				// First update the persisted start track
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.TrackId, song.MediaUri.ToString());
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.Position, new TimeSpan().ToString());

				// Start task
				StartBackgroundAudioTask();
			}
			else
			{
				MessageService.SendMessageToBackground(new TrackChangedMessage(string.IsNullOrEmpty(song.FullPath) ? song.MediaUri : new Uri(song.FullPath))); // Switch to the selected track!!!!!!!!!!!!!!!
			}

			if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
			{
				CurrentPlayer.Play();
			}
		}

		/// <summary>
		/// Sends message to the background task to skip to the previous track.
		/// </summary>
		void prevButton_Click(object sender, RoutedEventArgs e)
		{
			MessageService.SendMessageToBackground(new SkipPreviousMessage());

			// Prevent the user from repeatedly pressing the button and causing 
			// a backlong of button presses to be handled. This button is re-eneabled 
			// in the TrackReady Playstate handler.
			prevButton.IsEnabled = false;
		}

		/// <summary>
		/// If the task is already running, it will just play/pause MediaPlayer Instance
		/// Otherwise, initializes MediaPlayer Handlers and starts playback
		/// track or to pause if we're already playing.
		/// </summary>
		void playButton_Click(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine(BackgroundTaskRegistration.AllTasks.Count, "Looks like only 'Local' tasksk are here.. Count"); foreach (var task in BackgroundTaskRegistration.AllTasks) Debug.WriteLine($"{task.Value.Name}");

			Debug.WriteLine("Play button pressed from App");
			if (IsMyBackgroundTaskRunning)
			{
				if (MediaPlayerState.Playing == CurrentPlayer.CurrentState)
				{
					CurrentPlayer.Pause();
				}
				else if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
				{
					CurrentPlayer.Play();
				}
				else if (MediaPlayerState.Closed == CurrentPlayer.CurrentState)
				{
					StartBackgroundAudioTask();
				}
			}
			else
			{
				StartBackgroundAudioTask();
			}
		}

		/// <summary>
		/// Tells the background audio agent to skip to the next track.
		/// </summary>
		/// <param name="sender">The button</param>
		/// <param name="e">Click event args</param>
		void nextButton_Click(object sender, RoutedEventArgs e)
		{
			MessageService.SendMessageToBackground(new SkipNextMessage());

			nextButton.IsEnabled = false; // Prevent the user from repeatedly pressing the button and causing a backlong of button presses to be handled. This button is re-eneabled in the TrackReady Playstate handler.
		}

		void speedButton_Click(object sender, RoutedEventArgs e)
		{
			var popupMenu = new PopupMenu(); // Create menu and add commands

			popupMenu.Commands.Add(new UICommand("5.0x", command => _vm.PlaybackRate = CurrentPlayer.PlaybackRate = 5.0));
			popupMenu.Commands.Add(new UICommand("3.0x", command => _vm.PlaybackRate = CurrentPlayer.PlaybackRate = 3.0));
			popupMenu.Commands.Add(new UICommand("2.0x", command => _vm.PlaybackRate = CurrentPlayer.PlaybackRate = 2.0));
			popupMenu.Commands.Add(new UICommand("1.7x", command => _vm.PlaybackRate = CurrentPlayer.PlaybackRate = 1.7));
			popupMenu.Commands.Add(new UICommand("1.4x", command => _vm.PlaybackRate = CurrentPlayer.PlaybackRate = 1.4));
			popupMenu.Commands.Add(new UICommand("1.0x", command => _vm.PlaybackRate = CurrentPlayer.PlaybackRate = 1.0));
			popupMenu.Commands.Add(new UICommand("0.5x", command => _vm.PlaybackRate = CurrentPlayer.PlaybackRate = 0.5));
			popupMenu.Commands.Add(new UICommand("0.1x", command => _vm.PlaybackRate = CurrentPlayer.PlaybackRate = 0.1));

			var button = (Button)sender;
			var transform = button.TransformToVisual(null); // Get button transform and then offset it by half the button width to center. This will show the popup just above the button.
			var point = transform.TransformPoint(new Point(button.Width / 2, 0));

			var ignoreAsyncResult = popupMenu.ShowAsync(point);
		}
		//>> Button Click Event Handlers

		//<< Media Playback Helper methods

		void updateTransportControls(MediaPlayerState state) { playButton.Icon = state == MediaPlayerState.Playing ? new SymbolIcon(Symbol.Pause) : new SymbolIcon(Symbol.Play); }

		/// <summary>
		/// Unsubscribes to MediaPlayer events. Should run only on suspend
		/// </summary>
		public void RemoveMediaPlayerEventHandlers()
		{
			try
			{
				BackgroundMediaPlayer.Current.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
				BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
			}
			catch (Exception ex)
			{
				if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
				{
					// do nothing
				}
				else
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Subscribes to MediaPlayer events
		/// </summary>
		void AddMediaPlayerEventHandlers()
		{
			CurrentPlayer.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;

			try
			{
				BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
			}
			catch (Exception ex)
			{
				if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
				{
					// Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
					ResetAfterLostBackground();
				}
				else
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Initialize Background Media Player Handlers and starts playback
		/// </summary>
		void StartBackgroundAudioTask()
		{
			AddMediaPlayerEventHandlers();

			var startResult = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				bool result = backgroundAudioTaskStarted.WaitOne(10000);
				//Send message to initiate playback
				if (result == true)
				{
					MessageService.SendMessageToBackground(new UpdatePlaylistMessage(ghsPlaylistUC.Songs.ToList()));
					MessageService.SendMessageToBackground(new StartPlaybackMessage());
				}
				else
				{
					throw new Exception("Background Audio Task didn't start in expected time");
				}
			});
			startResult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
		}

		void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
		{
			if (status == AsyncStatus.Completed)
			{
				Debug.WriteLine("Background Audio Task initialized");
			}
			else if (status == AsyncStatus.Error)
			{
				Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
			}
		}
		//>>
	}
}