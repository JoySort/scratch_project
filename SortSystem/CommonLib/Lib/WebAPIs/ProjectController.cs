using System.Text;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using CommonLib.Lib.Worker;
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
        this.logger.LogInformation(1, "NLog injected into ProjectControllers");
    }
    
    [Route("/apis/project_start")]
    [HttpPost]
    public UIAPIResult project_start()
    {
        var errorObj = new JoyError();
        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        {  
            string content = reader.ReadToEndAsync().Result;
           
            if(!string.IsNullOrEmpty(content)){
                try
                {
                    ProjectParser parser = new ProjectParser(content);
                    Project project = parser.getProject();
                    //全局播发项目启动事件
                    //TODO:implement start project operation by registering an event listener 
                    LowerMachineHelper.getInstance().dispatchProjectStatusChangeEvent( project,ProjectStatus.start);
                    
                    
                    if(logger.IsEnabled(LogLevel.Debug))
                        logger.LogDebug("Project id: {} with name {} of content:{}", project.Id, project.Name,JsonConvert.SerializeObject(project));
                }
                catch (Exception e)
                {
                    errorObj.e = e.Message;
                }
            }
            else
            {
                errorObj.e = "Post content is empty";
            }
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
        
        LowerMachineHelper.getInstance().dispatchProjectStatusChangeEvent( null,ProjectStatus.stop);
        //TODO:implement stop project lower operations here;
        var errorObj = new JoyError();
        var errorCode = errorObj.e != null ? "2" : "1";
        var errorMessage = (errorObj.e ??"");
        var status =  (errorObj.e != null ? "error" : "ok");
        var resultData = new EmptyResult();
        
        return new UIAPIResult(errorCode,errorMessage,errorObj,status,resultData);
    }
}