using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using NLog;
namespace CommonLib.Lib.Camera;

public abstract class CameraDriverBase:ICameraDriver
{
    protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

    protected CameraConfig camConfig;
    
    public CameraConfig CamConfig => camConfig;
    
    public CameraDriverBase(CameraConfig camConfig)
    {
        logger.Info($"初始化相机{camConfig.Address} camPosition:{camConfig.CameraPosition} GID:{camConfig.GID}");
        this.camConfig = camConfig;
        ProjectManager.getInstance().ProjectStatusChanged += ProjectStatusChangeHandler;
        InitCam();
    }

    //Ҳ��û��
    internal bool isProjectRunning = false;

    public virtual void ProjectStatusChangeHandler(object? sender, ProjectStatusEventArgs args)
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

    public virtual void CloseCam()
    {
        //TODO: code to shut down the camera
        throw new NotImplementedException();
    }

    public virtual void processCameraData()
    {
        //TODO: code to init the camera
        //OnRecivingPicture(byte[] picture)
        //throw new NotImplementedException();
    }

    internal long counter = 0;

    public void onRecivingPicture(byte[] picture)
    {
        OnPictureArrive?.Invoke(this,new CameraPayLoad(counter++,camConfig,picture));
    }


    public  event EventHandler<CameraPayLoad>? OnPictureArrive;
}

