using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class TriggerDriver:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public TriggerDriver(ComLinkDriver cl) : base(cl)
    {
        cl.OnTrigger += onTriggerEventHandler;
    }
    
 
    public void ApplyChange(Trigger config)
    {
        
        logger.Info(" {} is applying parameters{}",config.Name,JsonConvert.SerializeObject(config));
        //comlink.send();
        //TODO: link to com communication
    }
    
    
    //Trigger声明事件对外的业务，让业务可以知道trigger在走。这个对于虚拟串口和真实串口都有效
    public event EventHandler<TriggerEventArg> OnTrigger;
    private void onTriggerEventHandler(object sender,TriggerEventArg e)
    {
        var handler = OnTrigger;
        handler?.Invoke(this, e);
    }
    
    //这个方法目前没有什么用，是给Trigger自己用来触发trigger事件的，但是目前想不到什么有用的场景。先放在这里
    public void dispatchTriggerEvent(TriggerEventArg e)
    {
        onTriggerEventHandler(this, e);
    }
}

