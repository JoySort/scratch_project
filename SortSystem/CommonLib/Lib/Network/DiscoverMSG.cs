namespace CommonLib.Lib.Network;

public class DiscoverMSG
{


    public const string BROADCAST_ADDR = "255.255.255.255";

    public const string MSG_TYPE_ACK = "ACK";
    public const string MSG_TYPE_BRD = "BRD";


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


    public DiscoverMSG(int rpcPort,int listenPort,string type,int msgId)
    {
        this.rpcPort = rpcPort;
        this.listenPort = listenPort;
        this.type = type;
        this.msgID = msgId;


    }

    public int RpcPort => rpcPort;

    
    public string ToString()
    {
        return "rpc port:" + rpcPort +
               " type:"+type+
               " msgID:"+msgID;
    }
}

