using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class StepMotorDriver:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public StepMotorDriver(COMLink cl) : base(cl)
    {
    }
    
    public void ApplyChange(StepMotoer config)
    {
        logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));
        //comlink.send();
        //TODO: do actual config update 
    }
}