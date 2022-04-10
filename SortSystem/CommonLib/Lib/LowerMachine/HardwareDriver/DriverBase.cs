namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class DriverBase
{
    internal COMLink comlink;

    public DriverBase(COMLink cl)
    {
        this.comlink = cl;
    }
    
}