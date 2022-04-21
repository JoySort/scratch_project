using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using CommonLib.Lib.JoyHTTPClient;
using CommonLib.Lib.Util;
using NLog;

namespace CommonLib.Lib.Network;

public class NetworkUtil
{
    private static Logger  logger = LogManager.GetCurrentClassLogger();
    private  List<UDPDiscoveryService> _discoveryServices = new List<UDPDiscoveryService>();

    public  List<UDPDiscoveryService> DiscoveryServices => _discoveryServices;

    private  Dictionary<string, string> rpcEndPoint = new Dictionary<string, string>();

    private NetworkUtil()
    {
    }

    private static NetworkUtil me = new NetworkUtil();
    public static NetworkUtil getInstance()
    {
        return me;
    }

    public  void UDPDiscoverSetup() {
        
        var rpc_port = ConfigUtil.getModuleConfig().NetworkConfig.RpcPort;
        var udp_ports =  ConfigUtil.getModuleConfig().NetworkConfig.DiscoveryPorts;
        var moduleName = ConfigUtil.getModuleConfig().Name;
        
        foreach (var port in udp_ports)
        {
            var uppDiscoverService = new UDPDiscoveryService(rpc_port,port,moduleName);
            uppDiscoverService.EndPointDiscoverFound += (object sender, DiscoverFoundEventArgs e) =>
            {
                logger.Info("Event catched "+e.ipAddr +" "+ e.rpcPort);
                //registerRPCEndPoint(e.ipAddr, e.rpcPort);
                
            };
            _discoveryServices.Add(uppDiscoverService);
            uppDiscoverService.StartListen();
        }
    }
    
   
    



}
