namespace CommonLib.Lib.Network;

public class DiscoverMSG
{


    public const string BROADCAST_ADDR = "255.255.255.255";

    public const string MSG_TYPE_ACK = "ACK";
    public const string MSG_TYPE_BRD = "BRD";

    private string uuid;
    
    private int rpcPort;
    private int udpPort;
    private int tcpPort;

    public int TcpPort
    {
        get => tcpPort;
        set => tcpPort = value;
    }

    private string type = "BROADCAST";
    private int msgID = 0;
    public int UdpPort => udpPort;

    public int Count
    {
        get => msgID;
        set => msgID = value;
    }

    public string Type
    {
        get => type;
        set => type = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Uuid
    {
        get => uuid;
        set => uuid = value ?? throw new ArgumentNullException(nameof(value));
    }

    public DiscoverMSG(string uuid, int rpcPort,int tcpPort, int udpPort, string type, int msgId)
    {
        this.uuid = uuid;
        this.rpcPort = rpcPort;
        this.udpPort = udpPort;
        this.type = type;
        this.tcpPort = tcpPort;
        msgID = msgId;
    }


    public int RpcPort => rpcPort;

    
    public override string ToString()
    {
        return $"DiscoverMSG ==> rpc port(web):{rpcPort} tcpPort:{tcpPort} type:{type} msgID:{msgID}";
    }
}

