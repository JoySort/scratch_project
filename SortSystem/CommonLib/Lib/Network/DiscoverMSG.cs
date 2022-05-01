namespace CameraLib.Lib.Network;

public class DiscoverMSG
{


    public const string BROADCAST_ADDR = "255.255.255.255";

    public const string MSG_TYPE_ACK = "ACK";
    public const string MSG_TYPE_BRD = "BRD";

    private string uuid;
    
    private int rpcPort;
    private int listenPort;


    private string type = "BROADCAST";
    private int msgID = 0;
    public int ListenPort => listenPort;

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

    public DiscoverMSG(string uuid, int rpcPort, int listenPort, string type, int msgId)
    {
        this.uuid = uuid;
        this.rpcPort = rpcPort;
        this.listenPort = listenPort;
        this.type = type;
        msgID = msgId;
    }


    public int RpcPort => rpcPort;

    
    public override string ToString()
    {
        return "rpc port:" + rpcPort +
               " type:"+type+
               " msgID:"+msgID;
    }
}

