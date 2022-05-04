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
        logger.Info($"Initialize camera : {camConfig.Address} camPosition:{camConfig.CameraPosition} GID:{camConfig.Gid} Column coverage {string.Join(",",camConfig.Columns)} rowOffset:{string.Join(",",camConfig.Offsets)}");
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

