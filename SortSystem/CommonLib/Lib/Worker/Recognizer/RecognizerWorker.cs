using System.Collections.Concurrent;
using System.Reflection;
using CommonLib.Lib.Camera;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using CommonLib.Lib.Worker.Camera;
using NLog;

namespace CommonLib.Lib.Worker.Recognizer;

public class RecognizerWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private RecognizerWorker()
    {
        init();
    }

    private static RecognizerWorker me = new RecognizerWorker();
    private ConcurrentQueue<CameraPayLoad> toBeRecognized = new ConcurrentQueue<CameraPayLoad>();
    public static RecognizerWorker getInstance()
    {
        return me;
    }

    private void init()
    {
        
        ProjectManager.getInstance().ProjectStatusChanged += ProjectStatusChangeHandler;
        
        List<ICameraDriver> cameraDrivers = CameraWorker.getInstance().CameraDrivers;
        
        foreach (var cameraDriver in cameraDrivers)
        {
            cameraDriver.OnPictureArrive+=((sender,cameraPayLoad)=>
            {
                if (cameraPayLoad.TriggerId == 0)
                {
                    logger.Debug("Recognizer trigger ID 0");
                }

                //这里使用队列来暂存对象，目的是把所有相机的照片顺序处理，确保只有一个线程调用dll，因为如果直接用相机进程调用，则会导致多个相机同时用不同线程调用识别，从而导致识别多线程运行。
                toBeRecognized.Enqueue(cameraPayLoad);
                //process(cameraPayLoad);
            });
        }
        
        
    }
    

    private bool isProjectRunning = false;
    private Project currentProject;

    private void ProjectStatusChangeHandler(object? sender, ProjectStatusEventArgs e)
    {
        if (e.State == ProjectState.start)
        {
            isProjectRunning = true;
            currentProject = e.currentProject;
            recognize();
        }

        if (e.State == ProjectState.stop)
        {
            //延时1秒，让识别线程可以把最后一批照片处理完毕，否则有可能会导致项目停止后，线程循环终止了。
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                isProjectRunning = false;
                currentProject = null;
            });
            
        }
    }

    private void recognize()
    {
        //这里启动一个线程，调用recognize，否则dll会崩溃。
        Task.Run(() =>
        {
            while (isProjectRunning)
            {
                if (!(toBeRecognized.Count > 0))
                {
                    Thread.Sleep(1);//当没有照片时，释放一下线程，有照片的时候，不停歇的进行识别。
                    continue;
                }
                CameraPayLoad tmpplc = null;
                if (toBeRecognized.TryDequeue(out tmpplc))
                {
                    process(tmpplc);
                }
            }
        });
    }

    private  void process(CameraPayLoad payload)
    {
        if (ConfigUtil.getModuleConfig().RecognizerSimulationMode)
        {
            //这个模拟器并不生成这一张照片的一个识别结果，而是生成4张照片的结果一次性。因此不是一个严格的模拟器。
            var result =  RecResultGenerator.prepareData(currentProject, payload.TriggerId, 1, payload.CamConfig.Columns, payload.CamConfig.CameraPosition,4);
            if (result.Last().Coordinate.TriggerId == 0)
            {
                logger.Debug($"TriggerID 0 triggerred");
            }

            dispatchResult(result);

        }
        else
        {
            var dllPath = ConfigUtil.getModuleConfig().RecognizerConfig.DllPaht;
            var initPicturePath = ConfigUtil.getModuleConfig().RecognizerConfig.InitializationImagePath;
            
            var dllRelativeToRunnerPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                dllPath);
            var initPicRelativeToRunnerPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                initPicturePath);
            
            // load Dll with dllRelativeToRunnerPath
            //byte[] picture = File.ReadAllBytes(initPicRelativeToRunnerPath);

        }

        return ;
    }

    private void dispatchResult(List<RecResult> result)
    {
        //单独启动一个线程来发送结果，否则发送结果可能会阻塞当前处理进程，从而造成耽误识别时间。
        Task.Run(() =>
        {
            RecResultGenerated?.Invoke(this,result);
        });
    }


    public  event EventHandler<List<RecResult>> RecResultGenerated;
}
