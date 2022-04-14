using CommonLib.Lib.LowerMachine.HardwareDriver;
using CommonLib.Lib.Sort.Exception;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.LowerMachine;
/**
 * <summary>用来操作下位机，所有二进制指令都应该写在driver类里面</summary>
 */
public class LowerMachineWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool isProjectRunning = false;
    private Project currentProject;
    private LowerMachineDriver lowerMachineDriver;
    private static readonly LowerMachineWorker instance = new();
    private List<EmitResult> toBeProcessedResults = new();
    private int currentInterval;
    private long currentTriggerId;

    public static LowerMachineWorker getInstance()
    {

        return instance;
    }
    
    
    private LowerMachineWorker()
    {
        ProjectEventDispatcher.getInstance().ProjectStatusChanged += OnSwitchProjectState;
        lowerMachineDriver =  LowerMachineDriver.getInstance();
    }

   

    public void processSingle(EmitResult result)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("LowerMachine:");
        toBeProcessedResults.Add(result);
    }

    public void processBulk(List<EmitResult> results)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("LowerMachine:");
        toBeProcessedResults.AddRange(results);
    }

    //Auto invoke when lower machine trigger send event. @see TriggerDriver
    private void onTrigger(object sender, TriggerEventArg args)
    {
         this.currentTriggerId = args.triggerID;
       
    }

    public void OnSwitchProjectState(object sender,ProjectStatusEventArgs statusEventArgs)
    {
        
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            logger.Info("Lower Machine start to switch to start state");
            prepareConfig(statusEventArgs.currentProject);
            
            lowerMachineDriver.applyStateChange( statusEventArgs.State);
            
            processResult();
            
            
          
        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse || statusEventArgs.State == ProjectState.washing)
        {
            logger.Info("Lower Machine start to switch to none start state");
            lowerMachineDriver.applyStateChange( statusEventArgs.State);
            isProjectRunning = false;
            toBeProcessedResults = new List<EmitResult>();
        }
        
        lowerMachineDriver.setupTriggerEventListener(statusEventArgs.State,onTrigger);
    }

    private void prepareConfig(Project p)
    {
        this.isProjectRunning = true;
        this.currentProject =p;
        this.currentInterval = ConfigUtil.getModuleConfig().SortConfig.SortingInterval;
    }

    private void processResult()
    {
        Task.Run(()=>{
            while (isProjectRunning)
            {
                logger.Info("current trigger id from lower machine{}",this.currentTriggerId);
                var tmpBatch = toBeProcessedResults;
                toBeProcessedResults = new List<EmitResult>();
                lowerMachineDriver.advancedEmitter.EmitBulk(tmpBatch);
                Thread.Sleep(currentInterval);
            }
            
        });
    }
}