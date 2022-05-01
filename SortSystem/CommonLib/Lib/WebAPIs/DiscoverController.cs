using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.ConfigVO.Emission;
using CommonLib.Lib.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommonLib.Lib.Controllers;

/**
 * <summary>发现服务，用来提供给调用端，作为第一个调用的方法来获得其提供的服务类型。</summary>
 */
[ApiController]
public class DiscoverController : ControllerBase
{
    private readonly ILogger<DiscoverController> _logger;
    
    public DiscoverController(ILogger<DiscoverController> logger)
    {
        _logger = logger;
       // _logger.LogInformation(1, "NLog injected into Discover");
    }
    
    [HttpGet]
    [Route("/config/module")]
    public ModuleConfig getModuleConfig()
    {
        return ConfigUtil.getModuleConfig();
    }
    
    [HttpGet]
    [Route("/config/state")]
    public MachineState[] getMachineStateConfig()
    {
        return ConfigUtil.getMachineState();
    }
    
    [HttpGet]
    [Route("/config/emitters")]
    public Emitter[] getEmitters()
    {
        return ConfigUtil.getEmitters();
    }
    
    [HttpGet]
    [Route("/isAlive")]
    public WebControllerResult isAlive()
    {
        return new WebControllerResult("OK");
    }
    
    
    

}
