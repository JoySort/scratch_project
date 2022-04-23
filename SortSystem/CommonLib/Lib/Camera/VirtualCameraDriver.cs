using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;

namespace CommonLib.Lib.Camera;

public class VirtualCameraDriver:CameraDriverBase
{
    public VirtualCameraDriver(CameraConfig camConfig) : base(camConfig)
    {
    }

    public override  void initCam()
    {
        for (var i = 0; i < 4; i++)
        {
            var filename = i+1;
            byte[] picture = File.ReadAllBytes("assets/"+filename+".bmp");
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
                //Thread.Sleep(70);
                var fileIndex = filenameCounter++ % 4+1;
               
                OnRecivingPicture(pictures[fileIndex]);
            }
        });
    }
}