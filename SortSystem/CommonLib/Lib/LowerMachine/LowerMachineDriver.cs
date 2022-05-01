using CameraLib.Lib.ConfigVO;
using CameraLib.Lib.LowerMachine.HardwareDriver;
using CameraLib.Lib.Util;
using NLog;

namespace CameraLib.Lib.LowerMachine;

public class LowerMachineDriver
{
    
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private List<ComLinkDriver> comLinks = new List<ComLinkDriver>();
    public List<ServoDriver> servos = new List<ServoDriver>();
    private List<StepMotorDriver> stepMotors = new List<StepMotorDriver>();
    private List<SwitchDriver> switches = new List<SwitchDriver>();
    public List<TriggerDriver> triggers = new List<TriggerDriver>();
    private EmitterDriver[][] emitters;
    public List<AdvancedEmitterDrive> advancedEmitter=new List<AdvancedEmitterDrive>();
    
    
     private LowerMachineDriver()
    {
        setupHardwareLink();
    }

     private static LowerMachineDriver instance = new LowerMachineDriver();

     public static LowerMachineDriver getInstance()
     {
         return instance;
     }
     

     private void setupHardwareLink()
    {
        LowerConfig[] lcs = ConfigUtil.getModuleConfig().LowerConfig;
        setupEmitters(lcs);
        //setuComLink and hardware must be latter than emitters;
        setupComLink(lcs);

        foreach (var stateConfig in ConfigUtil.getMachineState())
        {
            if (stateConfig.IsDefault)
            {
                applyStateChange( stateConfig.StateName);
            }
        }

    }

    private void setupComLink(LowerConfig[] lcs)
    {
        foreach (var lc in lcs)
        {            
            ComLinkDriver comLinkDriver = !ConfigUtil.getModuleConfig().LowerMachineSimulationMode? new ComLinkDriver(lc):new VirtualComLinkDriver(lc);
            comLinkDriver.onMachineID += onMachineIDEventHandler;
            comLinkDriver.init();
            setupHardwares(comLinkDriver);
            comLinks.Add(comLinkDriver);
        }
    }

    public string machineID { get; set; }

    public void onMachineIDEventHandler(object sender, string machineID)
    {
        ComLinkDriver cl = (ComLinkDriver) sender;
        if(cl.LowerConfig.IsMaster)
            this.machineID = machineID;
        ConfigUtil.getModuleConfig().MachineID = machineID;
    }

    private void setupHardwares(ComLinkDriver comLinkDriver)
    {
        if(comLinkDriver.LowerConfig.IsMaster){
            servos.Add(new ServoDriver(comLinkDriver));
            stepMotors.Add((new StepMotorDriver(comLinkDriver)));
            switches.Add((new SwitchDriver(comLinkDriver)));
            
        }
        var triggerDriver = new TriggerDriver(comLinkDriver);
        triggers.Add(triggerDriver);


        var rows = ConfigUtil.getEmitters();
        var columns = comLinkDriver.LowerConfig.Columns;
        
        
        for (var ri = 0; ri < rows.Length; ri++)
        {
            for (var ci = columns.First(); ci <= columns.Last(); ci++)
            {
                var emiiter = new EmitterDriver(comLinkDriver);
                emiiter.Address = new int[] {ri, ci};
                emitters[ri][ci]=emiiter;
            }
        }

        advancedEmitter.Add(new AdvancedEmitterDrive(comLinkDriver));
    }
   
    private void setupEmitters(LowerConfig[] lcs)
    {

        var rows = ConfigUtil.getEmitters();
        
        var totalColumnCount = 0;
        foreach (var lc in lcs)
        {
            if (totalColumnCount < lc.Columns.Last())
                totalColumnCount = lc.Columns.Last();
        }
        
        totalColumnCount = totalColumnCount + 1;
        emitters = new EmitterDriver[rows.Length][];
        for (var ri = 0; ri < rows.Length; ri++)
        {
            if (emitters[ri] == null)
            {
                emitters[ri] = new EmitterDriver[totalColumnCount];
            }
        }

    }

    public void applyStateChange( ProjectState state)
    {
        if(state== ProjectState.start)
        {
            foreach (var trigger in triggers)
            {
                trigger.resetCounter();
            }
        }

        MachineState[] machineStates = ConfigUtil.getMachineState();
        foreach (var stateConfig in machineStates)
        {
            if (stateConfig.StateName == state)
            {
                //servo
                if(stateConfig.State.Servos!=null){
                    foreach (var item in stateConfig.State.Servos)
                    {
                        foreach (var driver in servos)
                        {
                            driver.ApplyChange(item);
                        }
                    }
                }

                if (stateConfig.State.StepMotoers != null)
                {
                    //stepMotor
                    foreach (var item in stateConfig.State.StepMotoers)
                    {
                        foreach (var driver in stepMotors)
                        {
                            driver.ApplyChange(item);
                        }
                    }
                }

                if (stateConfig.State.Switches != null)
                {
                    //Switch
                    foreach (var item in stateConfig.State.Switches)
                    {
                        foreach (var driver in switches)
                        {
                            driver.ApplyChange(item);
                        }
                    }
                }

                if (stateConfig.State.Triggers != null)
                {
                    //trigger
                    foreach (var item in stateConfig.State.Triggers)
                    {
                        foreach (var driver in triggers)
                        {
                            driver.ApplyChange(item);
                        }
                    }
                }
            }
        }
    }
    
    public void setupTriggerEventListener(ProjectState state,EventHandler<TriggerEventArg>onTrigger)
    {
        //setup trigger event handler;
        foreach (var trigger in triggers)
        {
            if (state == ProjectState.start)
            {
                trigger.OnTrigger += onTrigger;
            }
            else
            {
                trigger.OnTrigger -= onTrigger;
            }
        }
    }

    public void StartRunning()
    {
        foreach (var trigger in triggers)
        {
            trigger.resetCounter();
        }

        foreach (var servo in servos)
        {
            servo.sendStartCMD();
        }

        foreach (var step in stepMotors)
        {
            step.sendStartCMD();
        }
    }

    public void StopRunning()
    {
        
        foreach (var servo in servos)
        {
            servo.Stop();
        }

        foreach (var step in stepMotors)
        {
            step.sendStopCMD();
        }
    }
}