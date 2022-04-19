using CommonLib.Lib.vo;
using Microsoft.AspNetCore.Mvc;

namespace CameraRunner.Controllers;

[ApiController]

public class ProjectController
{
    [Route("/cam/project/start")]
    [HttpPost]
    public void start(Project project)
    {

    }
}