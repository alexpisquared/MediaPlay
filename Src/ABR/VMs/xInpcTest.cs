using ABR.Model;
using MVVM.Common;
namespace ABR.VMs
{
    public partial class AbrVM : ViewModelBase
  {
    InpcTestFody _InpcTestFody = new InpcTestFody();
    InpcTestImpl _InpcTestImpl = new InpcTestImpl(); 
    InpcTestBase _InpcTestBase = new InpcTestBase();
    InpcTestNone _InpcTestNone = new InpcTestNone();

    public InpcTestFody InpcTestFody { get => _InpcTestFody; set => _InpcTestFody = value; }
    public InpcTestImpl InpcTestImpl { get => _InpcTestImpl; set => _InpcTestImpl = value; }
    public InpcTestBase InpcTestBase { get => _InpcTestBase; set => _InpcTestBase = value; }
    public InpcTestNone InpcTestNone { get => _InpcTestNone; set => _InpcTestNone = value; }
  }
}