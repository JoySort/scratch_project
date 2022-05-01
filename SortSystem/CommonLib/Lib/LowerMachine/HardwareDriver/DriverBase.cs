using CommonLib.Lib.Util;
using CommonLib.Lib.ConfigVO;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public abstract class  DriverBase
{
    internal ComLinkDriver comlink;
    
    public DriverBase(ComLinkDriver cl)
    {
        this.comlink = cl;
    }


    public virtual  void onData(object sender, byte[] cmd)
    {
        throw new NotImplementedException();
    }

    public virtual void ApplyChange(IHardwareconfig config)
    {
        throw new NotImplementedException();
    }
}