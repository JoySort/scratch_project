using CommonLib.Lib.Util;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class DriverBase
{
    internal ComLinkDriver comlink;
    
    public DriverBase(ComLinkDriver cl)
    {
        this.comlink = cl;
    }
 
}