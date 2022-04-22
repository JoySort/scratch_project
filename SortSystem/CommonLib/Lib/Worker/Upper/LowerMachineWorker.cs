using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.LowerMachine.HardwareDriver;
using CommonLib.Lib.Sort.Exception;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.Worker.Upper;
/**
 * <summary>用来操作下位机，所有二进制指令都应该写在driver类里面</summary>
 */
public class LowerMachineWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool isProjectRunning = false;
    private Project currentProject;
    private LowerMachineDriver lowerMachineDriver;

    public LowerMachineDriver LowerMachineDriver => lowerMachineDriver;

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
        lowerMachineDriver =  LowerMachineDriver.getInstance();
        ProjectManager.getInstance().ProjectStatusChanged += OnSwitchProjectState;
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
        processResult();
    }

    //Auto invoke when lower machine trigger send event. @see TriggerDriver
    private void onTrigger(object sender, TriggerEventArg args)
    {
         this.currentTriggerId = args.TriggerId;
         //logger.Debug("Trigger id in LowerMachine {}",this.currentTriggerId);
       
    }

    public void OnSwitchProjectState(object sender,ProjectStatusEventArgs statusEventArgs)
    {
        
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            logger.Info("Lower Machine start to switch to start state");
            prepareConfig(statusEventArgs.currentProject);
            
            lowerMachineDriver.applyStateChange( statusEventArgs.State);

        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse || statusEventArgs.State == ProjectState.washing)
        {
            logger.Info("Lower Machine start to switch to none-start state");
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

    private long counter = 0;
    private void processResult()
    {
        //Task.Run(()=>{
         //   while (isProjectRunning)
        //    {
        //        Thread.Sleep(currentInterval);
                counter++;
                //logger.Info("current lowermachine running cycle counter {} from lower machine{} isProjectRunning{} currentInterval {} currentProject {} ",counter,this.currentTriggerId,isProjectRunning,currentInterval,currentProject);
                var tmpBatch = toBeProcessedResults;
               // if (tmpBatch.Count <= 0) continue;
                toBeProcessedResults = new List<EmitResult>();
                logger.Debug("LowerMachine AdvancedEmitter count{}",tmpBatch.Count);
                lowerMachineDriver.advancedEmitter.EmitBulk(tmpBatch);

                // }

                // });
    }

    public static void init()
    {
        getInstance();
    }
}