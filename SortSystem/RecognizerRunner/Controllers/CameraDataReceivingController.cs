using System.Runtime.CompilerServices;
using CameraLib.Lib.Controllers;
using CameraLib.Lib.Sort.ResultVO;
using CameraLib.Lib.Worker.Recognizer;
using Microsoft.AspNetCore.Mvc;
using NLog;
using RecognizerLib.Lib.Worker;

namespace RecognizerRunner.Controllers;

[ApiController]
[Route("[controller]")]
public class CameraDataReceivingController : ControllerBase
{
 
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    [Route("/recognizer/process_batch")]
    [HttpPost]
    public WebControllerResult proces_batch(CameraPayLoad cpl)
    {

        WebControllerBridge.processCameraDataFromWeb(cpl);
        return  new WebControllerResult("OK");
    }
    
}