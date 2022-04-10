using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class ServoDriver:DriverBase
{ 
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public ServoDriver(COMLink cl) : base(cl)
    {
    }

    public void ApplyChange(Servo config)
    {
        logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));
        //comlink.send();
        //TODO: do actual config update 
    }
}