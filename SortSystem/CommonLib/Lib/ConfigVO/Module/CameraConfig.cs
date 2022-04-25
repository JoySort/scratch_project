namespace CommonLib.Lib.ConfigVO;

public class CameraConfig
{
    private string address;
    private int[] columns;
    private CameraPosition cameraPosition;
    private bool saveRawImage;
    private string savePath;

    private int width;
    private int height;

    public CameraConfig(string address, int[] columns, CameraPosition cameraPosition, bool saveRawImage, string savePath,int width,int height)
    {
        this.address = address;
        this.columns = columns;
        this.cameraPosition = cameraPosition;
        this.saveRawImage = saveRawImage;
        this.savePath = savePath;
        this.width = width;
        this.height = height;
    }

    public string SavePath => savePath;

    public string Address => address;

    public bool SaveRawImage => saveRawImage;

    public int[] Columns => columns;

    public CameraPosition CameraPosition => cameraPosition;

    public int Width => width;
    public int Height => height;
}

public enum CameraPosition
{
    left,
    middle,
    right
}