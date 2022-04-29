using Microsoft.AspNetCore.Mvc;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.Worker.Camera;
using CommonLib.Lib.Camera;

namespace CameraRunner.Controllers
{
    [ApiController]
    public class CameraController
    {
        private readonly ILogger<CameraController> _logger;

        public CameraController(ILogger<CameraController> logger)
        {
            _logger = logger;
            // _logger.LogInformation(1, "NLog injected into ProjectController");
        }
        [HttpGet]
        [Route("/camera/exit")]
        public WebControllerResult Exit()
        {
            string msg = "OK";
            try
            {
                _logger.LogInformation("CammeraRunner stopped manually.");

                foreach (ICameraDriver ic in CameraWorker.getInstance().CameraDrivers)
                {
                    ic.CloseCam();
                }
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                }
                );
            }
            catch (Exception e)
            {
                msg = e.Message;
            }

            return new WebControllerResult(msg);

        }


    }
}
