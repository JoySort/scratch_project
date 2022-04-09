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
        _logger.LogInformation(1, "NLog injected into Discover");
    }
    
    [HttpGet]
    [Route("/config/module")]
    public ModuleConfig getModuleConfig()
    {
        return ConfigUtil.loadModuleConfig();
    }
    
    [HttpGet]
    [Route("/config/state")]
    public MachineState[] getMachineStateConfig()
    {
        return ConfigUtil.loadMachineState();
    }
    
    [HttpGet]
    [Route("/config/emitters")]
    public Emitter[] getEmitters()
    {
        return ConfigUtil.LoadEmitters();
    }
}