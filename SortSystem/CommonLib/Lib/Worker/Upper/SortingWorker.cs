using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.Exception;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.Worker.Upper;

public class SortingWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static SortingWorker worker = new SortingWorker();

    private Project currentProject;
    private Outlet[] currentOutlets;

    private bool isProjectRunning;
    private int sortingInterval;
    private List<ConsolidatedResult> toBeProcessedResults = new List<ConsolidatedResult>();
    private List<SortResult> sortResults;

    private SortingWorker()
    {
        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

    public void OnProjectStatusChange(object sender,ProjectStatusEventArgs statusEventArgs)
    {
        if ((statusEventArgs.State == ProjectState.start || statusEventArgs.State == ProjectState.update ) && statusEventArgs.currentProject != null)
        {
            this.currentProject = statusEventArgs.currentProject;
            prepareConfig();
            this.isProjectRunning = true;
            //processResult();
        }
        

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse || statusEventArgs.State == ProjectState.washing)
        {
            isProjectRunning = false;
        }
    }

    private void prepareConfig()
    {
        var outlets = currentProject.Outlets;
        // priority 
        var priority = ConfigUtil.getModuleConfig().SortConfig.OutletPriority;
        if (priority == OutletPriority.DESC)
        {
            outlets = outlets.OrderByDescending(outlet => outlet.ChannelNo).ToArray();
        }
        else
        {
            outlets = outlets.OrderBy(outlet => outlet.ChannelNo).ToArray();
        }

        this.sortingInterval = ConfigUtil.getModuleConfig().SortConfig.SortingInterval;
        this.currentOutlets = outlets;
    }

    public static SortingWorker getInstance()
    {
        return worker;
    }

    public void processSingle(ConsolidatedResult result)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("SortingWorker:");
        toBeProcessedResults.Add(result);
    }

    public void processBulk(List<ConsolidatedResult> consolidatedResults)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("SortingWorker:");
        toBeProcessedResults.AddRange(consolidatedResults);
        processResult();
    }


    private void processResult()
    {
        //Task.Run(() =>
       // {
            //logger.Info("SortingWorker starts process project id {} project name {} ",currentProject.Id,currentProject.Name);

           // while (isProjectRunning)
          //  {
               // Thread.Sleep(sortingInterval);
                var processBatch = toBeProcessedResults;
               // if (processBatch.Count == 0) continue;
                
                toBeProcessedResults = new List<ConsolidatedResult>();
                sortResults = new List<SortResult>();
                foreach (var item in processBatch)
                {
                    applySortingRules(item);
                }
                logger.Debug("Sortingworker with count {} ",sortResults.Count);
                if(sortResults.Count>0)
                DispatchResultEvent(new SortingResultEventArg(sortResults));
                
               
           // }
        //    logger.Info("SortingWorker stops process project id {} project name {} ",currentProject.Id,currentProject.Name);
       // });
    }

   
    
    private void applySortingRules(ConsolidatedResult consolidatedResult)
    {
        var selectedOutlets = new List<Outlet>();
        foreach (var outlet in currentOutlets)
        {   
            var oRResult = false;

            foreach (var OrFilterGroup in outlet.Filters)// Or relationship
            {

                var andResult = true;
                foreach (var andFilter in OrFilterGroup) //And relationship
                {
                    andResult = andResult && andFilter.doFilter(consolidatedResult);
                    if (andResult){
                        var filterBoundaries = andFilter.FilterBoundaries;
                    }
                    
                }
               
                oRResult = oRResult || andResult;
            }

            if (oRResult)
            {
                selectedOutlets.Add(outlet);
                break;
            }

            
        }

        if (selectedOutlets.Count > 0)
        {
            sortResults.Add(new SortResult(consolidatedResult.Coordinate, consolidatedResult.ExpectedFeatureCount,consolidatedResult.RecTimestamp ,consolidatedResult.Features,
                selectedOutlets.ToArray()));
        }
        else
        {
            logger.Debug("SortingWorker: No outlet is selected for date {}",JsonConvert.SerializeObject(consolidatedResult));
        }
    }
  
    public event EventHandler<SortingResultEventArg> OnResult;
    protected virtual void DispatchResultEvent(SortingResultEventArg e)
    {
        var handler = OnResult;
        handler?.Invoke(this, e);
    }
}

public class SortingResultEventArg
{
    public List<SortResult> Results;

    public SortingResultEventArg(List<SortResult> results)
    {
        Results = results;
    }
}