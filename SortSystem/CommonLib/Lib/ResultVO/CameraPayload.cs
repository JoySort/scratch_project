using CommonLib.Lib.ConfigVO;
using MessagePack;

namespace CommonLib.Lib.Sort.ResultVO;

[MessagePackObject(keyAsPropertyName: true)]
public class CameraPayLoad
{
    private CameraConfig camConfig;
    private byte[] pictureData;
    private long triggerID = 0;
    public long startTime;

    public CameraPayLoad(long triggerId,long startTime, CameraConfig camConfig, byte[] pictureData)
    {
        triggerID = triggerId;
        this.camConfig = camConfig;
        this.pictureData = pictureData;
        this.startTime = startTime;
    }

    public CameraPayLoad()
    {
    }

    public virtual CameraConfig CamConfig
    {
        get => camConfig;
        set => camConfig = value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual byte[] PictureData
    {
        get => pictureData;
        set => pictureData = value ;
    }

    public virtual long TriggerId
    {
        get => triggerID;
        set => triggerID = value;
    }
}