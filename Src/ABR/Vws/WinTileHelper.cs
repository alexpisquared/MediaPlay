using ABR.VMs;
using ABR.Vws;
using AsLink;
using MVVM.Common;
using System;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsLink
{
  public class WinTileHelper
  {
    public static async Task<string> PinTile(object sender)
    {
      string rv = "Nothing to report";
      const string hd = "Windows.Phone.UI.Input.HardwareButtons";
      try
      {
        // Prepare package images for all four tile sizes in our tile to be pinned as well as for the square30x30 logo used in the Apps view.  
        var square150x150Logo = new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png");
        var square310x310Logo = new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png");
        var wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.scale-200.png");
        var square30x30Logo = new Uri("ms-appx:///Assets/Square44x44Logo.targetsize-24_altform-unplated.png");

        // Create a Secondary tile with all the required arguments.
        // Note the last argument specifies what size the Secondary tile should show up as by default in the Pin to start fly out.
        // It can be set to TileSize.Square150x150, TileSize.Wide310x150, or TileSize.Default.  
        // If set to TileSize.Wide310x150, then the asset for the wide size must be supplied as well.
        // TileSize.Default will default to the wide size if a wide size is provided, and to the medium size otherwise. 
        var st = new SecondaryTile("SecondaryTile.Logo", $"Pin @{DateTime.Now:yy-MM-dd}", "???", square150x150Logo, TileSize.Square150x150);

        if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent((hd))))
        {
          st.VisualElements.Wide310x150Logo = wide310x150Logo;
          st.VisualElements.Square310x310Logo = square310x310Logo;
        }

        st.VisualElements.Square30x30Logo = square30x30Logo;  // Like the background color, the square30x30 logo is inherited from the parent application tile by default. Let's override it, just to see how that's done.
        st.VisualElements.ShowNameOnSquare150x150Logo = true; // The display of the secondary tile name can be controlled for each tile size. The default is false.

        if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent((hd))))
        {
          st.VisualElements.ShowNameOnWide310x150Logo = true;
          st.VisualElements.ShowNameOnSquare310x310Logo = true;
        }

        //st.VisualElements.ForegroundText = ForegroundText.Light;  // The tile background color is inherited from the parent unless a separate value is specified.
        //st.RoamingEnabled = false;                                // Set this to false if roaming doesn't make sense for the secondary tile.

        if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent((hd)))) // OK, the tile is created and we can now attempt to pin the tile.
        {
          var isPinned = await st.RequestCreateForSelectionAsync(GetElementRect((FrameworkElement)sender), Windows.UI.Popups.Placement.Below);
          rv = isPinned ? "Secondary tile successfully pinned." : "Secondary tile not pinned.";
        }

        if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent((hd)))
        {
          // OK, the tile is created and we can now attempt to pin the tile.
          // Since pinning a secondary tile on Windows Phone will exit the app and take you to the start screen, any code after RequestCreateForSelectionAsync or RequestCreateAsync is not guaranteed to run.  
          // For an example of how to use the OnSuspending event to do work after RequestCreateForSelectionAsync or RequestCreateAsync returns, see Scenario9_PinTileAndUpdateOnSuspend in the SecondaryTiles.WindowsPhone project.
          await st.RequestCreateAsync();
        }

      }
      catch (Exception ex) { rv = ex.Message; /*ApplicationData.Current.LocalSettings.Values[Shared.ExnDetail] += DevOp.ExHrT(ex, "ctor");*/ }

      return rv;
    }

    static Rect GetElementRect(FrameworkElement element)
    {
      Windows.UI.Xaml.Media.GeneralTransform transform = element.TransformToVisual(null);
      Point point = transform.TransformPoint(new Point());
      return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
    }
  }
}
