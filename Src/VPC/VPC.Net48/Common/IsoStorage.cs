using AAV.Sys.Ext;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Xml.Serialization;


namespace AsLink
{
  public static class IsoStore
  {
    const string _isoName = @"isoStore.AP.Test.xml.txt";

    public static void TestIsoAccessibilty(string msg)
    {
      try
      {
        var save = new DtoContainer
        {
          Msg = msg,
          Time = DateTime.Now
        };

        trySave(save);
        var read = tryLoad();
        var rr = GetIsolatedStorageFile().GetDirectoryNames();
        Trace.WriteLine(save.Time == read.Time ? "AP: SUCCESS - Iso Storage is fine." : "AP: Iso Storage is not accessible!!!");
      }
      catch (Exception ex) { ex.Log(); }
    }
    static void trySave(DtoContainer usb)
    {
      try { using (var z = new IsolatedStorageFileStream(_isoName, FileMode.Create, GetIsolatedStorageFile())) { using (var streamWriter = new StreamWriter(z)) { new XmlSerializer(usb.GetType()).Serialize(streamWriter, usb); streamWriter.Close(); } } } catch (Exception ex) { ex.Log(); }

      //Jul2015: Back-up storage to normal store has been removed for code scan requirement: 
      //try { using (var z = new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _isoName), FileMode.OpenOrCreate)) { using (var streamWriter = new StreamWriter(z)) { new XmlSerializer(usb.GetType()).Serialize(streamWriter, usb); streamWriter.Close ( ); } } }		catch (Exception ex) { ex.Log(); }
    }
    static DtoContainer tryLoad()
    {
      try
      {
        var isoStore = GetIsolatedStorageFile();
        using (var stream = new IsolatedStorageFileStream(_isoName, FileMode.OpenOrCreate, isoStore))
        {
          if (stream.Length <= 0) return new DtoContainer();// if no file - will be 0: good for the first app run on a new PC lest throw exception.

          Trace.WriteLine(stream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(stream).ToString()); //Retrieve the actual path of the file using reflection.
          Trace.WriteLine(stream.Name);

          var files = isoStore.GetFileNames(_isoName);
          if (files.Length <= 0) return new DtoContainer();

          foreach (var file in files)
            using (var streamReader = new StreamReader(stream))
            {
              var xcls = (DtoContainer)(new XmlSerializer(typeof(DtoContainer)).Deserialize(streamReader)); //TU: DEserialise from a stream
              streamReader.Close();
              return xcls;
            }
        }
      }
      catch (Exception ex) { ex.Log(); }
      return new DtoContainer();
    }

    //  C:\Users\Alex\AppData\Local\IsolatedStorage\brc4ml45.i4w\pl34ymp1.jar\Url.sh14zs21avtfwfzq03mf5xwzszxgrxid\AssemFiles
#if _DEBUG
    public static IsolatedStorageFile GetIsolatedStorageFile() => IsolatedStorageFile.GetUserStoreForAssembly();           //todo: GetUserStoreForApplication does not work
#else
    public static IsolatedStorageFile GetIsolatedStorageFile()  //  C:\Users\Alex\AppData\Local\IsolatedStorage\brc4ml45.i4w\pl34ymp1.jar\Url.sh14zs21avtfwfzq03mf5xwzszxgrxid\AssemFiles
      => (AppDomain.CurrentDomain.ActivationContext == null) ?
        IsolatedStorageFile.GetMachineStoreForDomain() :        // C:\ProgramData\IsolatedStorage\...			http://stackoverflow.com/questions/72626/could-not-find-file-when-using-isolated-storage				//return IsolatedStorageFile.GetMachineStoreForAssembly(); 			
        IsolatedStorageFile.GetUserStoreForApplication();	      // C:\Users\Alex\AppData\Local\Apps\...		http://stackoverflow.com/questions/202013/clickonce-and-isolatedstorage/227218#227218
#endif

    public class DtoContainer { public string Msg { get; set; } public DateTime Time { get; set; } }
  }
}