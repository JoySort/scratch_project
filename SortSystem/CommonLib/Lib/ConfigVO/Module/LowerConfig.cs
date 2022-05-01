namespace CommonLib.Lib.ConfigVO;

public class LowerConfig
{
    private string hardwarePort;
    private int baudRate;

    public int BaudRate => baudRate;

    private int[] columns;
    private bool isMaster;
    

    public LowerConfig(string hardwarePort, int baudRate, int[] columns, bool isMaster)
    {
        this.hardwarePort = hardwarePort;
        this.baudRate = baudRate;
        this.columns = columns;
        this.isMaster = isMaster;
    }

    public bool IsMaster => isMaster;

    public string HardwarePort => hardwarePort;
    

    public int[] Columns => columns;
}

