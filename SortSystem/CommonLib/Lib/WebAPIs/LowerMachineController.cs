using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.ConfigVO.Emission;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommonLib.Lib.Controllers;

/**
 * <summary>发现服务，用来提供给调用端，作为第一个调用的方法来获得其提供的服务类型。</summary>
 */
[ApiController]
public class LowerMachineController : ControllerBase
{
    private readonly ILogger<DiscoverController> _logger;
    
    public LowerMachineController(ILogger<DiscoverController> logger)
    {
        _logger = logger;
        _logger.LogInformation(1, "NLog injected into Discover");
    }

    [HttpPost]
    [Route("/lower/switch_machine_status")]
    public void changeMachineStatus(ProjectState state)
    {
        if (state == ProjectState.reverse || state == ProjectState.washing)
        {
            ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(state);
           
        }
        else
        {
            throw new Exception("次API仅供调试，如果要启动项目应该使用project_start API，这里仅支持清洗，维护");
        }

    }
    

    
  
}