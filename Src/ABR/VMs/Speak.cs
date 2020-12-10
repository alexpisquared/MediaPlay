using AsLink;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Popups;

namespace ABR.VMs
{
  public partial class AbrVM
  {
    SpeechSynthesizer _synth = new SpeechSynthesizer();

    public async Task Speak(string msg)
    {
      if (string.IsNullOrEmpty(msg))
        return;

      Debug.WriteLine($"spk:> {msg}");

      var isPlaying = mp_Vm.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
      if (isPlaying)
        mp_Vm.Pause();

      try
      {
        var wasSpeaking = 0;
        while (sp_Vm.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) { Debug.WriteLine($"   wasSpeaking: {++wasSpeaking}"); await Task.Delay(333); }// aug 2017: trying to resolve speaking conflicts.

        var speechSynthesisStream = await _synth.SynthesizeTextToStreamAsync(msg); // Create a stream from the text. This will be played using a media element.
        sp_Vm.Source = MediaSource.CreateFromStream(speechSynthesisStream, speechSynthesisStream.ContentType);

        TypedEventHandler<MediaPlayer, object> h = null;
        sp_Vm.MediaEnded += h = (s, a) =>
         {
           sp_Vm.MediaEnded -= h;
           sp_Vm.Source = null; // prevent replaying the old message on 
           if (isPlaying)
             mp_Vm.Play();
         };

        sp_Vm.Play();
      }
      catch (FileNotFoundException ex) /**/ { await new MessageDialog(ex.Message, "Media player components unavailable").ShowAsync(); }   // If media player components are unavailable, (eg, using a N SKU of windows), we won't be able to start media playback. Handle this gracefully
      catch (Exception ex) { DevOp.ExHrT(ex, GetType().FullName); }
    }
  }
}
