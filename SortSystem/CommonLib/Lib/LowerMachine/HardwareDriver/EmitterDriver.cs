using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class EmitterDriver:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public EmitterDriver(COMLink cl) : base(cl)
    {
    }

    private int[] address;

    public int[] Address
    {
        get => address;
        set => address = value;
    }

    public COMLink Comlink
    {
        get => comlink;
        set => comlink = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void trigger()
    {
        logger.Debug("Emitter triggered row:{}-column:{}",address[0],address[1]);
    }
}