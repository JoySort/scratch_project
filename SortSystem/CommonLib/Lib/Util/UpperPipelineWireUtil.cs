using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Worker.Analytics;
using CommonLib.Lib.Worker.Upper;

namespace CommonLib.Lib.Util;

public class UpperPipelineWireUtil
{
    public static void setup()
    {
        //Piple line wireup;
        
        ConsolidateWorker.getInstance().onRecReceiving += consolidateRecReceivingHandler;
        ConsolidateWorker.getInstance().OnResult+=consolidateResultEventHandler;
        SortingWorker.getInstance().OnResult+=sortingResultEventHandler;
        LBWorker.getInstance().OnResult+=LBResultEventHandler;
        EmitWorker.getInstance().OnResult+=EmitResultEventHandler;
        LowerMachineWorker.init();
        ElasticSearchWorker.getInstance();

    }

    public static void tearDown()
    {
        ConsolidateWorker.getInstance().onRecReceiving -= consolidateRecReceivingHandler;
        ConsolidateWorker.getInstance().OnResult-=consolidateResultEventHandler;
        SortingWorker.getInstance().OnResult-=sortingResultEventHandler;
        LBWorker.getInstance().OnResult-=LBResultEventHandler;
        EmitWorker.getInstance().OnResult-=EmitResultEventHandler;
        
    }

    private  static void consolidateResultEventHandler(object sender, ResultEventArg args)
    {
        SortingWorker.getInstance().processBulk(args.Results);
    }
    
    private  static void sortingResultEventHandler(object sender, SortingResultEventArg args)
    {
        LBWorker.getInstance().processBulk(args.Results);
    }

    private static void LBResultEventHandler(object sender, LBResultEventArg args)
    {
        EmitWorker.getInstance().processBulk(args.Results);
        if (ConfigUtil.getModuleConfig().ElasticSearchConfig != null &&
            ConfigUtil.getModuleConfig().ElasticSearchConfig.enabled)
        {
            ElasticSearchWorker.getInstance().processBulkLBResult(args.Results);
        }
    }
    private static void EmitResultEventHandler(object sender, EmitResultEventArg args)
    {
        LowerMachineWorker.getInstance().processBulk(args.Results);
    }
    private  static void consolidateRecReceivingHandler(object sender, List<RecResult> results)
    {
        if (ConfigUtil.getModuleConfig().ElasticSearchConfig !=null && ConfigUtil.getModuleConfig().ElasticSearchConfig.enabled)
        {
            ElasticSearchWorker.getInstance().processBulkRecResult(results);
        }
       
    }
    

    
    
}