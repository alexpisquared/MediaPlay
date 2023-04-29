
namespace VPC.Common;

public class AppSettingsEx
{
  public int IValue { get; set; }
  public WindowPlace Window1 { get; set; } = new WindowPlace();
  WindowPlace window2 = new(); public WindowPlace Window2 { get => window2; set => window2 = value; }

  public WindowPlace Window3 { get; set; } = new WindowPlace();
}
