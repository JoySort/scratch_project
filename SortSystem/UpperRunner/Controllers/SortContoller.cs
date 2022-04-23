using CommonLib.Lib.Controllers;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Sort.ResultVO;
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
}