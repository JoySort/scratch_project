using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.Exception;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.Worker.Upper;

public class LBWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static LBWorker worker = new LBWorker();

    private Project currentProject;
    private bool isProjectRunning;
    private int sortingInterval;
    private List<SortResult> toBeProcessedResults = new List<SortResult>();
    private Dictionary<string,Dictionary<string,int>> loadBalanceCount = new Dictionary<string,Dictionary<string,int>>();
    private OutletPriority priority;
    private LBWorker()
    {
        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

    public void OnProjectStatusChange(object sender,ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            this.currentProject = statusEventArgs.currentProject;
            prepareConfig();
            this.isProjectRunning = true;
            channelStat = new Dictionary<string, long>();
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
        priority = ConfigUtil.getModuleConfig().SortConfig.OutletPriority;
        if (priority == OutletPriority.DESC)
        {
            outlets = outlets.OrderByDescending(outlet => outlet.ChannelNo).ToArray();
        }
        else
        {
            outlets = outlets.OrderBy(outlet => outlet.ChannelNo).ToArray();
        }

        var outletFilterSignitures = new List<string>();
        loadBalanceCount= new Dictionary<string, Dictionary<string, int>>();

        //outlet 已经根据优先级排序，分拣操作也是根据优先级排序的。因此，符合条件的分选结果遇到第一个负载均衡的通道就停止了。

        for (var i = 0; i < outlets.Length; i++)
        {
            var sig = generateFilterSigniture(outlets[i]);
            //if(String.IsNullOrEmpty(sig))
                outletFilterSignitures.Add(sig);
            
        }
        
        for (var i = 0; i < outletFilterSignitures.Count; i++)
        {
            for (var j = i+1; j < outletFilterSignitures.Count; j++)
            {
                var found = false;
                if (!String.IsNullOrEmpty(outletFilterSignitures[i]) && outletFilterSignitures[i] == outletFilterSignitures[j])
                {
                    found = true;
                    if (!loadBalanceCount.ContainsKey(outlets[i].ChannelNo))
                    {

                        loadBalanceCount.Add(outlets[i].ChannelNo, new Dictionary<string, int>());
                    }
                    if (loadBalanceCount[outlets[i].ChannelNo] != null)
                    {
                        //这里获得filter签名后，逐个找到跟他相同的通道后，就可以保证齐全。不必反向寻找
                        loadBalanceCount[outlets[i].ChannelNo].Add(outlets[j].ChannelNo, 0);
                        logger.Info("Load balance aligble outlent {}{}", outlets[i].ChannelNo, outlets[j].ChannelNo);
                    }
                }

                if (found && !loadBalanceCount[outlets[i].ChannelNo].ContainsKey(outlets[i].ChannelNo))
                {
                    loadBalanceCount[outlets[i].ChannelNo].Add(outlets[i].ChannelNo, 0);
                    loadBalanceCount[outlets[i].ChannelNo] = priority == OutletPriority.ASC ? 
                        loadBalanceCount[outlets[i].ChannelNo].OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value): 
                        loadBalanceCount[outlets[i].ChannelNo].OrderByDescending(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            
        }
        

        this.sortingInterval = ConfigUtil.getModuleConfig().SortConfig.SortingInterval;
        //this.currentOutlets = outlets;
        
    }

    private string generateFilterSigniture(Outlet outlet)
    {
        
            var signitures = new List<string>();
            try
            {
                foreach (var Orfilters in outlet.Filters)
                {
                    var tmpFilters = Orfilters.OrderBy(filter => filter.Criteria.Code).ToArray();
                    var signiture = "";
                    foreach (var andFilters in tmpFilters)
                    {
                        signiture += andFilters.Criteria.Code + String.Join(",", andFilters.FilterBoundryIndices);

                    }

                    signitures.Add(signiture);
                }
            }
            catch (System.Exception e)
            {
                logger.Error("generateFilterSigniture for outlet {}",outlet.ChannelNo);
            }
            var result = String.Join(",", signitures.OrderBy(value => value).ToArray());
            //logger.Info("Outlet {} filter signiture{}", outlet.ChannelNo, result);
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
        processResult();
    }


    private void processResult()
    {
         //Task.Run(() =>
         // {
            //  logger.Info("LBWorker starts process project id {} project name {} ", currentProject.Id, currentProject.Name);
             
           //   while (isProjectRunning)
          //    {
                //  Thread.Sleep(sortingInterval);
                  var processBatch = toBeProcessedResults;
              //    if (processBatch.Count <= 0) continue;
                  toBeProcessedResults = new List<SortResult>();
                //Load and Balancing
                var lbResults = new List<LBResult>();
                  foreach (var sortResult in processBatch)
                  {
                      var outletNO = sortResult.Outlets.First().ChannelNo;
                      string lbChannelNO = outletNO;
                      if (loadBalanceCount.ContainsKey(outletNO) && loadBalanceCount[outletNO] != null && loadBalanceCount[outletNO].Count > 0)
                      {
                          loadBalanceCount[outletNO] = loadBalanceCount[outletNO].OrderBy(dic => dic.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                          lbChannelNO = loadBalanceCount[outletNO].Keys.First();
                          loadBalanceCount[outletNO][lbChannelNO]++;
                          //outletNO = lbChannelNO;
                      }

                      var lbResult = new LBResult(sortResult.Coordinate, sortResult.ExpectedFeatureCount, sortResult.Features,
                          sortResult.Outlets,
                          new Outlet[] { new Outlet(lbChannelNO, sortResult.Outlets.First().Type, sortResult.Outlets.First().Filters) });
                      lbResults.Add(lbResult);
                      //logger.Debug("loadBalanceCount obj status when outletNO:{} outletNO {} loadBalanceCount {} ", outletNO, lbChannelNO, JsonConvert.SerializeObject(loadBalanceCount,Formatting.Indented));
                  }
                
                  logger.Debug("LB result with count:{}",lbResults.Count);
                  updateChannelStats(lbResults);
                  DispatchResultEvent(new LBResultEventArg(lbResults));
                  
                 
              //}
              //logger.Info("LBWorker stops process project id {} project name {} ", currentProject.Id, currentProject.Name);
         // });
    }

    private void updateChannelStats(List<LBResult> results)
    {
        foreach (var result in results)
        {
            if(!channelStat.ContainsKey(result.LoadBalancedOutlet.First().ChannelNo))channelStat.Add(result.LoadBalancedOutlet.First().ChannelNo,0);
            channelStat[result.LoadBalancedOutlet.First().ChannelNo]++;
        }
    }

    private Dictionary<string, long> channelStat = new Dictionary<string, long>();

    public Dictionary<string, long> ChannelStat => channelStat;

    public event EventHandler<LBResultEventArg> OnResult;
    protected virtual void DispatchResultEvent(LBResultEventArg e)
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