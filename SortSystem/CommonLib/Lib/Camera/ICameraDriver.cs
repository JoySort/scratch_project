using CommonLib.Lib.LowerMachine;

namespace CommonLib.Lib.Camera;

public interface ICameraDriver
{
    void ProjectStatusChangeHandler(object sender, ProjectStatusEventArgs args);
    void InitCam();
    void processCameraData();

    void onRecivingPicture(byte[] picture);
    
    event EventHandler<CameraPayLoad> OnPictureArrive;
}