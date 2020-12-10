using EF7UwpPoc.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace EF7UwpPoc
{
    public sealed partial class MainPage : Page
    {
        DadosContext _db = new DadosContext();
        public MainPage() { this.InitializeComponent(); }

        async void onAddOne(object sender, RoutedEventArgs e)
        {
            //foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) Debug.WriteLine("  {0} = {1}", de.Key, de.Value);

            _db.Pessoa.Add(new Pessoa { Name = $"TIAGO {DateTime.Now:MMMdd-HHmm}", Dscr = NetworkInformation.GetHostNames().FirstOrDefault(name => name.Type == HostNameType.DomainName)?.DisplayName ?? "???" });

            await _db.SaveChangesAsync();

            var l = _db.Pessoa.ToList();
            b1.Content = l[0].Name;
        }
        async void onGetAll(object sender, RoutedEventArgs e)
        {
            var l = _db.Pessoa.ToList();
            h1.Content = _db.DbPath;
            h1.NavigateUri = new Uri(_db.DbPath);

            t1.Text = $"{_db.DbPath}\r\n\r\n Total:\t{l.Count}.\r\n First:\t{l.First().Name}, \t{l.First().Dscr}.\r\n Last:\t{l.Last().Name}, \t{l.Last().Dscr}.";

            var used = await GetRoamingFolderSizeKBFromFiles();
            t2.Text = $"Roaming Quota: Total {ApplicationData.Current.RoamingStorageQuota} KB  Used {used} KB";
        }

        void h1_Click(object sender, RoutedEventArgs e)
        {
            var rr = ((Windows.UI.Xaml.Controls.HyperlinkButton)e.OriginalSource).NavigateUri;
            var d = new DataPackage();
            d.SetText(rr.AbsolutePath);
            Clipboard.SetContent(d); 
            // Process.Start(new ProcessStartInfo(Path.GetDirectoryName(e...)));
        }

        async Task<double> GetRoamingFolderSizeKBFromFiles() //ap: https://github.com/AndrewJByrne/Clearing-Windows-AppData/blob/master/DebugSettings/DebugFlyout.xaml.cs
        {
            ulong total = 0;
            foreach (var file in await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery))
            {
                var basicProperties = await file.GetBasicPropertiesAsync();
                total += basicProperties.Size;
            }

            return total / 1000d;
        }
    }
}
