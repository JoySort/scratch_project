using CameraLib.Lib.Controllers;
using CameraLib.Lib.LowerMachine;
using CameraLib.Lib.Worker.Upper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CameraLib.Lib.WebAPIs.Upper;

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
    [Route("/lower/switch_machine_running_status")]
    public void changeMachineStatus(ProjectState state)
    {
        if (state == ProjectState.reverse || state == ProjectState.washing)
        {
            ProjectManager.getInstance().dispatchProjectStatusChangeEvent(state);
           
        }
        else
        {
            throw new Exception("次API仅供调试，如果要启动项目应该使用project_start API，这里仅支持清洗，维护");
        }

    }
    
    [HttpGet]
    [Route("/lower/machineID")]
    public string getMachineID()
    {
        return LowerMachineWorker.getInstance().LowerMachineDriver.machineID;

    }
    

    [HttpGet]
    [Route("/lower/startrunning")]
    public string startrunning()
    {
        LowerMachineWorker.getInstance().LowerMachineDriver.StartRunning();
        return "Ok";

    }

    [HttpGet]
    [Route("/lower/stoprunning")]
    public string stoprunning()
    {
        LowerMachineWorker.getInstance().LowerMachineDriver.StopRunning();
        return "Ok";
    }

    [HttpGet]
    [Route("/lower/gettriggercount")]
    public string getTriggerCount()
    {
        LowerMachineWorker.getInstance().LowerMachineDriver.triggers[0].getTirggerCount();
        Thread.Sleep(100);
        return LowerMachineWorker.getInstance().LowerMachineDriver.triggers[0].TriggerCount.ToString();
    }

    [HttpGet]
    [Route("/lower/moveoneslot")]
    public string moveOneSlot()
    {
        LowerMachineWorker.getInstance().LowerMachineDriver.servos[0].MoveOneSlot();
        
        return "OK";
    }

    [HttpGet]
    [Route("/lower/sendfakeresult")]
    public string sendfakeresult()
    {
        byte[] fakeResults = new byte[16];
        
        fakeResults[0] = 1;
        fakeResults[1] = 1;
        fakeResults[2] = 1;
        fakeResults[3] = 1;
        fakeResults[4] = 1;
        fakeResults[5] = 6;
        fakeResults[6] = 6;
        fakeResults[7] = 6;

        int tid = 0;
        int.TryParse(Request.Query["tid"], out tid);
        LowerMachineWorker.getInstance().LowerMachineDriver
            .advancedEmitter[0].SendEmitResultCMD(fakeResults, tid);
        
        return "OK";
    }
}