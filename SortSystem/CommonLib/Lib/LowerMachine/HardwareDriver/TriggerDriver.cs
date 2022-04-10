using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class TriggerDriver:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public TriggerDriver(COMLink cl) : base(cl)
    {
    }
    public void ApplyChange(Trigger config)
    {
        logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));
        //comlink.send();
        //TODO: do actual config update 
    }
}