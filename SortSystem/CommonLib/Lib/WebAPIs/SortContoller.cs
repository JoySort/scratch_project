using System.Text;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EmptyResult = CommonLib.Lib.Controllers.EmptyResult;

namespace CommonLib.Lib.WebAPIs;

/**
 * <summary>发现服务，用来提供给调用端，作为第一个调用的方法来获得其提供的服务类型。</summary>
 */
[ApiController]
public class SortController: ControllerBase
{
    private readonly ILogger<SortController> logger;
    
    public SortController(ILogger<SortController> logger)
    {
        this.logger = logger;
        this.logger.LogInformation(1, "NLog injected into SortController");
    }
    
    [Route("/sort/single")]
    [HttpPost]
    public void singleSort(RecResult rawResult)
    {
       logger.LogDebug("raw result",rawResult.Coordinates.First().Column);
        
        return ;
    }
}