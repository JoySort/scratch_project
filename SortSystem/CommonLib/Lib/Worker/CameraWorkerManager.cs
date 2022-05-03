using CommonLib.Lib.Camera;
using CommonLib.Lib.Worker.Camera;
using NLog;

namespace CommonLib.Lib.Worker;

public class CameraWorkerManager
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static CameraWorkerManager me = new CameraWorkerManager();
    private bool running;

    private CameraWorkerManager()
    {
        setup();
    }

    public static CameraWorkerManager getInstance()
    {
        return me;
    }
    private  void setup()
    {
        //Piple line wireup;
        List<ICameraDriver> cameraDrivers = CameraWorker.getInstance().CameraDrivers;
        
        Task.Run(() =>
        {
            while (running)
            {
                Thread.Sleep(5000);
               // printStats();
            }
        });
    }
    
    public void tearDown()
    {
        running = false;
       
        CameraWorker.CloseAllCams();
  
       
       
    }
}