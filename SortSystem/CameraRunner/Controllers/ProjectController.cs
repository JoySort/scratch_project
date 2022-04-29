using CommonLib.Lib.Controllers;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.vo;
using Microsoft.AspNetCore.Mvc;

namespace CameraRunner.Controllers;

[ApiController]

public class ProjectController
{ 
    private readonly ILogger<ProjectController> _logger;
    
    public ProjectController(ILogger<ProjectController> logger)
    {
        _logger = logger;
       // _logger.LogInformation(1, "NLog injected into ProjectController");
    }

    [Route("/project/start")]
    [HttpPost]
    public WebControllerResult start(Project project)
    {
        string msg ="OK";
        try
        {
            _logger.LogInformation($"Project start WEB API invoked project id {project.Id},project name {project.Id}");
            ProjectManager.getInstance().dispatchProjectStatusStartEvent(project, ProjectState.start);
        }
        catch (Exception e)
        {
            msg = e.Message;
        }

        return new WebControllerResult(msg);

    }

    
    [Route("/project/stop")]
    [HttpGet]
    public  WebControllerResult stop()
    {
        string msg = "OK";
        try
        {
            _logger.LogInformation($"Project STOP WEB API invoked ");
            ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
            
        }
        catch (Exception e)
        {
            msg = e.Message;
        }
        
        Environment.Exit(0);
        return new WebControllerResult(msg);
    }
}