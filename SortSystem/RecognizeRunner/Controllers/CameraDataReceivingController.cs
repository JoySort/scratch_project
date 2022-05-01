using System.Runtime.CompilerServices;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Worker.Recognizer;
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