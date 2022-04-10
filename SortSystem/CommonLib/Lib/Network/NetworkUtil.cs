using CommonLib.Lib.Util;
using NLog;

namespace CommonLib.Lib.Network;

public class NetworkUtil
{
    private static Logger  logger = LogManager.GetCurrentClassLogger();
    private static List<UDPDiscoveryService> _discoveryServices = new List<UDPDiscoveryService>();
    private static Dictionary<string, string> rpcEndPoint = new Dictionary<string, string>(); 
    public static void UDPDiscoverSetup() {
        
        var rpc_port = ConfigUtil.getModuleConfig().Network.RpcPort;
        var udp_ports =  ConfigUtil.getModuleConfig().Network.DiscoveryPorts;
        var moduleName = ConfigUtil.getModuleConfig().Name;
        
        foreach (var port in udp_ports)
        {
            var uppDiscoverService = new UDPDiscoveryService(rpc_port,port,moduleName);
            uppDiscoverService.EndPointDiscoverFound += (object sender, DiscoverFoundEventArgs e) =>
            {
                logger.Info("Event catched "+e.ipAddr +" "+ e.rpcPort);
                registerRPCEndPoint(e.ipAddr, e.rpcPort);
            };
            _discoveryServices.Add(uppDiscoverService);
            uppDiscoverService.StartListen();
        }
    }

    public static void registerRPCEndPoint(string ipAddr,int port)
    {
        
    }


}