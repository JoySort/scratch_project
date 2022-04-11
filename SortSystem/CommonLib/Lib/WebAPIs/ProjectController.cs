using System.Text;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EmptyResult = CommonLib.Lib.Controllers.EmptyResult;

namespace CommonLib.Lib.WebAPIs;

/**
 * <summary>项目启动，停止，暂停等相关API</summary>
 */
[ApiController]
public class ProjectController: ControllerBase
{
    private readonly ILogger<ProjectController> logger;
    
    public ProjectController(ILogger<ProjectController> logger)
    {
        this.logger = logger;
        //this.logger.LogInformation(1, "NLog injected into ProjectControllers");
    }
    
    [Route("/apis/project_start")]
    [HttpPost]
    public UIAPIResult project_start()
    {
        var errorObj = new JoyError();

                try
                {

                    Project project = ProjectParser.ParseHttpRequest(Request);
                    ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent( project,ProjectState.start);
                }
                catch (Exception e)
                {
                    errorObj.e = e.Message;
                }
        

        var errorCode = errorObj.e != null ? "2" : "1";
        var errorMessage = (errorObj.e ??"");
        var status =  (errorObj.e != null ? "error" : "ok");
        var resultData = new EmptyResult();
        
        return new UIAPIResult(errorCode,errorMessage,errorObj,status,resultData);
    }

    [Route("/apis/project_stop")]
    [HttpGet]
    public UIAPIResult project_stop()
    { 
        var errorObj = new JoyError();
        try
        {
            ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        }
        catch (Exception e)
        {
            errorObj.e = e.Message;
        }

        var errorCode = errorObj.e != null ? "2" : "1";
        var errorMessage = (errorObj.e ??"");
        var status =  (errorObj.e != null ? "error" : "ok");
        var resultData = new EmptyResult();
        
        return new UIAPIResult(errorCode,errorMessage,errorObj,status,resultData);
    }
}