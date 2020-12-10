using ApxCmn;
using ApxCmn.Messages;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;
using AsLink;

/* This background task will start running the first time the
 * MediaPlayer singleton instance is accessed from foreground. When a new audio 
 * or video app comes into picture the task is expected to recieve the cancelled 
 * event. User can save state and shutdown MediaPlayer at that time. When foreground 
 * app is resumed or restarted check if your music is still playing or continue from
 * previous state.
 * 
 * This task also implements SystemMediaTransportControl APIs for windows phone universal 
 * volume control. Unlike Windows 8.1 where there are different views in phone context, 
 * SystemMediaTransportControl is singleton in nature bound to the process in which it is 
 * initialized. If you want to hook up volume controls for the background task, do not 
 * implement SystemMediaTransportControls in foreground app process.
 */

namespace ApxBgt
{
    /// <summary>
    /// Impletements IBackgroundTask to provide an entry point for app code to be run in background. 
    /// Also takes care of handling UVC and communication channel with foreground
    /// </summary>
    public sealed partial class Bgt : IBackgroundTask
	{
 		public void Run(IBackgroundTaskInstance taskInstance) // The Run method is the entry point of a background task. 
		{
			Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");

			// Initialize SystemMediaTransportControls (SMTC) for integration with
			// the Universal Volume Control (UVC).
			//
			// The UI for the UVC must update even when the foreground process has been terminated
			// and therefore the SMTC is configured and updated from the background task.
			_smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
			_smtc.ButtonPressed += smtc_ButtonPressed;
			_smtc.PropertyChanged += smtc_PropertyChanged;
			_smtc.IsEnabled = true;
			_smtc.IsPauseEnabled = true;
			_smtc.IsPlayEnabled = true;
			_smtc.IsNextEnabled = true;
			_smtc.IsPreviousEnabled = true;

			// Read persisted state of foreground app
			var value = AppSettingsHelper.ReadVal(ApplicationSettingsConstants.AppState);
			if (value == null)
				foregroundAppState = AppState.Unknown;
			else
				foregroundAppState = EnumHelper.Parse<AppState>(value.ToString());

			// Add handlers for MediaPlayer
			BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

			// Initialize message channel 
			BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

			// Send information to foreground that background task has been started if app is active
			if (foregroundAppState != AppState.Suspended)
				MessageService.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

			AppSettingsHelper.SaveVal(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());

			deferral = taskInstance.GetDeferral(); // This must be retrieved prior to subscribing to events below which use it

			// Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
			backgroundTaskStarted.Set();

			// Associate a cancellation and completed handlers with the background task.
			taskInstance.Task.Completed += TaskCompleted;
			taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled); // event may raise immediately before continung thread excecution so must be at the end
		}

		void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
		{
			Debug.WriteLine("Bgt " + sender.TaskId + " Completed...");
			deferral.Complete();
		}

		/// <summary>
		/// Handles background task cancellation. Task cancellation happens due to:
		/// 1. Another Media app comes into foreground and starts playing music 
		/// 2. Resource pressure. Your task is consuming more CPU and memory than allowed.
		/// In either case, save state so that if foreground app resumes it can know where to start.
		/// </summary>
		void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
		{
			// You get some time here to save your state before process and resources are reclaimed
			Debug.WriteLine("Bgt " + sender.Task.TaskId + " Cancel Requested...");
			try
			{
				// immediately set not running
				backgroundTaskStarted.Reset();

				// save state
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.TrackId, GetCurrentTrackId() == null ? null : GetCurrentTrackId().ToString());
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Canceled.ToString());
				AppSettingsHelper.SaveVal(ApplicationSettingsConstants.AppState, Enum.GetName(typeof(AppState), foregroundAppState));

				// unsubscribe from list changes
				if (_mpl != null)
				{
					_mpl.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
					_mpl = null;
				}

				// unsubscribe event handlers
				BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
				_smtc.ButtonPressed -= smtc_ButtonPressed;
				_smtc.PropertyChanged -= smtc_PropertyChanged;

				BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
			}
			catch (Exception ex) { Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break(); /*else throw;*/ MessageService.SendMessageToForeground(new ExceptionCaughtMessage(ex.Message)); }
			deferral.Complete(); // signals task completion. 
			Debug.WriteLine("Bgt Cancel complete...");
		}
		
	}
}