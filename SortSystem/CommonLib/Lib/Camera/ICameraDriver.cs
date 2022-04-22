using CommonLib.Lib.LowerMachine;

namespace CommonLib.Lib.Camera;

public interface ICameraDriver
{
    public abstract void ProjectStatusChangeHandler(object sender, ProjectStatusEventArgs args);
    public abstract void initCam();
    public abstract void processCameraData();

    public abstract void OnRecivingPicture(byte[] picture);
    
    public  event EventHandler<CameraPayLoad> OnPictureArrive;
}