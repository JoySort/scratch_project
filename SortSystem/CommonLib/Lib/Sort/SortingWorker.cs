using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.Sort;

public class SortingWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static SortingWorker worker = new SortingWorker();

    private Project currentProject;
    private Outlet[] currentOutlets;
    private List<RecResult> incompleteWaitingList = new List<RecResult>();
    private bool isProjectRunning;
    private int sortingInterval;
    private List<RecResult> toBeProcessedResults = new List<RecResult>();
    private List<SortResult> sortResults;
    private int[] outletLBCount ;
    private SortingWorker()
    {
        ProjectEventDispatcher.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

    public void OnProjectStatusChange(object sender,ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            this.currentProject = statusEventArgs.currentProject;
            prepareConfig();
            this.isProjectRunning = true;
            processResult();
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
        this.outletLBCount = new int[outlets.Length+1];
    }

    public static SortingWorker getInstance()
    {
        return worker;
    }

    public void processSingle(RecResult recResult)
    {
        toBeProcessedResults.Add(recResult);
    }

    public void processBulk(List<RecResult> recResults)
    {
        toBeProcessedResults.AddRange(recResults);
    }


    private void processResult()
    {
        Task.Run(() =>
        {
            logger.Info("SortingWorker starts process project id {} project name {} ",currentProject.Id,currentProject.Name);

            while (isProjectRunning)
            {
                var processBatch = toBeProcessedResults;
                toBeProcessedResults = new List<RecResult>();
                sortResults = new List<SortResult>();
                foreach (var item in processBatch)
                {
                    if (item.ExpectedFeatureCount == item.Features.Length)
                    {
                        applySortingRules(item);
                    }
                    else
                    {
                        incompleteWaitingList.Add(item);
                        //TODO: write code to cross check incomplete waiting list to add them back to toBeProcessedResults for sorting
                    }
                }
                
                //Load and Balancing
                
                foreach (var sortResult in sortResults)
                {
                    if (sortResult.Outlets.Length == 1)
                    {
                        
                    }
                    else if(sortResult.Outlets.Length>1)
                    {
                        foreach (var outlet in sortResult.Outlets)
                        {
                            outletLBCount[Int32.Parse(outlet.ChannelNo)]++;
                        }

                       
                    }
                }

                Thread.Sleep(sortingInterval);
            }
            logger.Info("SortingWorker stops process project id {} project name {} ",currentProject.Id,currentProject.Name);
        });
    }

   
    
    private void applySortingRules(RecResult recResult)
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
                    andResult = andResult && andFilter.doFilter(recResult);
                }

                oRResult = oRResult || andResult;
            }

            if (oRResult)
            {
                selectedOutlets.Add(outlet);
            }

            
        }
        if(selectedOutlets.Count>0)
            sortResults.Add( new SortResult(recResult.Coordinate,recResult.ExpectedFeatureCount,recResult.Features,selectedOutlets.ToArray()));
    }
}