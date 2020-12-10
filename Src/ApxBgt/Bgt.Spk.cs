using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ApxBgt
{
    /// <summary>
    /// Impletements IBackgroundTask to provide an entry point for app code to be run in background. 
    /// Also takes care of handling UVC and communication channel with foreground
    /// </summary>
    public sealed partial class Bgt : IBackgroundTask
	{
		SpeechSynthesizer synthesizer;
		ResourceContext speechContext;    // ResourceMap speechResourceMap;

		MediaPlayer _media = BackgroundMediaPlayer.Current;
		string btnSpeak_Content = "";


		public void Speech0Ctor()
		{
			synthesizer = new SpeechSynthesizer();
			speechContext = ResourceContext.GetForCurrentView();
			speechContext.Languages = new string[] { SpeechSynthesizer.DefaultVoice.Language };

			//speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationTTSResources");

			//InitializeListboxVoiceChooser();
		}

		async Task Speak(string text)//_Click(object sender, RoutedEventArgs e)
		{
			if (synthesizer == null) Speech0Ctor();

			if (_media.CurrentState.Equals(MediaElementState.Playing))      // If the media is playing, the user has pressed the button to stop the playback.
			{
				_media.Pause();
				btnSpeak_Content = "Speak";
			}
			else
			{
				if (!String.IsNullOrEmpty(text))
				{
					btnSpeak_Content = "Pause"; // Change the button label. You could also just disable the button if you don't want any user control.

					try
					{
						SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text); // Create a stream from the text. This will be played using a media element.

						_media.AutoPlay = true;
						_media.Source = MediaSource.CreateFromStream(synthesisStream, synthesisStream.ContentType); // (synthesisStream, synthesisStream.ContentType);   ... synthesisStream.GetType().ToString() //from: http://stackoverflow.com/questions/36875193/backgroundaudio-not-work-in-windows-10-mobile
						_media.Play();
						await Task.Delay(1500);
					}
					catch (System.IO.FileNotFoundException ex)
					{
						Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break();

						// If media player components are unavailable, (eg, using a N SKU of windows), we won't be able to start media playback. Handle this gracefully
						btnSpeak_Content = "Speak";
						//btnSpeak.IsEnabled = false;
						//textToSynthesize.IsEnabled = false;
						//listboxVoiceChooser.IsEnabled = false;
						var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
						await messageDialog.ShowAsync();
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"$#~>{ex.Message}"); if (Debugger.IsAttached) Debugger.Break();

						// If the text is unable to be synthesized, throw an error message to the user.
						btnSpeak_Content = "Speak";
						_media.AutoPlay = false;
						var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to synthesize text");
						await messageDialog.ShowAsync();
					}
				}
			}
		}

		void media_MediaEnded(object sender, RoutedEventArgs e) { btnSpeak_Content = "Speak"; }

		/// <summary>
		/// This creates items out of the system installed voices. The voices are then displayed in a listbox.
		/// This allows the user to change the voice of the synthesizer in your app based on their preference.
		/// </summary>
		// void InitializeListboxVoiceChooser()
		//{
		//	// Get all of the installed voices.
		//	var voices = SpeechSynthesizer.AllVoices;

		//	// Get the currently selected voice.
		//	VoiceInformation currentVoice = synthesizer.Voice;

		//	foreach (VoiceInformation voice in voices.OrderBy(p => p.Language))
		//	{
		//		ComboBoxItem item = new ComboBoxItem();
		//		item.Name = voice.DisplayName;
		//		item.Tag = voice;
		//		item.Content = voice.DisplayName + " (Language: " + voice.Language + ")";
		//		listboxVoiceChooser.Items.Add(item);

		//		// Check to see if we're looking at the current voice and set it as selected in the listbox.
		//		if (currentVoice.Id == voice.Id)
		//		{
		//			item.IsSelected = true;
		//			listboxVoiceChooser.SelectedItem = item;
		//		}
		//	}
		//}

		/// <summary>
		/// This is called when the user has selects a voice from the drop down.
		/// </summary>
		/// <param name="sender">unused object parameter</param>
		/// <param name="e">unused event parameter</param>
		void ListboxVoiceChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//ComboBoxItem item = (ComboBoxItem)(listboxVoiceChooser.SelectedItem);
			//VoiceInformation voice = (VoiceInformation)(item.Tag);
			synthesizer.Voice = SpeechSynthesizer.AllVoices.First();

			// update UI text to be an appropriate default translation.
			speechContext.Languages = new string[] { synthesizer.Voice.Language };
			//textToSynthesize.Text = speechResourceMap.GetValue("SynthesizeTextDefaultText", speechContext).ValueAsString;
		}
	}
}
