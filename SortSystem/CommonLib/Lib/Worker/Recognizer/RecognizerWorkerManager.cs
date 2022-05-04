
using CommonLib.Lib.Worker.Camera;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.Worker.Recognizer;
using NLog;

namespace RecognizerLib.Lib.Worker;

public class RecognizerWorkerManager
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static RecognizerWorkerManager me = new RecognizerWorkerManager();
    private bool running;
    private bool standalone;
    private RecognizerWorkerManager()
    {
       // setup();
    }

    public static RecognizerWorkerManager getInstance()
    {
        return me;
    }

    public void setup()
    {
        this.standalone = ConfigUtil.getModuleConfig().Standalone;
        running = true;
        if (!standalone)
        {
            CameraWorker.getInstance().CameraDrivers.ForEach(value=>value.OnPictureArrive+=WebControllerBridge.wrapCameraData);
           
        }
       
        WebControllerBridge.OnPicturesArrive += RecognizerWorker.getInstance().processData;
        RecognizerWorker.getInstance().RecResultGenerated += RecognizerHttpClientWorker.getInstance().onRecResultGenerated; //将识别结果通过HttpClient发出去。
        
        
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
        if (!standalone)
        {
            CameraWorker.getInstance().CameraDrivers.ForEach(value=>value.OnPictureArrive-=WebControllerBridge.wrapCameraData);
            CameraWorker.CloseAllCams();
           
        }
        WebControllerBridge.OnPicturesArrive -= RecognizerWorker.getInstance().processData;
        RecognizerWorker.getInstance().RecResultGenerated -= RecognizerHttpClientWorker.getInstance().onRecResultGenerated; //将识别结果通过HttpClient发出去。
       
    }

}

public class WebControllerBridge
{
    public static void processCameraDataFromWeb(List<CameraPayLoad> cpl)
    {
        OnPicturesArrive?.Invoke(null,cpl);
    }

    public static void wrapCameraData(object sender, CameraPayLoad cpl)
    {
        var list = new List<CameraPayLoad>();
        list.Add(cpl);
        OnPicturesArrive?.Invoke(null,list);
    }

    public static  event EventHandler<List<CameraPayLoad>>? OnPicturesArrive;
   

}