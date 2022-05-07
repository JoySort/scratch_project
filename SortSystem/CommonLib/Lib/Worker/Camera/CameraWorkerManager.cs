using System.Collections.Concurrent;
using CommonLib.Lib.Camera;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Network;
using CommonLib.Lib.Util;
using CommonLib.Lib.Worker.Camera;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.Worker;

public class CameraWorkerManager
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static CameraWorkerManager me = new CameraWorkerManager();
    private bool running;

    private CameraWorkerManager()
    {
       // setup();
    }

    public static CameraWorkerManager getInstance()
    {
        return me;
    }
    public  void setup()
    {
        //Piple line wireup;
        List<ICameraDriver> cameraDrivers = CameraWorker.getInstance().CameraDrivers;
        
        //TCPChannelService.getInstance().initClient(ConfigUtil.getModuleConfig().NetworkConfig.TcpBindIp,ConfigUtil.getModuleConfig().NetworkConfig.TcpPort);

        ProjectManager.getInstance().ProjectStatusChanged += ProjectStatusChange;
        CameraWorker.getInstance().CameraDrivers.ForEach(value=>value.OnPictureArrive+=CameraTransmissionWorker.getInstance().onCameraPayLoad);
        CameraTransmissionWorker.getInstance().onCameraDataSent += processCameraSendingStats;
        
        
    }

    private void ProjectStatusChange(object? sender, ProjectStatusEventArgs e)
    {
        if (e.State == ProjectState.start)
        {
            running = true;
            statsQueue = new ConcurrentQueue<StatsHolder>();
            Task.Run(() =>
            {
                while (running)
                {
                    Thread.Sleep(5000);
                    printStats();
                }
            });
        }else if (e.State == ProjectState.stop)
        {
            running = false;
            statsQueue.Clear();
        }
    }

    public void tearDown()
    {
        running = false;
        statsQueue.Clear();
        ProjectManager.getInstance().ProjectStatusChanged -= ProjectStatusChange;
        CameraTransmissionWorker.getInstance().onCameraDataSent -= processCameraSendingStats;
        CameraWorker.CloseAllCams();
    }

    private ConcurrentQueue<StatsHolder> statsQueue;
    private void processCameraSendingStats(object sender, StatsHolder statsHolder)
    {
        if (statsQueue.Count > 100)
        {
            StatsHolder tmpStatsHolder = null;
            statsQueue.TryDequeue(out tmpStatsHolder);
        }
        statsQueue.Enqueue(statsHolder);
    }

    private void printStats()
    {
        if (statsQueue.Count == 0) return;
        var dic = JsonConvert.SerializeObject(statsQueue.Select(value => value.TriggerId).OrderBy(value => value)
            .GroupBy(value => value).ToDictionary(value => value.Key, value => value.ToArray().Length));
        
        
        logger.Info($"Camera Sending stats avg:{statsQueue.Select(value=>value.timeTook).Average()} min:{statsQueue.Select(value=>value.timeTook).Min()} max:{statsQueue.Select(value=>value.timeTook).Max()} " +
                    $"\n\r triggerids:{dic}");
    }
}