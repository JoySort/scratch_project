namespace CommonLib.Lib.Network;

public class EndPointChangedArgs:EventArgs

{
    public EndPointChangedArgs(string ipAddr, string uuid, int rpcPort,int tcpPort)
    {
        this.ipAddr = ipAddr;
        this.uuid = uuid;
        this.rpcPort = rpcPort;
        this.tcpPort = tcpPort;

    }

    public string ipAddr { get; set; } = null!;
    public string uuid { get; set; }
    public int rpcPort { get; set; }
    public int tcpPort { get; set; }
}