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
    private List<CameraDriverBase> ipCameras = new List<CameraDriverBase>();

    public List<CameraDriverBase> IpCameras => ipCameras;

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
        foreach (var item in cameraConfigs)
        {

            CameraDriverBase cameraDriver = isSimulation ? new VirtualCameraDriver(item) : new IPCameraDriver(item);
            if (item.SaveRawImage) cameraDriver.OnPictureArrive += ((sender, cameraPayLoad) =>
            {
                toBeSavedPictures.Enqueue(cameraPayLoad);
            });
            ipCameras.Add(cameraDriver);
        }

        savePicture();
    }

    private ConcurrentQueue<CameraPayLoad> toBeSavedPictures = new ConcurrentQueue<CameraPayLoad>();
    private void savePicture()
    {
        Task.Run(() =>
        {
            while (true)
            {
                
                Thread.Sleep(10);
                if (toBeSavedPictures.Count == 0) continue;
                CameraPayLoad cameraPayLoad;
                if (toBeSavedPictures.TryDequeue(out cameraPayLoad))
                {
                    var path = Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                        cameraPayLoad.CamConfig.SavePath);
                    bool dirExists = Directory.Exists(path);
                    if (!dirExists)
                    {
                        DirectoryInfo di = Directory.CreateDirectory(path);
                        var timestamp = GetTimestamp(DateTime.Now);
                        var triggerID = cameraPayLoad.TriggerId.ToString("D7");
                        
                        var filename = triggerID + "-"+timestamp+".bmp";
                        File.WriteAllBytes(path+"/"+filename, cameraPayLoad.PictureData);
                    }
                }
            }
        });
    }

    public static String GetTimestamp(DateTime value)
    {
        return value.ToString("yyyy-MM-dd_HH-mm-ss_ffff");
    }


}