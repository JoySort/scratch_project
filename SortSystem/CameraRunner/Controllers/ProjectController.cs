using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.vo;
using Microsoft.AspNetCore.Mvc;

namespace CameraRunner.Controllers;

[ApiController]

public class ProjectController
{
    [Route("/project/start")]
    [HttpPost]
    public void start(Project project)
    {
        ProjectManager.getInstance().dispatchProjectStatusStartEvent( project,ProjectState.start);
    }
    
    [Route("/project/stop")]
    [HttpGet]
    public void stop()
    {
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
    }
}