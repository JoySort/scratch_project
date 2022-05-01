using System.Collections;
using CameraLib.Lib.ConfigVO;
using CameraLib.Lib.vo;
using NLog;

namespace CameraLib.Lib.LowerMachine.HardwareDriver;

public class VirtualComLinkDriver:ComLinkDriver
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private Project currentProject;
    private bool isProjectRunning;
    
    
    public VirtualComLinkDriver(LowerConfig lowerConfig) : base(lowerConfig)
    {
        
        machineID = "";
        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

  

    public void OnProjectStatusChange(object sender,ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            currentProject = statusEventArgs.currentProject;
            
            isProjectRunning = true;
            startSimulate();
        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse || statusEventArgs.State == ProjectState.washing)
        {
            isProjectRunning = false;
        }
    }

    private int interval = 1000 / 14;
    private long count;
    private void startSimulate()
    {
        count = 0;
        var task = Task.Run(() =>
        {
            while (isProjectRunning)
            {
                
                byte[] intBytes = BitConverter.GetBytes(count);
                //dispatchTriggerEvent(new TriggerEventArg(count));
                OnTriggerCMDFired(intBytes);
                count++;
                Thread.Sleep(count < 14 ? (int)(1000/count):interval);
            }
        });

    }

    public override void init()
    {
        
        logger.Info("ComLink init begin with address:{}",lowerConfig.HardwarePort);
        onMachineIDChanged("Simulator-Machine-ID");
        
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