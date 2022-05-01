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

    public void sendStopCMD()
    {
        byte[] addr = new byte[2] { 0x00, 0x98 };
        byte[] data = new byte[2] { 0x00, 0x00 };
        comlink.writeSingleReg(addr, data, 0, 0);
    }

    public void sendStartCMD()
    {
        byte[] addr = new byte[2] { 0x00, 0x98 };
        byte[] data = new byte[2] { 0x00, 0x01 };
        comlink.writeSingleReg(addr, data, 0, 0);

    }

    public void ApplyChange(StepMotoer config)
    {
        //logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));
  
        if (config.Enabled)
        {
            sendStartCMD();
        }
        else
        {            
            sendStopCMD();
        }
    }
    
    public override void onData(object obj, byte[] cmd)
    {        
    }
}