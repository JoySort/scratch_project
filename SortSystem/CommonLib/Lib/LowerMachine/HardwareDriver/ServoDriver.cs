using System.Collections;
using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class ServoDriver:DriverBase
{ 
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public ServoDriver(ComLinkDriver cl) : base(cl)
    {
        cl.onServoCMD += onData;
    }

    private bool isStopped = false;
    private bool keepWatching = true;
    private void watcher()
    {
        Task.Run(()=>{
            while (keepWatching)
            {
                if (isStopped)
                {
                    keepWatching = false;
                }
                else
                {
                    sendStopCMDAgin();
                }
            }
        });
    }

    private void sendStopCMDAgin()
    {
        throw new NotImplementedException();
    }

    public void ApplyChange(Servo config)
    {
        keepWatching = true;
        logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));

        //comlink.send();
        //TODO: link to com communication
    }

    public void onData(object sender,byte[] cmd)
    {
        //解析代码
        // if (cmd)
        // {
        //     isStopped = true;
        // }
    }
    
   
}
