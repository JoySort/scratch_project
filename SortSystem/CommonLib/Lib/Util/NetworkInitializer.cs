using NetworkLib.Discovery;
using NLog;

namespace CommonLib.Lib.Util;

public class NetworkInitializer
{
    private static Logger  logger = LogManager.GetCurrentClassLogger();
    
    public static void UDPDiscoverSetup() {
        
        var rpc_port = ConfigUtil.loadModuleConfig().Network.RpcPort;
        var udp_ports =  ConfigUtil.loadModuleConfig().Network.DiscoveryPorts;
        var moduleName = ConfigUtil.loadModuleConfig().Name;
        
        foreach (var port in udp_ports)
        {
            var uppDiscoverService = new UDPDiscoveryService(rpc_port,port,moduleName);
            uppDiscoverService.EndPointDiscoverFound += (object sender, DiscoverFoundEventArgs e) =>
            {
                logger.Info("Event catched "+e.ipAddr +" "+ e.rpcPort);
            };
            uppDiscoverService.StartListen();
        }

        
      
    }
}