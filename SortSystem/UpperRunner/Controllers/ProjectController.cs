using CameraLib.Lib.Controllers;
using CameraLib.Lib.LowerMachine;
using CameraLib.Lib.Util;
using CameraLib.Lib.vo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EmptyResult = CameraLib.Lib.Controllers.EmptyResult;

namespace CameraLib.Lib.WebAPIs.Upper;

/**
 * <summary>项目启动，停止，暂停等相关API</summary>
 */
[ApiController]
public class ProjectController : ControllerBase
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
        return project_startv1();
    }

    //compatible with old UI
    [Route("/apis/project_start_v1")]
    [HttpPost]
    public UIAPIResult project_startv1()
    {
        var errorObj = new JoyError();

        try
        {
            Project project = ProjectParser.ParseHttpRequest(Request, "v1");

            ProjectManager.getInstance().dispatchProjectStatusStartEvent(project, ProjectState.start);
        }
        catch (Exception e)
        {
            errorObj.e = e.Message;
        }


        var resultData = new Controllers.EmptyResult();

        return new UIAPIResult(errorObj, resultData);
    }

    [Route("/apis/project_update")]
    [HttpPost]
    public UIAPIResult projectUpdate()
    {
        var errorObj = new JoyError();

        try
        {
            Project project = ProjectParser.ParseHttpRequest(Request, "v1");

            ProjectManager.getInstance().dispatchProjectStatusStartEvent(project, ProjectState.update);
        }
        catch (Exception e)
        {
            errorObj.e = e.Message;
        }


        var resultData = new Controllers.EmptyResult();

        return new UIAPIResult(errorObj, resultData);
    }


    [Route("/apis/apis/startwashing")]
    [HttpGet]
    public UIAPIResult projectWashing()
    {
        var errorObj = new JoyError();

        try
        {
            //Project project = ProjectParser.ParseHttpRequest(Request,"v1");

            ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.washing);
        }
        catch (Exception e)
        {
            errorObj.e = e.Message;
        }


        var resultData = new Controllers.EmptyResult();

        return new UIAPIResult(errorObj, resultData);
    }
    
    
    [Route("/apis/stopwashing")]
    [HttpGet]
    public UIAPIResult projectStopWashing()
    {
        var errorObj = new JoyError();

        try
        {
            ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        }
        catch (Exception e)
        {
            errorObj.e = e.Message;
        }


        var resultData = new Controllers.EmptyResult();

        return new UIAPIResult(errorObj, resultData);
    }

    //Support and or filter
    [Route("/apis/project_start_v2")]
    [HttpPost]
    public UIAPIResult project_start_support_and_or_filter_v2()
    {
        var errorObj = new JoyError();

        try
        {
            Project project = ProjectParser.ParseHttpRequest(Request, "v2");
            ProjectManager.getInstance().dispatchProjectStatusStartEvent(project, ProjectState.start);
        }
        catch (Exception e)
        {
            errorObj.e = e.Message;
        }


        var resultData = new Controllers.EmptyResult();

        return new UIAPIResult(errorObj, resultData);
    }


    [Route("/apis/project_start_v3")]
    [HttpPost]
    public UIAPIResult project_start_v3(Project project)
    {
        var errorObj = new JoyError();

        try
        {
            ProjectManager.getInstance().dispatchProjectStatusStartEvent(project, ProjectState.start);
        }
        catch (Exception e)
        {
            errorObj.e = e.Message;
        }


        var resultData = new Controllers.EmptyResult();

        return new UIAPIResult(errorObj, resultData);
    }

    [Route("/apis/project_stop")]
    [HttpGet]
    public UIAPIResult project_stop()
    {
        var errorObj = new JoyError();
        try
        {
            //Thread.Sleep(1000);
            ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        }
        catch (Exception e)
        {
            errorObj.e = e.Message;
        }


        var resultData = new Controllers.EmptyResult();

        return new UIAPIResult(errorObj, resultData);
    }
    [Route("/apis/get_project")]
    [HttpGet]
    public Project getProject()
    {
        return  ProjectManager.getInstance().CurrentProject;
    }
}