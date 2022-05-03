using System.Reflection;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.ConfigVO;
using NLog;

namespace CommonLib.Lib.Camera;

public class VirtualCameraDriver:CameraDriverBase
{
    private static Logger  logger = LogManager.GetCurrentClassLogger();
    public VirtualCameraDriver(CameraConfig camConfig) : base(camConfig)
    {
        logger.Info("相机模拟模式启动，初始化模拟相机驱动");
        
    }

    public override  void InitCam()
    {
        for (var i = 0; i < 4; i++)
        {
            var filename = i+1;
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                "assets/" + filename + ".bmp");
            byte[] picture = File.ReadAllBytes(path);
            pictures[i] = picture;
        }
    }

    private byte[][] pictures = new byte[4][] ;
    public override void processCameraData()
    {
        Task.Run(() =>
        {
            var filenameCounter = 0;
            var interval = 1000 / 14;
            Thread.Sleep(5000);
            while (isProjectRunning)
            {
                Thread.Sleep(filenameCounter < 14 ? (int)(1000/(filenameCounter==0?1:filenameCounter)):interval);
                if(filenameCounter == 0) logger.Debug($"Camera {CamConfig.Address}-{CamConfig.CameraPosition}-{CamConfig.Columns[0]}-{CamConfig.Columns[1]} starts to trigger");
                //Thread.Sleep(70);
                var fileIndex = filenameCounter++ % 4;
               
                onRecivingPicture(pictures[fileIndex]);
            }
        });
    }
}