namespace NetworkLib.Discovery;

public class DiscoverMSG
{

    public const int RPC_PORT = 13652;
    public const int DISCOVER_PORT = 13655;
    public const string BROADCAST_ADDR = "255.255.255.255";

    public const string MSG_TYPE_ACK = "ACK";
    public const string MSG_TYPE_BRD = "BRD";
    
    
    private int rpcPort = RPC_PORT;
    private string type = "BROADCAST";
    private int msgID = 0;

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


    public DiscoverMSG(int rpcPort,string type,int msgId)
    {
        this.rpcPort = rpcPort;
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

