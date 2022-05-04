using CommonLib.Lib.ConfigVO;
using ZeroFormatter;

namespace CommonLib.Lib.Sort.ResultVO;

[ZeroFormattable]
public class CameraPayLoad
{
    private CameraConfig camConfig;
    private byte[] pictureData;
    private long triggerID = 0;


    public CameraPayLoad(long triggerId, CameraConfig camConfig, byte[] pictureData)
    {
        triggerID = triggerId;
        this.camConfig = camConfig;
        this.pictureData = pictureData;
    }

    public CameraPayLoad()
    {
    }
    [Index(0)]
    public virtual CameraConfig CamConfig
    {
        get => camConfig;
        set => camConfig = value ?? throw new ArgumentNullException(nameof(value));
    }
    [IgnoreFormat]
    public virtual byte[] PictureData
    {
        get => pictureData;
        set => pictureData = value ;
    }
    [Index(1)]
    public virtual long TriggerId
    {
        get => triggerID;
        set => triggerID = value;
    }
}