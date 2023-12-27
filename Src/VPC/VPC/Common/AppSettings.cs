namespace AsLink;

public partial class AppSettings
{
  public int IValue { get; set; }
  public double windowLeft = 200;
  public double windowTop = 200;
  public double windowWidth = 960;
  public double windowHeight = 540;

  public int PlayerMargin { get; set; }

  public string AppSetting { get; internal set; } = "";
  public string LastVideo { get; internal set; } = "";
}
