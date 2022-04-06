namespace NetworkLib.Discovery;

public class Announcement
{
    public const string ROLE_CAMERA = "camera";
    public const string ROLE_UPPER = "upper";
    public const string ROLE_lower = "lower";
    public const string ROLE_UISERVER = "UISERVER";
    
    
    public int RemoteRpcPort
    {
        get => remote_rpc_port;
        set => remote_rpc_port = value;
    }


    public const int RPC_PORT = 13652;
    public const int DISCOVER_PORT = 13655;
    private string role;
    private int local_discover_port = DISCOVER_PORT;
    private int remote_rpc_port;

    private string remote_address;



    public string Role => role;
    

    public int LocalDiscoverPort => local_discover_port;

    public Announcement(string role)
    {
        this.role = role;
    }

    public Announcement(string role, int localRpcPort, int localDiscoverPort)
    {
        this.role = role;
        local_discover_port = localDiscoverPort;
    }
    
    public string RemoteAddress
    {
        get => remote_address;
        set => remote_address = value ?? throw new ArgumentNullException(nameof(value));
    }

}

