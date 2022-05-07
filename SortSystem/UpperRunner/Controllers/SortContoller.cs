using CommonLib.Lib.Sort;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.Worker;
using CommonLib.Lib.Worker.Upper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EmptyResult = CommonLib.Lib.Controllers.EmptyResult;

namespace CommonLib.Lib.WebAPIs.Upper;

/**
 * <summary>sorting server</summary>
 */
[ApiController]
public class SortController: ControllerBase
{
    private readonly ILogger<SortController> logger;
    private static ConsolidateWorker consolidateWorker = ConsolidateWorker.getInstance();
    private static SortingWorker sortingWorker = SortingWorker.getInstance();
    private static LBWorker lbWorker = LBWorker.getInstance();
    public SortController(ILogger<SortController> logger)
    {
        this.logger = logger;
        //this.logger.LogInformation(1, "NLog injected into SortController");
    }
   
    
    [Route("/sort/consolidate_batch")]
    [HttpPost]
    public WebControllerResult consolidate(List<RecResult> recResults)
    {

       
        consolidateWorker.processBulk(recResults);
        return new WebControllerResult("OK");//如果有异常，则会抛出，因此运行到这里就意味着没问题。
       
    }
    
   
    
    
    
    [Route("/sort/sort_batch")]
    [HttpPost]
    public WebControllerResult sort(List<ConsolidatedResult> recResults)
    {
       sortingWorker.processBulk(recResults);
       return new WebControllerResult("OK");
    }
    
   
    
    [Route("/sort/lb_batch")]
    [HttpPost]
    public WebControllerResult sort(List<SortResult> results)
    {
        lbWorker.processBulk(results);
        return new WebControllerResult("OK");
    }
    
   
    
    [HttpPost]
    [Route("/sort/emit_batch")]
    public WebControllerResult emitSingle(List<EmitResult> results)
    {
        LowerMachineWorker.getInstance().processBulk(results);
        return new WebControllerResult("OK");
    }
    
    
    [HttpGet]
    [Route("/apis/total_runtime")]
    public UIAPIResult totalRunTime()
    {   
        var errorObj = new JoyError();
        var resultData = new UIResultDataTotalRuntime();
        return new UIAPIResult(errorObj,resultData);
    }
    
    [HttpGet]
    [Route("/apis/channel_counter")]
    public UIAPIResult channelCounter()
    {
        var errorObj = new JoyError();
       
        var cstat = LBWorker.getInstance().ChannelStat;
        var project = ProjectManager.getInstance().CurrentProject;
        var result = new List<UIResultChannelCounter>();
        var total = LBWorker.getInstance().ResultTotalCount;

        total = total == 0 ? -1 : total;
        if (project != null)
        {
            long totalCount = 0;
            foreach (var outlet in project.Outlets)
            {
                var count = cstat.ContainsKey(outlet.ChannelNo) ? cstat[outlet.ChannelNo] : 0;
                result.Add( new UIResultChannelCounter(outlet.ChannelNo, count, Math.Round((double)count/total,3)));;
                totalCount += count;
                logger.LogDebug($" totalCount {totalCount},total {total},percent {Math.Round((double)count/total,3)}");
            }
        }
       

     

        return new UIAPIResult(errorObj,result);
    }
    
    [HttpGet]
    [Route("/apis/discover")]
    public UIAPIResult discover()
    {
        var errorObj = new JoyError();
        var resultData = new V1Discover();
        var rpcEndPoints = ModuleCommunicationWorker.getInstance().RpcEndPoints;
        var nodeCount = 1;
        foreach(var item in rpcEndPoints){
            if (item.Key == JoyModule.Upper)
            {
                nodeCount++;
            }
        }

        var isLocalMaster = ConfigUtil.getModuleConfig().LowerConfig.Select(value=>value.IsMaster).Contains(true);
        resultData.count = nodeCount;
        resultData.node = isLocalMaster ? "master" : "slave";
        return new UIAPIResult(errorObj,resultData);
    }
    
    [HttpGet]
    [Route("/apis/sync_status")]
    public UIAPIResult syncStatus()
    {
        var errorObj = new JoyError();
        var status = "";
        switch (ProjectManager.getInstance().ProjectState)
        {
            case ProjectState.start:
                status = "running";
                break;
            case ProjectState.stop:
                status = "ready";
                break;
            default:
                status = Enum.GetName(ProjectManager.getInstance().ProjectState);
                break;
        }

        var resultData = new SynStatus(status);
        
        return new UIAPIResult(errorObj,resultData);
    }

}
public class UIResultDataTotalRuntime:IJoyResult
{
    
    public long total_runtime => DateTimeOffset.Now.ToUnixTimeMilliseconds()-ProjectManager.getInstance().StartTimestamp;
}

public class UIResultChannelCounter:IJoyResult
{
    private string id;
    private long count;
    private double percent;

    public UIResultChannelCounter(string id, long count, double percent)
    {
        this.id = id;
        this.count = count;
        this.percent = percent;
    }

    public long Count
    {
        get => count;
        set => count = value;
    }

    public double Percent
    {
        get => percent;
        set => percent = value;
    }

    public string Id
    {
        get => id;
        set => id = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    
}


public class V1Discover
{
    private string _node ="master";
    private int _count = 1;

    public string node
    {
        get => _node;
        set => _node = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int count
    {
        get => _count;
        set => _count = value ;
    }
}

public class SynStatus
{
    public string sync_status
    {
        get;
        set;
    }

    public SynStatus(string syncStatus)
    {
        sync_status = syncStatus;
    }
}