namespace NetworkLib.Discovery;

public class DiscoverMSG
{

    public const int RPC_PORT = 13652;
    public const int DISCOVER_PORT = 13655;
    public const string BROADCAST_ADDR = "255.255.255.255";

    
    
    private int rpcPort = RPC_PORT;




    public DiscoverMSG(int rpcPort)
    {
        this.rpcPort = rpcPort;
  

    }

    public int RpcPort => rpcPort;

    
    public string toString()
    {
        return "rpc port:" + rpcPort;
    }
}

