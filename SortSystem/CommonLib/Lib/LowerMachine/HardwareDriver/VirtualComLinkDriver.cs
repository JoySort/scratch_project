using System.Collections;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class VirtualComLinkDriver:ComLinkDriver
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private Project currentProject;
    private bool isProjectRunning;
    
    
    public VirtualComLinkDriver(LowerConfig lowerConfig) : base(lowerConfig)
    {
        ProjectEventDispatcher.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

    public void OnProjectStatusChange(object sender,ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            this.currentProject = statusEventArgs.currentProject;
            
            this.isProjectRunning = true;
            startSimulate();
        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse || statusEventArgs.State == ProjectState.washing)
        {
            isProjectRunning = false;
        }
    }

    private int interval = 1000 / 14;
    private long count = 0;
    private void startSimulate()
    {
        count = 0;
        var task = Task.Run(() =>
        {
            while (isProjectRunning)
            {


                dispatchTriggerEvent(new TriggerEventArg(count));
                count++;
                Thread.Sleep(count < interval ? (int)count:interval);
            }
        });

    }

    public void init()
    {
        //TODO: implement actual link with lowerconfig 
        logger.Info("ComLink init begin with address:{}",lowerConfig.HardwarePort);
        var comPort = lowerConfig.HardwarePort;
        
    }

    public void send(BitArray cmd)
    {
        //Do Nothing
    }

    public void invoke()
    {
        //TODO: when recieve lower machine communication notify other interested party
    }
}