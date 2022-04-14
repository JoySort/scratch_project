using System.Text;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EmptyResult = CommonLib.Lib.Controllers.EmptyResult;

namespace CommonLib.Lib.WebAPIs;

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
    
    [Route("/sort/consolidate_single")]
    [HttpPost]
    public void consolidate(RecResult recResult)
    {
      
       consolidateWorker.processSingle(recResult);
      
    }
    
    [Route("/sort/consolidate_batch")]
    [HttpPost]
    public void consolidate(List<RecResult> recResults)
    {
        consolidateWorker.processBulk(recResults);
       
    }
    
    [Route("/sort/sort_single")]
    [HttpPost]
    public void sort(RecResult recResult)
    {
      
        sortingWorker.processSingle(recResult);
      
    }
    
    [Route("/sort/sort_batch")]
    [HttpPost]
    public void sort(List<RecResult> recResults)
    {
       sortingWorker.processBulk(recResults);
    }
    
    [Route("/sort/lb_single")]
    [HttpPost]
    public void sort(SortResult result)
    {
      
        lbWorker.processSingle(result);
      
    }
    
    [Route("/sort/lb_batch")]
    [HttpPost]
    public void sort(List<SortResult> results)
    {
        lbWorker.processBulk(results);
    }
    
    [HttpPost]
    [Route("/sort/emit_single")]
    public void emitSingle(EmitResult result)
    {
        LowerMachineWorker.getInstance().processSingle(result);
        return ;
    }
    
    [HttpPost]
    [Route("/sort/emit_batch")]
    public void emitSingle(List<EmitResult> results)
    {
        LowerMachineWorker.getInstance().processBulk(results);
        return ;
    }
}