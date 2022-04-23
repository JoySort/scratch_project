using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using CommonLib.Lib.Camera;
using CommonLib.Lib.Util;
using NLog;

namespace CommonLib.Lib.Worker.Camera;

public class CameraWorker
{ 
    
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private List<ICameraDriver> cameraDrivers = new List<ICameraDriver>();

    public List<ICameraDriver> CameraDrivers => cameraDrivers;

    private CameraWorker()
    {
        init();
    }

    private static CameraWorker me = new CameraWorker();

    public static CameraWorker getInstance()
    {
        return me;
    }

    private void init()
    {
        var cameraConfigs = ConfigUtil.getModuleConfig().CameraConfigs;
        var isSimulation = ConfigUtil.getModuleConfig().CameraSimulationMode;
        timestamp =  GetTimestamp(DateTime.Now);//每次项目启动的时候，设置一个启动时间戳，照片存储的时候，每个项目的照片名是 {triggerID}-{项目启动时间戳} .bmp 存储于 配置的存储目录下的项目启动时间戳文件夹 {项目启动时间戳} 可以方便区分每批照片,以后就算拷贝到一起也不会重名。
        foreach (var item in cameraConfigs)
        {

            CameraDriverBase cameraDriver = isSimulation ? new VirtualCameraDriver(item) : new IPCameraDriver(item);
            if (item.SaveRawImage) cameraDriver.OnPictureArrive += ((sender, cameraPayLoad) =>
            {
                CameraPayLoad tmpCameraPayLoad = null;
                
                //if (toBeSavedPictures.Count > 20) toBeSavedPictures.TryDequeue(out tmpCameraPayLoad);//内存中只缓存20个照片（5个triggerID)
                toBeSavedPictures.Enqueue(cameraPayLoad);
                
            });
            cameraDrivers.Add(cameraDriver);
        }

        savePicture();


    }

    private string timestamp = null;
    private ConcurrentQueue<CameraPayLoad> toBeSavedPictures = new ConcurrentQueue<CameraPayLoad>();
    
    //出现异常或者项目停止的时候可以调用。TODO：后续完成存储逻辑
    private void savePicture()
    {
        Task.Run(() =>
        {
           
            while(true){
                Thread.Sleep(10);
                if (toBeSavedPictures.Count == 0) continue;
                CameraPayLoad cameraPayLoad;
                if (toBeSavedPictures.TryDequeue(out cameraPayLoad))
                {
                    if (!cameraPayLoad.CamConfig.SaveRawImage) continue;//如果设置为不存图，则直接返回。
                    var path = Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                        cameraPayLoad.CamConfig.SavePath)+"/"+timestamp;
                    bool dirExists = Directory.Exists(path);
                    if (!dirExists)
                    {
                        DirectoryInfo di = Directory.CreateDirectory(path);
                    }
                    //triggerID补足7位，比如1是0000001 避免排序的时候出问题。
                    var triggerID = cameraPayLoad.TriggerId.ToString("D7");
                        
                    var filename = triggerID+"-"+timestamp+".bmp";
                    File.WriteAllBytes(path+"/"+filename, cameraPayLoad.PictureData);
                }
            }
        });
    }
    
    

    public static String GetTimestamp(DateTime value)
    {
        return value.ToString("yyyy-MM-dd_HH-mm-ss_ffff");
    }


}