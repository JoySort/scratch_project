using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;

namespace CommonLib.Lib.Camera;

public abstract class CameraDriverBase:ICameraDriver
{
    private CameraConfig camConfig;
    
    public CameraConfig CamConfig => camConfig;
    
    public CameraDriverBase(CameraConfig camConfig)
    {
        this.camConfig = camConfig;
        ProjectManager.getInstance().ProjectStatusChanged += ProjectStatusChangeHandler;
        initCam();
    }

    internal bool isProjectRunning = false;

    public virtual void ProjectStatusChangeHandler(object sender, ProjectStatusEventArgs args)
    {
        if (args.State == ProjectState.stop)
        {
            isProjectRunning = false;
            counter = 0;
            
        }
        if (args.State == ProjectState.start)
        {
            isProjectRunning = true;
            processCameraData();
        }
        
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

    internal long counter = 0;

    public void OnRecivingPicture(byte[] picture)
    {
        OnPictureArrive?.Invoke(this,new CameraPayLoad(counter++,camConfig,picture));
    }


    public  event EventHandler<CameraPayLoad> OnPictureArrive;
}

public class CameraPayLoad
{
    private long triggerID = 0;
    private CameraConfig camConfig;
    private byte[] pictureData;


    public CameraPayLoad(long triggerId, CameraConfig camConfig, byte[] pictureData)
    {
        triggerID = triggerId;
        this.camConfig = camConfig;
        this.pictureData = pictureData;
    }

    public long TriggerId => triggerID;

    public byte[] PictureData => pictureData;

    public CameraConfig CamConfig => camConfig;
}