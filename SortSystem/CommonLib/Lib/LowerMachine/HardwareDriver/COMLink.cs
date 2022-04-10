using System.Collections;
using CommonLib.Lib.ConfigVO;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class COMLink
{

    private LowerConfig lowerConfig;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public COMLink(LowerConfig lowerConfig)
    {
        this.lowerConfig = lowerConfig;
    }

    public LowerConfig LowerConfig
    {
        get => lowerConfig;
        set => lowerConfig = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void init()
    {
        //TODO: implement actual link with lowerconfig 
        logger.Info("ComLink init begin with address:{}",lowerConfig.HardwarePort);
        var comPort = lowerConfig.HardwarePort;
        
    }

    public void send(BitArray cmd)
    {
        //TODO: send cmd to lowermachine
    }

    public void invoke()
    {
        //TODO: when recieve lower machine communication notify other interested party
    }
}