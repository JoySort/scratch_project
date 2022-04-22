namespace CommonLib.Lib.ConfigVO;

public class CameraConfig
{
    private string ipAddress;
    private int[] columns;
    private CameraPosition cameraPosition;

    public CameraConfig(string ipAddress, int[] columns, CameraPosition cameraPosition)
    {
        this.ipAddress = ipAddress;
        this.columns = columns;
        this.cameraPosition = cameraPosition;
    }

    public string IpAddress => ipAddress;

    public int[] Columns => columns;

    public CameraPosition CameraPosition => cameraPosition;
}

public enum CameraPosition
{
    left,
    middle,
    right
}