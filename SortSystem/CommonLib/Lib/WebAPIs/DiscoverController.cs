using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.ConfigVO.Emission;
using CommonLib.Lib.LowerMachine;
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
    
    [HttpGet]
    [Route("/apis/discover")]
    public UIAPIResult discover()
    {
        var errorObj = new JoyError();
        var resultData = new V1Discover();
        
        return new UIAPIResult(errorObj,resultData);
    }
    
    [HttpGet]
    [Route("/apis/sync_status")]
    public UIAPIResult syncStatus()
    {
        var errorObj = new JoyError();
        var status = "";
        switch (ProjectManager.getInstance().ProjectState)
        {
            case ProjectState.start:
                status = "running";
                break;
            case ProjectState.stop:
                status = "ready";
                break;
            default:
                status = Enum.GetName(ProjectManager.getInstance().ProjectState);
                break;
        }

        var resultData = new SynStatus(status);
        
        return new UIAPIResult(errorObj,resultData);
    }
    
    

}

public class V1Discover
{
    private string _node ="master";
    private string _count = "1";

    public string node
    {
        get => _node;
        set => _node = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string count
    {
        get => _count;
        set => _count = value ?? throw new ArgumentNullException(nameof(value));
    }
}

public class SynStatus
{
    public string sync_status
    {
        get;
        set;
    }

    public SynStatus(string syncStatus)
    {
        sync_status = syncStatus;
    }
}