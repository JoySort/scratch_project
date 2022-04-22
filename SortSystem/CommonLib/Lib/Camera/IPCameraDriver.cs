using CommonLib.Lib.ConfigVO;

namespace CommonLib.Lib.Camera;

public class IPCameraDriver:CameraDriverBase
{
    public IPCameraDriver(CameraConfig camConfig) : base(camConfig)
    {
    }
    public virtual void initCam()
    {
        //TODO: code to init the camera
        throw new NotImplementedException();
    }

    public virtual void processCameraData()
    {
        //TODO: code to init the camera
        //OnRecivingPicture(byte[] picture)
        throw new NotImplementedException();
    }
}