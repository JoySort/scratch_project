using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class EmitterDriver:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public EmitterDriver(ComLinkDriver cl) : base(cl)
    {
    }

    private int[] address;

    public int[] Address
    {
        get => address;
        set => address = value;
    }

    public ComLinkDriver Comlink
    {
        get => comlink;
        set => comlink = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void Emit(int column,int outletNo)
    {
        logger.Debug("Emitter triggered row:{}-column:{}",column,outletNo);
        //TODO: link to com communication
    }
  
}