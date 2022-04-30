using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
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
    const string strDllName = "datedll.dll";
    [DllImport(strDllName, EntryPoint = "ApplicationRecognize")]
    private static extern int ApplicationRecognize(byte[] buf, int height, int width, int[] outdata,
        int per_len = 20, int rows = 4, int cols = 6);
    [DllImport(strDllName, EntryPoint = "ApplicationRecognize")]
    private static extern int ApplicationRecognize(IntPtr buf, int height, int width, int[] outdata,
        int per_len = 20, int rows = 4, int cols = 6);

    [DllImport(strDllName, EntryPoint = "set_category")]
    public static extern void set_category(int ct, int func);

    [DllImport(strDllName, EntryPoint = "init_date_Algorithm")]
    public static extern int init_date_Algorithm();

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

        if (!ConfigUtil.getModuleConfig().RecognizerSimulationMode)
        {
            //var dllPath = ConfigUtil.getModuleConfig().RecognizerConfig.DllPaht;
            

            //var dllRelativeToRunnerPath = Path.Combine(
            //    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            //    dllPath);
            

            Task.Run(() =>
            {
                var recConfig = ConfigUtil.getModuleConfig().RecognizerConfig;
                int genreCode = (int)ConfigUtil.getModuleConfig().Genre;
                set_category(genreCode, 0x0fffffff);
                init_date_Algorithm();

                var initPicturePath = recConfig.InitializationImagePath;
                var initPicRelativeToRunnerPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                    initPicturePath);

                // load Dll with dllRelativeToRunnerPath

                byte[] picture = File.ReadAllBytes(initPicRelativeToRunnerPath); 
                int dataOffset = picture[10] 
                    + 256 * picture[11] 
                    + 256 * 256 * picture[12] 
                    + 256 * 256 * 256 * picture[13];
                int width = picture[18]
                    + 256 * picture[19]
                    + 256 * 256 * picture[20]
                    + 256 * 256 * 256 * picture[21];

                int height = picture[22]
                    + 256 * picture[23]
                    + 256 * 256 * picture[24]
                    + 256 * 256 * 256 * picture[25];

                int rows = recConfig.InitRows;
                int cols = recConfig.InitCols;

                IntPtr dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(picture, dataOffset);
                int[] outdata = new int[20 * rows * cols];
                ApplicationRecognize(dataPtr, height, width, outdata, 20, rows, cols);
                logger.Info("Recognization libaray initialized.");
                logger.Info("Sample picture result 1: "
                    + outdata[0]
                    + ","
                    + outdata[1]
                    + ","
                    + outdata[2]
                    + ","
                    + outdata[3]
                    + ","
                    + outdata[4]                      
                    + ","
                    + outdata[5]
                    );


            });
        }

    }
    

    private bool isProjectRunning = false;
    private Project? currentProject;

    private void ProjectStatusChangeHandler(object? sender, ProjectStatusEventArgs e)
    {
        if (e.State == ProjectState.start || e.State == ProjectState.update)
        {
            isProjectRunning = true;
            currentProject = e.currentProject;
            logger.Info("Recognizer received project start || change message.");
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
                if ((toBeRecognized.Count <= 0))
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
        List<RecResult>? results=null;
        if (ConfigUtil.getModuleConfig().RecognizerSimulationMode)
        {
            //这个模拟器并不生成这一张照片的一个识别结果，而是生成4张照片的结果一次性。因此不是一个严格的模拟器。
            results =  RecResultGenerator.prepareData(currentProject, payload.TriggerId, 1, payload.CamConfig.Columns, payload.CamConfig.CameraPosition,4);
            if (results.Last().Coordinate.TriggerId == 0)
            {
                logger.Debug($"TriggerID 0 triggerred");
            }         

        }
        else
        {
            results=new List<RecResult>();
            int width = payload.CamConfig.Width; ;
            int height = payload.CamConfig.Height;
            int cols = payload.CamConfig.Columns.Last() - payload.CamConfig.Columns.First() + 1;
            int rows = payload.CamConfig.Offsets.Last() - payload.CamConfig.Offsets.First() + 1;
            int[] outdata = new int[cols * rows * 20];

            if (currentProject != null)
            {
                ApplicationRecognize(payload.PictureData, height, width, outdata, 20, rows, cols);

                Criteria[] criterias = currentProject.Criterias;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        List<Feature> features = new List<Feature>();
                        for (int k = 0; k < criterias.Length; k++)
                        {
                            int index = criterias[k].Index;
                            features.Add(new Feature(index, outdata[(i*cols+j)*20+index]));
                        }
                        RecResult result = new RecResult(new Coordinate(j, rows-1-i, ConfigVO.CameraPosition.middle, payload.TriggerId),
                            5,DateTimeOffset.Now.ToUnixTimeMilliseconds(), features);
                        results.Add(result);

                    }
                }
            }
        }
        if(results!=null)
            dispatchResult(results);
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


    public  event EventHandler<List<RecResult>>? RecResultGenerated;
}
