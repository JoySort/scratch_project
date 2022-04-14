using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class ServoDriver:DriverBase
{ 
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public ServoDriver(ComLinkDriver cl) : base(cl)
    {
    }

    public void ApplyChange(Servo config)
    {
        logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));

        //comlink.send();
        //TODO: link to com communication
    }
}