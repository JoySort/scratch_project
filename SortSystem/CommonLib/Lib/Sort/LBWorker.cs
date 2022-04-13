using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.Sort;

public class LBWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static LBWorker worker = new LBWorker();

    private Project currentProject;
    private Outlet[] currentOutlets;
    private bool isProjectRunning;
    private int sortingInterval;
    private List<SortResult> toBeProcessedResults = new List<SortResult>();
    private Dictionary<string,Dictionary<string,int>> loadBalanceCount = new Dictionary<string,Dictionary<string,int>>();

    private int[] outletLBCount ;
    private LBWorker()
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

        var outletFilterSignitures = new List<string>();
        
        //outlet 已经根据优先级排序，分拣操作也是根据优先级排序的。因此，符合条件的分选结果遇到第一个负载均衡的通道就停止了。
       
        for (var i = 0; i < outlets.Length; i++)
        {
            outletFilterSignitures.Add( generateFilterSigniture(outlets[i]));
            
        }
        
        for (var i = 0; i < outletFilterSignitures.Count; i++)
        {
            for (var j = i+1; j < outletFilterSignitures.Count; j++)
            {
                var found = false;
                if (outletFilterSignitures[i] == outletFilterSignitures[j])
                {
                    found = true;
                    if (!loadBalanceCount.ContainsKey(outlets[i].ChannelNo))
                    {
                        
                        loadBalanceCount.Add(outlets[i].ChannelNo,new Dictionary<string, int>());
                    }
                    if (loadBalanceCount[outlets[i].ChannelNo] != null)
                    {
                        //这里获得filter签名后，逐个找到跟他相同的通道后，就可以保证齐全。不必反向寻找
                        loadBalanceCount[outlets[i].ChannelNo].Add(outlets[j].ChannelNo,0);
                        logger.Info("Load balance aligble outlent {}{}",outlets[i].ChannelNo,outlets[j].ChannelNo);
                    }
                }

                if (found && !loadBalanceCount[outlets[i].ChannelNo].ContainsKey(outlets[i].ChannelNo)) loadBalanceCount[outlets[i].ChannelNo].Add(outlets[i].ChannelNo, 0);
            }
            
        }
        

        this.sortingInterval = ConfigUtil.getModuleConfig().SortConfig.SortingInterval;
        this.currentOutlets = outlets;
        this.outletLBCount = new int[outlets.Length+1];//为未来的loop通道预留一个位置
    }

    private string generateFilterSigniture(Outlet outlet)
    {
        var signitures = new List<string>();
        foreach (var Orfilters in outlet.Filters)
        {
            var tmpFilters = Orfilters.OrderBy(filter => filter.Criteria.Code).ToArray();
            var signiture = "";
            foreach (var andFilters in tmpFilters)
            {
                signiture += andFilters.Criteria.Code + String.Join(",", andFilters.FilterBoundrryIndices);
                
            }
            signitures.Add(signiture);
        }

        var result = String.Join(",", signitures.OrderBy(value => value).ToArray());
        logger.Info("Outlet {} filter signiture{}",outlet.ChannelNo,result);
        return result;
    }

   
    public static LBWorker getInstance()
    {
        return worker;
    }

    public void processSingle(SortResult sortResult)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("LBWorker:");
        toBeProcessedResults.Add(sortResult);
    }

    public void processBulk(List<SortResult> sortResults)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("LBWorker:");
        toBeProcessedResults.AddRange(sortResults);
    }


    private void processResult()
    {
        Task.Run(() =>
        {
            logger.Info("LBWorker starts process project id {} project name {} ",currentProject.Id,currentProject.Name);

            while (isProjectRunning)
            {

                var processBatch = toBeProcessedResults;
                toBeProcessedResults = new List<SortResult>();
                //Load and Balancing
                var lbResults = new List<LBResult>();
                foreach (var sortResult in processBatch)
                {
                    var outletNO = sortResult.Outlets.First().ChannelNo;
                    if (loadBalanceCount[outletNO]!=null && loadBalanceCount[outletNO].Count>0)
                    {
                       var lbChannelNO = loadBalanceCount[outletNO].OrderBy(dic=>dic.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).Keys.First();
                       loadBalanceCount[outletNO][lbChannelNO]++;
                       outletNO = lbChannelNO;
                    }
                    
                    var lbResult = new LBResult(sortResult.Coordinate, sortResult.ExpectedFeatureCount, sortResult.Features,
                        sortResult.Outlets,
                        new Outlet[] {new Outlet(outletNO, sortResult.Outlets.First().Type,sortResult.Outlets.First().Filters)});
                    lbResults.Add(lbResult);
                    
                }

                OnConsolidateResult(new LBResultEventArg(lbResults));
                
                Thread.Sleep(sortingInterval);
            }
            logger.Info("LBWorker stops process project id {} project name {} ",currentProject.Id,currentProject.Name);
        });
    }

   
    
   
  
    public event EventHandler<LBResultEventArg> OnResult;
    protected virtual void OnConsolidateResult(LBResultEventArg e)
    {
        var handler = OnResult;
        handler?.Invoke(this, e);
    }
}

public class LBResultEventArg
{
    public List<LBResult> Results;

    public LBResultEventArg(List<LBResult> results)
    {
        Results = results;
    }
}