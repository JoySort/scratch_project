using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using NLog;
namespace CommonLib.Lib.Camera;

public abstract class CameraDriverBase:ICameraDriver
{
    protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

    protected CameraConfig camConfig;
    
    public CameraConfig CamConfig => camConfig;
    
    public CameraDriverBase(CameraConfig camConfig)
    {
        this.camConfig = camConfig;
        ProjectManager.getInstance().ProjectStatusChanged += ProjectStatusChangeHandler;
        InitCam();
    }

    //也许没用
    internal bool isProjectRunning = false;

    public virtual void ProjectStatusChangeHandler(object sender, ProjectStatusEventArgs args)
    {
        if (args.State == ProjectState.stop)
        {
            isProjectRunning = false;
            
            
        }
        if (args.State == ProjectState.start)
        {
            counter = 0;
            isProjectRunning = true;
            processCameraData();
        }
        
    }

    public virtual void InitCam()
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

    public void onRecivingPicture(byte[] picture)
    {
        OnPictureArrive?.Invoke(this,new CameraPayLoad(counter++,camConfig,picture));
    }


    public  event EventHandler<CameraPayLoad>? OnPictureArrive;
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