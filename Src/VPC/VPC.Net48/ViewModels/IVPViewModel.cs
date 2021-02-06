using VPC.Models;

namespace VPC.ViewModels
{
	public interface IVPViewModel
	{
		VPModel VPModel {get;set;}
		void PlayNewFile(string mediaFile);
		void PlayNewFileOrFolder(string mediaFileOrFolder);
        void FlashAllControlls();
	}
}
