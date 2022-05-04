using ZeroFormatter;

namespace CommonLib.Lib.ConfigVO;
[ZeroFormattable]
public class CameraConfig
{
    private string classDriver;
    private string brand;
    private string model;
    private string address;
    private int[] columns;
    private int[] offsets;
    private CameraPosition cameraPosition;
    private bool saveRawImage;
    private string savePath;

    private int gid;
    private int width;
    private int height;

    private CameraType cameraType;

    public CameraConfig(string classDriver, string brand, string model,string address, int[] columns,int[] offsets, CameraPosition cameraPosition, bool saveRawImage, string savePath,int width,int height,int gid,CameraType cameraType)
    {
        this.ClassDriver = classDriver;
        this.brand = brand;
        this.model = model;
        this.address = address;
        this.columns = columns;
        this.offsets = offsets;
        this.cameraPosition = cameraPosition;
        this.cameraType = cameraType;
        this.saveRawImage = saveRawImage;
        this.savePath = savePath;
        this.width = width;
        this.height = height;
        this.gid = gid;
    }
 
    public CameraConfig()
    {
    }
    [Index(0)]
    public virtual string Brand
    {
        get => brand;
        set => brand = value ;
    }
    [Index(1)]
    public virtual string Model
    {
        get => model;
        set => model = value ;
    }
    [Index(2)]
    public virtual string Address
    {
        get => address;
        set => address = value ;
    }
    [Index(3)]
    public virtual int[] Columns
    {
        get => columns;
        set => columns = value;
    }
    [Index(4)]
    public virtual int[] Offsets
    {
        get => offsets;
        set => offsets = value ;
    }
    [Index(5)]
    public virtual CameraPosition CameraPosition
    {
        get => cameraPosition;
        set => cameraPosition = value;
    }
    [Index(6)]
    public virtual bool SaveRawImage
    {
        get => saveRawImage;
        set => saveRawImage = value;
    }
    [Index(7)]
    public virtual string SavePath
    {
        get => savePath;
        set => savePath = value ;
    }
    [Index(8)]
    public virtual int Gid
    {
        get => gid;
        set => gid = value;
    }
    [Index(9)]
    public virtual int Width
    {
        get => width;
        set => width = value;
    }
    [Index(10)]
    public virtual int Height
    {
        get => height;
        set => height = value;
    }
    [Index(11)]
    public virtual CameraType CameraType
    {
        get => cameraType;
        set => cameraType = value;
    }
    [Index(12)]
    public virtual string ClassDriver { get => classDriver; set => classDriver = value; }
}

public enum CameraType
{ 
    IP,
    USB,
    CamLink,
    Other
}

public enum CameraPosition
{
    left,
    middle,
    right
}