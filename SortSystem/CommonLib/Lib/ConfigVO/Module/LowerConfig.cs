namespace CommonLib.Lib.ConfigVO;

public class LowerConfig
{
    private string hardwarePort;
    private int[] columns;

    public LowerConfig(string hardwarePort, int[] columns)
    {
        this.hardwarePort = hardwarePort;
        this.columns = columns;
    }

    public string HardwarePort => hardwarePort;

    public int[] Columns => columns;
}