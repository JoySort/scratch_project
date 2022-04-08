namespace CommonLib.Lib.ConfigVO;

public class Network
{
    private int rpcPort;
    private int[] discoveryPorts;
    private string rpcBindIP;
    private string udpBindIP;

    public Network(int rpcPort, int[] discoveryPorts, string rpcBindIp, string udpBindIp)
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