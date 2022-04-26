namespace CommonLib.Lib.ConfigVO;

public class CameraConfig
{
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

    public CameraConfig(string brand, string model,string address, int[] columns,int[] offsets, CameraPosition cameraPosition, bool saveRawImage, string savePath,int width,int height,int gid,CameraType cameraType)
    {
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

    public string Brand => brand;
    public string Model => model;

    public string SavePath => savePath;

    public string Address => address;

    public bool SaveRawImage => saveRawImage;

    public int[] Columns => columns;

    public int[] Offsets => offsets;

    public CameraPosition CameraPosition => cameraPosition;

    public CameraType CameraType => cameraType;

    public int GID => gid;
    public int Width => width;
    public int Height => height;
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