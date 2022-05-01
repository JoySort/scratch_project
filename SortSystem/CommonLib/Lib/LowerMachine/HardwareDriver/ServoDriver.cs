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
        //在串口添加数据事件，以便在串口收到事件的时候进行操作。比如启动确认逻辑，停止确认逻辑等等。
        cl.onServoCMD += onData;
    }

    private bool isStopped = false;
    private bool keepWatching = true;
    private void watcher()
    {
        Task.Run(()=>{
            while (keepWatching)
            {
                Thread.Sleep(3000);
                if (isStopped)
                {
                    keepWatching = false;
                }
                else
                {
                    
                    sendStopCMD();
                    
                }
            }
        });
    }

    public void sendStopCMD()
    {
        byte[] addr= new byte[2] { 0x00, 0x50 };
        byte[] data = new byte[2] { 0x00, 0x00 };
        comlink.writeSingleReg(addr, data, 0, 200);        
    }

    public void sendStartCMD()
    {
        byte[] addr = new byte[2] { 0x00, 0x50 };
        byte[] data = new byte[2] { 0x01, 0x00 };
        comlink.writeSingleReg(addr, data, 0, 200);

    }

    public void Start()
    {
        sendStartCMD();
    }

    public void MoveOneSlot()
    {
        byte[] addr = new byte[2] { 0x00, 0x51 };
        byte[] data = new byte[2] { 0x00, 0x01 };
        comlink.writeSingleReg(addr, data, 0, 200);
    }

    public void Stop()
    {
        isStopped = false;
        sendStopCMD();
        Thread thread = new Thread(watcher);
        thread.Start();
    }

    public void ApplyChange(Servo config)
    {
        keepWatching = true;
        //logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));
        if (config.Enabled)
        {
            sendStartCMD();
        }
        else
        {
            Stop();
                
        }        
    }

    public override void onData(object sender,byte[] cmd)
    {
        //解析代码
        // if (cmd)
        // {
        if(cmd[5]==0)
            isStopped = true;
        // }
    }
    
   
}
