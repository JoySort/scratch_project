using System.Text;
using CommonLib.Lib.Controllers;
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
    
    [Route("/sort/")]
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
}