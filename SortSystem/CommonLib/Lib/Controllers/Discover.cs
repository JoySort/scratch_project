using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommonLib.Lib.Controllers;

/**
 * <summary>发现服务，用来提供给调用端，作为第一个调用的方法来获得其提供的服务类型。</summary>
 */
[ApiController]
[Route("[controller]")]
public class Discover: ControllerBase
{
    private readonly ILogger<Discover> _logger;
    
    public Discover(ILogger<Discover> logger)
    {
        _logger = logger;
        _logger.LogInformation(1, "NLog injected into Discover");
    }
    
    [HttpGet(Name = "GetServiceInfo")]
    public ModuleConfig Get()
    {
        return ConfigUtil.loadModuleConfig();
    }
}