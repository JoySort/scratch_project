using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.Sort;

public class ConsolidateWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static ConsolidateWorker worker = new ConsolidateWorker();


    private List<RawResult> incompleteWaitingList = new List<RawResult>();

    private List<RawResult> cache = new List<RawResult>();

    //private consolidationPolicy;
    private ConsolidateWorker()
    {
        
    }

    private void consolidate(RawResult rawResult)
    {
        
    }

}