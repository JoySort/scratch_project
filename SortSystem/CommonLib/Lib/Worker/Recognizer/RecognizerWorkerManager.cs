
using System.Collections.Concurrent;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Worker.Camera;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.Worker;
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
        //running = true;
        if (!standalone)
        {
            CameraWorker.getInstance().CameraDrivers.ForEach(value=>value.OnPictureArrive+=RawDataBridge.wrapCameraData);
           
        }

        RawDataBridge.OnPicturesArrive += OnPicturesArrive;//RecognizerWorker.getInstance().processData;
        RecognizerWorker.getInstance().RecResultGenerated += RecognizerHttpClientWorker.getInstance().onRecResultGenerated; //将识别结果通过HttpClient发出去。
        ProjectManager.getInstance().ProjectStatusChanged += ProjectStatusChanged;
        
        
    }

    private ConcurrentQueue<StatsHolder> pictureProcessingStatsQueue;
    private void OnPicturesArrive(object? sender, List<CameraPayLoad> e)
    {
        var stats = new StatsHolder();
        stats.StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        stats.extraTimestamp = e.First().startTime;
        RecognizerWorker.getInstance().processData(sender, e);
        stats.FinishTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        stats.count = e.Count;
        stats.extra = String.Join(",", e.Select(value => value.TriggerId));
        pictureProcessingStatsQueue.Enqueue(stats);
        if (pictureProcessingStatsQueue.Count > 100)
        {
            StatsHolder tmp;
            pictureProcessingStatsQueue.TryDequeue(out tmp);
        }
    }

    private void ProjectStatusChanged(object? sender, ProjectStatusEventArgs e)
    {
        if (e.State == ProjectState.start)
        {
            running = true;
            pictureProcessingStatsQueue = new ConcurrentQueue<StatsHolder>();
            Task.Run(() =>
            {
                while (running)
                {
                    Thread.Sleep(5000);
                     printStats();
                }
            });
        }
        else if(e.State == ProjectState.stop)
        {
            running = false;
            pictureProcessingStatsQueue.Clear();
        }
    }

    private void printStats()
    {
        if (pictureProcessingStatsQueue.Count == 0) return;
        logger.Info($"Picture Processing stats in 5 sec avg:{pictureProcessingStatsQueue.Select(value=>value.timeTook).Average()} " +
                    $"min:{pictureProcessingStatsQueue.Select(value=>value.timeTook).Min()}" +
                    $"max:{pictureProcessingStatsQueue.Select(value=>value.timeTook).Max()}" +
                    $"Count:{pictureProcessingStatsQueue.Select(value=>value.count).Sum()}" +
                    $"triggerid:{pictureProcessingStatsQueue.Select(value=>value.extra).Aggregate((i,j)=> i+"|"+j)}" 
                    );
        logger.Info($"picture transfertime:{pictureProcessingStatsQueue.Select(value=>value.StartTime-value.extraTimestamp).Average()}");
    }

    public void tearDown()
    {
        running = false;
        if (!standalone)
        {
            CameraWorker.getInstance().CameraDrivers.ForEach(value=>value.OnPictureArrive-=RawDataBridge.wrapCameraData);
            CameraWorker.CloseAllCams();
           
        }
        pictureProcessingStatsQueue.Clear();
        RawDataBridge.OnPicturesArrive -= RecognizerWorker.getInstance().processData;
        RecognizerWorker.getInstance().RecResultGenerated -= RecognizerHttpClientWorker.getInstance().onRecResultGenerated; //将识别结果通过HttpClient发出去。
        ProjectManager.getInstance().ProjectStatusChanged -= ProjectStatusChanged;
    }

}

public class RawDataBridge
{
    public static void processCameraDataFromWeb(List<CameraPayLoad> cpl)
    {
        OnPicturesArrive?.Invoke(null,cpl);
    }
    

    public static void wrapCameraData(object sender, CameraPayLoad cpl)
    {
        var list = new List<CameraPayLoad>();
        list.Add(cpl);
        OnPicturesArrive?.Invoke(sender,list);
    }

    public static  event EventHandler<List<CameraPayLoad>>? OnPicturesArrive;
   

}