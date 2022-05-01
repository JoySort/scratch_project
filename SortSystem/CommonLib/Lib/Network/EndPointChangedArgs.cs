namespace CommonLib.Lib.Network;

public class EndPointChangedArgs:EventArgs

{
    public EndPointChangedArgs(string ipAddr, string uuid, int rpcPort)
    {
        this.ipAddr = ipAddr;
        this.uuid = uuid;
        this.rpcPort = rpcPort;
    }

    public string ipAddr { get; set; } = null!;
    public string uuid { get; set; }
    public int rpcPort { get; set; }
}