namespace CommonLib.Lib.ConfigVO;

public class NetworkConfig
{
    private int rpcPort;
    private int rpcProxyPort;
    private int tcpPort;

    public NetworkConfig(int rpcPort,int tcpPort, int rpcProxyPort, int[] discoveryPorts, string rpcBindIp, string udpBindIp,string tcpBindIP)
    {
        this.rpcPort = rpcPort;
        this.rpcProxyPort = rpcProxyPort;
        this.discoveryPorts = discoveryPorts;
        this.tcpPort = tcpPort;
        rpcBindIP = rpcBindIp;
        udpBindIP = udpBindIp;
    }

    public int TcpPort => tcpPort;

    private int[] discoveryPorts;
    private string tcpBindIP;
    private string rpcBindIP;
    private string udpBindIP;

    public int RpcProxyPort => rpcProxyPort;



    public int RpcPort => rpcPort;

    public int[] DiscoveryPorts => discoveryPorts;

    public string TcpBindIp
    {
        get => tcpBindIP;
        set => tcpBindIP = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string RpcBindIp
    {
        get => rpcBindIP;
        set => rpcBindIP = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string UdpBindIp
    {
        get => udpBindIP;
        set => udpBindIP = value ?? throw new ArgumentNullException(nameof(value));
    }
}