using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.Exception;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.Worker.Upper;

public class EmitWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static EmitWorker instance = new();
    private Project currentProject;
    private bool isProjectRunning;
    private int sortingInterval;
    private List<LBResult> toBeProcessedResults = new();

    private EmitWorker()
    {
        ProjectEventDispatcher.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

    public static EmitWorker getInstance()
    {
        return instance;
    }

    public void processSingle(LBResult result)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("Emitworker:");
        toBeProcessedResults.Add(result);
    }

    public void processBulk(List<LBResult> results)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("Emitworker:");
        toBeProcessedResults.AddRange(results);
        processResult();
    }

    public void OnProjectStatusChange(object sender, ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            currentProject = statusEventArgs.currentProject;
            prepareConfig();
            isProjectRunning = true;
            //processResult();
        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse ||
            statusEventArgs.State == ProjectState.washing) isProjectRunning = false;
    }

    private void prepareConfig()
    {
        sortingInterval = ConfigUtil.getModuleConfig().SortConfig.SortingInterval;
    }

    private void processResult()
    {
       // Task.Run(() =>
       // {
            //logger.Info("EmiitWorker starts process project id {} project name {} ", currentProject.Id,currentProject.Name);

           // while (isProjectRunning)
           // {
               // Thread.Sleep(sortingInterval);
                var processBatch = toBeProcessedResults;
                //if (processBatch.Count == 0) continue;
                toBeProcessedResults = new List<LBResult>();

                var emitResults = new List<EmitResult>();
                foreach (var item in processBatch)
                    emitResults.Add(new EmitResult(item.Coordinate.Column, int.Parse(item.LoadBalancedOutlet.First().ChannelNo),item.Coordinate.TriggerId));
                DispatchResultEvent(new EmitResultEventArg(emitResults));

                
           // }

            //logger.Info("EmiitWorker stops process project id {} project name {} ", currentProject.Id,currentProject.Name);
       // });
    }

    public event EventHandler<EmitResultEventArg> OnResult;

    protected virtual void DispatchResultEvent(EmitResultEventArg e)
    {
        var handler = OnResult;
        handler?.Invoke(this, e);
    }
}

public class EmitResultEventArg
{
    public List<EmitResult> Results;

    public EmitResultEventArg(List<EmitResult> results)
    {
        Results = results;
    }
}