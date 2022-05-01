
using CameraLib.Lib.Worker.Camera;
using CameraLib.Lib.Sort.ResultVO;
using CameraLib.Lib.Worker.Recognizer;
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

    public void setup(bool standalone)
    {
        this.standalone = standalone;
        running = true;
        if (standalone)
        {
            WebControllerBridge.OnPictureArrive += RecognizerWorker.getInstance().processData;
        }
        else
        {
            CameraWorker.getInstance().CameraDrivers.ForEach(value=>value.OnPictureArrive+=RecognizerWorker.getInstance().processData);
           
        }
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
        if (standalone)
        {
            WebControllerBridge.OnPictureArrive -= RecognizerWorker.getInstance().processData;
        }
        else
        {
            CameraWorker.getInstance().CameraDrivers.ForEach(value=>value.OnPictureArrive-=RecognizerWorker.getInstance().processData);
            CameraWorker.CloseAllCams();
           
        }
        RecognizerWorker.getInstance().RecResultGenerated -= RecognizerHttpClientWorker.getInstance().onRecResultGenerated; //将识别结果通过HttpClient发出去。
       
    }

}

public class WebControllerBridge
{
    public static void processCameraDataFromWeb(CameraPayLoad cpl)
    {
        OnPictureArrive?.Invoke(null,cpl);
    }

    public static  event EventHandler<CameraPayLoad>? OnPictureArrive;

}