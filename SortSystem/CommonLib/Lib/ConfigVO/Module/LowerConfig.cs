namespace CommonLib.Lib.ConfigVO;

public class LowerConfig
{
    private string hardwarePort;
    private int[] columns;
    private bool isMaster;

    public LowerConfig(string hardwarePort, int[] columns,bool isMaster)
    {
        this.hardwarePort = hardwarePort;
        this.columns = columns;
        this.isMaster = isMaster;
    }

    public bool IsMaster => isMaster;

    public string HardwarePort => hardwarePort;

    public int[] Columns => columns;
}