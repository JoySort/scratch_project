using CommonLib.Lib.ConfigVO;

namespace CommonLib.Lib.Camera;

public class IPCameraDriver:CameraDriverBase
{
    public IPCameraDriver(CameraConfig camConfig) : base(camConfig)
    {
    }
    public override void InitCam()
    {
        //TODO: code to init the camera
        throw new NotImplementedException();
    }
    public override void CloseCam()
    {
        //TODO: code to init the camera
        throw new NotImplementedException();
    }

    public override void processCameraData()
    {
        //TODO: code to init the camera
        //OnRecivingPicture(byte[] picture)
        throw new NotImplementedException();
    }
}