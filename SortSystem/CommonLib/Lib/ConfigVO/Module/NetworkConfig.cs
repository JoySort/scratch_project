namespace CommonLib.Lib.ConfigVO;

public class NetworkConfig
{
    private int rpcPort;
    private int[] discoveryPorts;
    private string rpcBindIP;
    private string udpBindIP;

    public NetworkConfig(int rpcPort, int[] discoveryPorts, string rpcBindIp, string udpBindIp)
    {
        this.rpcPort = rpcPort;
        this.discoveryPorts = discoveryPorts;
        rpcBindIP = rpcBindIp;
        udpBindIP = udpBindIp;
    }

    public int RpcPort => rpcPort;

    public int[] DiscoveryPorts => discoveryPorts;

    public string RpcBindIp => rpcBindIP;

    public string UdpBindIp => udpBindIP;
}