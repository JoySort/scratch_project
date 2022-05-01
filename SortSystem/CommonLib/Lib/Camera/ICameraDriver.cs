using CameraLib.Lib.LowerMachine;
using CameraLib.Lib.Sort.ResultVO;

namespace CameraLib.Lib.Camera;

public interface ICameraDriver
{
    void ProjectStatusChangeHandler(object sender, ProjectStatusEventArgs args);
    void InitCam();
    void CloseCam();
    void processCameraData();

    void onRecivingPicture(byte[] picture);
    
    event EventHandler<CameraPayLoad> OnPictureArrive;
}