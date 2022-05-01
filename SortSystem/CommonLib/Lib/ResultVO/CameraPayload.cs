using CommonLib.Lib.ConfigVO;

namespace CommonLib.Lib.Sort.ResultVO;

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