
using MessagePack;

namespace CommonLib.Lib.ConfigVO;
[MessagePackObject(keyAsPropertyName: true)]
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
  
    public virtual string Brand
    {
        get => brand;
        set => brand = value ;
    }

    public virtual string Model
    {
        get => model;
        set => model = value ;
    }
 
    public virtual string Address
    {
        get => address;
        set => address = value ;
    }
  
    public virtual int[] Columns
    {
        get => columns;
        set => columns = value;
    }
  
    public virtual int[] Offsets
    {
        get => offsets;
        set => offsets = value ;
    }
   
    public virtual CameraPosition CameraPosition
    {
        get => cameraPosition;
        set => cameraPosition = value;
    }

    public virtual bool SaveRawImage
    {
        get => saveRawImage;
        set => saveRawImage = value;
    }
  
    public virtual string SavePath
    {
        get => savePath;
        set => savePath = value ;
    }
  
    public virtual int Gid
    {
        get => gid;
        set => gid = value;
    }

    public virtual int Width
    {
        get => width;
        set => width = value;
    }

    public virtual int Height
    {
        get => height;
        set => height = value;
    }

    public virtual CameraType CameraType
    {
        get => cameraType;
        set => cameraType = value;
    }
 
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