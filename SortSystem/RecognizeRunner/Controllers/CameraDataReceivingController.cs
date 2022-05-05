using System.Runtime.CompilerServices;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Worker.Recognizer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using RecognizerLib.Lib.Worker;

namespace RecognizerRunner.Controllers;

[ApiController]
[Route("[controller]")]
public class CameraDataReceivingController : ControllerBase
{
 
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    [Route("/recognize/process_camera_data")]
    [HttpPost]
    public void proces_batch()
    {
//
        List<CameraPayLoad> cpls = null;
        if (Request.Form.ContainsKey("cameraPayload"))
        {
            cpls = JsonConvert.DeserializeObject<List<CameraPayLoad>>(Request.Form["cameraPayload"]);
        }
        
        if (Request.Form.Files != null && Request.Form.Files.Count > 0)
        {
            foreach (var formFile in Request.Form.Files)
            {
                foreach (var cpl in cpls)
                {
                    string imageName = $"{cpl.TriggerId}-{cpl.CamConfig.Columns[0]}-{cpl.CamConfig.Columns[1]}-{cpl.CamConfig.CameraPosition}";
                    if (formFile.FileName == imageName)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            formFile.CopyTo(ms);
                            cpl.PictureData = ms.ToArray();
                        }

                    }
                }

                
            }
        }
        RawDataBridge.processCameraDataFromWeb(cpls);
        return  ;
    }
    
}