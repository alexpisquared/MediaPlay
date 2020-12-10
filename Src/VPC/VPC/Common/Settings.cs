
namespace VPC.Common
{
	public class AppSettings
	{
		public int IValue { get; set; }
		public double windowLeft = 200;
		public double windowTop = 200;
		public double windowWidth = 960;
		public double windowHeight = 540;

		public int PlayerMargin { get; set; }
	}


	public class AppSettingsEx
	{
		public int IValue { get; set; }
		WindowPlace window1 = new WindowPlace(); public WindowPlace Window1 { get { return window1; } set { window1 = value; } }
		WindowPlace window2 = new WindowPlace(); public WindowPlace Window2 { get { return window2; } set { window2 = value; } }
		WindowPlace window3 = new WindowPlace(); public WindowPlace Window3 { get { return window3; } set { window3 = value; } }
	}

	public class WindowPlace
	{
		public double windowLeft = 200;
		public double windowTop = 200;
		public double windowWidth = 960;
		public double windowHeight = 540;
	}
}
