using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class StepMotorDriver:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public StepMotorDriver(ComLinkDriver cl) : base(cl)
    {
        cl.onStepMotorrCMD += onData;
    }
    
    public void ApplyChange(StepMotoer config)
    {
        //logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));
  
        //comlink.send();
        //TODO: link to com communication
    }
    
    public void onData(object obj, byte[] cmd)
    {
        //解析代码
        // if (cmd)
        // {
        //     var isStopped = true;
        // }
    }
}