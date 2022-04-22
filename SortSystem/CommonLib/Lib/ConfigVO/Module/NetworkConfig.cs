namespace CommonLib.Lib.ConfigVO;

public class NetworkConfig
{
    private int rpcPort;
    private int rpcProxyPort;

    public NetworkConfig(int rpcPort, int rpcProxyPort, int[] discoveryPorts, string rpcBindIp, string udpBindIp)
    {
        this.rpcPort = rpcPort;
        this.rpcProxyPort = rpcProxyPort;
        this.discoveryPorts = discoveryPorts;
        rpcBindIP = rpcBindIp;
        udpBindIP = udpBindIp;
    }

    private int[] discoveryPorts;
    private string rpcBindIP;
    private string udpBindIP;

    public int RpcProxyPort => rpcProxyPort;



    public int RpcPort => rpcPort;

    public int[] DiscoveryPorts => discoveryPorts;

    public string RpcBindIp => rpcBindIP;

    public string UdpBindIp => udpBindIP;
}