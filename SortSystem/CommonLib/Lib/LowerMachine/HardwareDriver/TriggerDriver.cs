using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class TriggerDriver:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public TriggerDriver(ComLinkDriver cl) : base(cl)
    {
        cl.OnTriggerCMD += onData;
    }

    public void resetCounter()
    {
        byte[] addr = new byte[2] { 0x00, 0x4e };
        byte[] data = new byte[2] { 0x00, 0x00 };
        comlink.writeSingleReg(addr, data, 0, 10);
    }

    private int triggerCount = 0;
    public int TriggerCount => triggerCount;

    public void ApplyChange(Trigger config)
    {
        
        //logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));
        //comlink.send();
        //TODO: link to com communication
    }
    
    public void onData(object obj, byte[] cmd)
    {
        //解析代码
        // if cmd contains trigger id , fireTriggerEvent(this, new TriggerEventArg(triggerID))
        // for simulator
        if (obj is VirtualComLinkDriver) fireSimulationTriggerID(cmd);
        triggerCount=(cmd[3]<<24)+(cmd[4]<<16)+(cmd[5]<<8)+(cmd[6]);

    }
    public void getTirggerCount()
    {
        byte[] addr = new byte[2] { 0x00, 0x9c };        
        comlink.readReg(addr,2, 0, 20);
    }
    
    //Trigger声明事件对外的业务，让业务可以知道trigger在走。这个对于虚拟串口和真实串口都有效
    public event EventHandler<TriggerEventArg> OnTrigger;
    private void fireTriggerEvent(object sender,TriggerEventArg e)
    {
        var handler = OnTrigger;
        handler?.Invoke(this, e);
    }

    private void fireSimulationTriggerID(byte[] cmd)
    {
        fireTriggerEvent(this, new TriggerEventArg(BitConverter.ToInt64(cmd)));
    }
}

public class TriggerEventArg
{
    private long triggerID;

    public TriggerEventArg(long triggerId)
    {
        triggerID = triggerId;
    }

    public long TriggerId
    {
        get => triggerID;
        set => triggerID = value;
    }
}

