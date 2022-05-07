using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.Worker.Analytics;
using CommonLib.Lib.Worker.Upper;
using NLog;

namespace CommonLib.Lib.Worker.HTTP;

public class UpperWorkerManager
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static UpperWorkerManager me = new UpperWorkerManager();

    private UpperWorkerManager()
    {
        //setup();
    }

    public static UpperWorkerManager getInstance()
    {
        return me;
    }

    public  void setup()
    {
        
        UpperToCameraHTTPClientWorker.getInstance();
        //Piple line wireup;
        
        ConsolidateWorker.getInstance().onRecReceiving += consolidateRecReceivingHandler;
        ConsolidateWorker.getInstance().OnResult+=consolidateResultEventHandler;
        SortingWorker.getInstance().OnResult+=sortingResultEventHandler;
        LBWorker.getInstance().OnResult+=LBResultEventHandler;
        EmitWorker.getInstance().OnResult+=EmitResultEventHandler;
        LowerMachineWorker.init();
        ElasticSearchWorker.getInstance();
        
        running = true;
        stats.Add(typeof(ConsolidateWorker),new Queue<WorkerStats>());
        stats.Add(typeof(SortingWorker),new Queue<WorkerStats>());
        stats.Add(typeof(LBWorker),new Queue<WorkerStats>());
        stats.Add(typeof(EmitWorker),new Queue<WorkerStats>());
        stats.Add(typeof(LowerMachineWorker),new Queue<WorkerStats>());
        stats.Add(typeof(ElasticSearchWorker),new Queue<WorkerStats>());

        Task.Run(() =>
        {
            while (running)
            {
                Thread.Sleep(5000);
                printStats();
            }
        });
    }

    private  bool running;
    public  void printStats()
    {
        foreach ((var key, var value) in stats)
        {
           

            if (value.Count == 0) continue;

            var tempValue = value.OrderBy(value => value.timeTook).ToList();
            var min = tempValue.First();
            var max = tempValue.Last();
            var avg = tempValue.Average(value=>value.timeTook);

            var callsites = key.ToString().Split(",");
            var className = callsites[callsites.Length - 1];
            logger.Info($"stats of {className}, max timeTook {max.timeTook} with batch count {max.resultCount} lastTriggerId {max.triggerID}");
            logger.Info($"stats of {className}, min timeTook {min.timeTook} with batch count {min.resultCount} lastTriggerId {min.triggerID}");
            logger.Info($"stats of {className}, avg timeTook {avg}");
            
            value.Clear();
            
        }

    }

    public  void tearDown()
    {
        running = false;
        ConsolidateWorker.getInstance().onRecReceiving -= consolidateRecReceivingHandler;
        ConsolidateWorker.getInstance().OnResult-=consolidateResultEventHandler;
        SortingWorker.getInstance().OnResult-=sortingResultEventHandler;
        LBWorker.getInstance().OnResult-=LBResultEventHandler;
        EmitWorker.getInstance().OnResult-=EmitResultEventHandler;
        
        stats.Clear();
        
        
    }

    private  Dictionary<Type, Queue<WorkerStats>> stats = new Dictionary<Type, Queue<WorkerStats>>();
    private   void consolidateResultEventHandler(object sender, ResultEventArg args)
    {
        var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        SortingWorker.getInstance().processBulk(args.Results);
        var timeTook = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
        var count = args.Results.Count;
        var triggerID = count > 0 ? args.Results.Last().Coordinate.TriggerId:-1;
        var tmpStats = new WorkerStats(triggerID, timeTook, count);
        stats[typeof(SortingWorker)].Enqueue(tmpStats);
    }
    
    private   void sortingResultEventHandler(object sender, SortingResultEventArg args)
    {
        var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        LBWorker.getInstance().processBulk(args.Results);
        var timeTook = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
        var count = args.Results.Count;
        var triggerID = count > 0 ? args.Results.Last().Coordinate.TriggerId:-1;
        var tmpStats = new WorkerStats(triggerID, timeTook, count);
        stats[typeof(LBWorker)].Enqueue(tmpStats);
    }

    private  void LBResultEventHandler(object sender, LBResultEventArg args)
    {
        
        if (ConfigUtil.getModuleConfig().ElasticSearchConfig != null &&
            ConfigUtil.getModuleConfig().ElasticSearchConfig.enabled)
        {
            var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            ElasticSearchWorker.getInstance().processBulkLBResult(args.Results);
            var timeTook = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
            var count = args.Results.Count;
            var triggerID = count > 0 ? args.Results.Last().Coordinate.TriggerId:-1;
            var tmpStats = new WorkerStats(triggerID, timeTook, count);
            stats[typeof(ElasticSearchWorker)].Enqueue(tmpStats);
        }
        
        var startTime1 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        EmitWorker.getInstance().processBulk(args.Results);

        //args.Results.Select(value => value.imageTime);
        
        var timeTook1 = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime1;
        var count1 = args.Results.Count;
        var triggerID1 = count1 > 0 ? args.Results.Last().Coordinate.TriggerId:-1;
        var tmpStats1 = new WorkerStats(triggerID1, timeTook1, count1);
        stats[typeof(EmitWorker)].Enqueue(tmpStats1);
    }
    private  void EmitResultEventHandler(object sender, EmitResultEventArg args)
    {
        var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        LowerMachineWorker.getInstance().processBulk(args.Results);
        
        var timeTook = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
        var count = args.Results.Count;
        var triggerID = count > 0 ? args.Results.Last().TriggerId:-1;
        var tmpStats = new WorkerStats(triggerID, timeTook, count);
        stats[typeof(LowerMachineWorker)].Enqueue(tmpStats);
    }
    private   void consolidateRecReceivingHandler(object sender, List<RecResult> results)
    {
        if (ConfigUtil.getModuleConfig().ElasticSearchConfig !=null && ConfigUtil.getModuleConfig().ElasticSearchConfig.enabled)
        {
            var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            ElasticSearchWorker.getInstance().processBulkRecResult(results);
        }
       
    }
    

    
    
}

public class WorkerStats
{
    public long triggerID;
    public long timeTook;
    public int resultCount;

    public WorkerStats(long triggerId, long timeTook, int resultCount)
    {
        triggerID = triggerId;
        this.timeTook = timeTook;
        this.resultCount = resultCount;
    }
}