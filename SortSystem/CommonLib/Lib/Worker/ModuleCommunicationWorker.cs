
using System.Collections.Concurrent;
using CommonLib.Lib.ConfigVO;

using CommonLib.Lib.Network;
using CommonLib.Lib.Util;
using NLog;

namespace CommonLib.Lib.Worker;

public class ModuleCommunicationWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static ModuleCommunicationWorker me = new ModuleCommunicationWorker();
    private  List<UDPDiscoveryService> _discoveryServices = new List<UDPDiscoveryService>();
    private  Dictionary<string, string> rpcEndPoint = new Dictionary<string, string>();
    
    public static ModuleCommunicationWorker getInstance()
    {
        return me;
    }
    private ModuleCommunicationWorker()
    {
        init();

    }


    private void init() {
        
        var rpc_port = ConfigUtil.getModuleConfig().NetworkConfig.RpcPort;
        var proxyRpcPort = ConfigUtil.getModuleConfig().NetworkConfig.RpcProxyPort;
        if (proxyRpcPort == 0) proxyRpcPort = rpc_port;
        var udp_ports =  ConfigUtil.getModuleConfig().NetworkConfig.DiscoveryPorts;
        var moduleName = ConfigUtil.getModuleConfig().Name;
        
        foreach (var port in udp_ports)
        {
            var uppDiscoverService = new UDPDiscoveryService(rpc_port==proxyRpcPort?rpc_port:proxyRpcPort,port,moduleName);
            uppDiscoverService.EndPointDiscoverFound += onDiscoverEndPoint;
            _discoveryServices.Add(uppDiscoverService);
            uppDiscoverService.StartListen();
        }
    }
    private async Task registerRPCEndPoint(string ipAddr,int port,string uuid)
    {
        var url = buildUri(ipAddr, port, "/config/module");
        var result = await (new JoyHTTPClient.JoyHTTPClient()).GetFromRemote<ModuleConfig>(url);
        if (result == null) return;
        
        var rpcEndpoint = new RpcEndPoint(port, ipAddr, uuid);
        rpcEndpoint.ModuleConfig = result;
        if(!rpcEndPoints.ContainsKey(result.Module))rpcEndPoints.TryAdd(result.Module,new ConcurrentDictionary<string,RpcEndPoint>());

        if (rpcEndPoints[result.Module].ContainsKey(rpcEndpoint.Key()))
        {
            rpcEndPoints[result.Module][rpcEndpoint.Key()] = rpcEndpoint;
        }
        else
        {
            rpcEndPoints[result.Module].TryAdd(rpcEndpoint.Key(),rpcEndpoint);
        }





        OnDiscovery?.Invoke(this,rpcEndpoint);
    }

    private  string buildUri(string ipAddr, int port,string endPoint)
    {
        return "http://" + ipAddr + ":" + port + "" + endPoint;
    }
    
    private ConcurrentDictionary<JoyModule,ConcurrentDictionary<string,RpcEndPoint>> rpcEndPoints = new ConcurrentDictionary<JoyModule,ConcurrentDictionary<string,RpcEndPoint>>();
  
    public ConcurrentDictionary<JoyModule, ConcurrentDictionary<string,RpcEndPoint>> RpcEndPoints => rpcEndPoints;

  
    private void onDiscoverEndPoint(object sender, DiscoverFoundEventArgs arg)
    {
        registerRPCEndPoint(arg.ipAddr, arg.rpcPort,arg.uuid);
    }


    public   event EventHandler<RpcEndPoint> OnDiscovery;
}

public class RpcEndPoint
{
    private int port;
    private string address;
    private string uuid;
    private ModuleConfig moduleConfig;

    public ModuleConfig ModuleConfig
    {
        get => moduleConfig;
        set => moduleConfig = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int Port => port;

    public string Uuid
    {
        get => uuid;
        set => uuid = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Address => address;

    public RpcEndPoint(int port, string address,string uuid)
    {
        this.port = port;
        this.address = address;
        this.uuid = uuid;
    }

    public string Key()
    {
        return this.Address +":"+ this.Port;
    }
}