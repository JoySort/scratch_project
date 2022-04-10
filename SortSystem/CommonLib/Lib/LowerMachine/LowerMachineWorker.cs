using System.Collections;
using System.Reflection.PortableExecutable;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.ConfigVO.Emission;
using CommonLib.Lib.LowerMachine.HardwareDriver;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.LowerMachine;

public class LowerMachineWorker
{

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private List<COMLink> comLinks = new List<COMLink>();
    private List<ServoDriver> servos = new List<ServoDriver>();
    private List<StepMotorDriver> stepMotors = new List<StepMotorDriver>();
    private List<SwitchDriver> switches = new List<SwitchDriver>();
    private List<TriggerDriver> triggers = new List<TriggerDriver>();
    private EmitterDriver[][] emitters;
    
    private static readonly LowerMachineWorker instance = new LowerMachineWorker();
    public static void init()
    {
        
        ProjectEventDispatcher.getInstance().ProjectStatusChanged += instance.OnSwitchProjectState;
    }

    public LowerMachineWorker()
    {
        setupHardwareLink();
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
            COMLink comLink = new COMLink(lc);
            comLink.init();
            setupHardwares(comLink);
            comLinks.Add(comLink);
        }
    }

    private void setupHardwares(COMLink comLink)
    {
        if(comLink.LowerConfig.IsMaster){
            servos.Add(new ServoDriver(comLink));
            stepMotors.Add((new StepMotorDriver(comLink)));
            switches.Add((new SwitchDriver(comLink)));
            triggers.Add(new TriggerDriver(comLink));
        }  
            
       
        var rows = ConfigUtil.getEmitters();
        var columns = comLink.LowerConfig.Columns;
        
        
        for (var ri = 0; ri < rows.Length; ri++)
        {
            for (var ci = columns.First(); ci <= columns.Last(); ci++)
            {
                var emiiter = new EmitterDriver(comLink);
                emiiter.Address = new int[] {ri, ci};
                emitters[ri][ci]=emiiter;
            }
        }
    }

    private void setupEmitters(LowerConfig[] lcs)
    {

        var rows = ConfigUtil.getEmitters();
        
        var totalColumnCount = 0;
        foreach (var lc in lcs)
        {
            if (totalColumnCount < lc.Columns.Last()) totalColumnCount = lc.Columns.Last();
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

    public void OnSwitchProjectState(object sender,ProjectStatusEventArgs statusEventArgs)
    {
        
        switch (statusEventArgs.State)
        {
            case ProjectState.start:
                logger.Info("Lower Machine start to switch to start state");
                //TODO: do something else
                applyStateChange( statusEventArgs.State);
            break;
            case ProjectState.stop:
                logger.Info("Lower Machine start to switch to stop state");
                //TODO: do something else
                applyStateChange( statusEventArgs.State);
                break;
            default:
                break;
        }
    }
    
 

    private void applyStateChange( ProjectState state)
    {
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
    

}