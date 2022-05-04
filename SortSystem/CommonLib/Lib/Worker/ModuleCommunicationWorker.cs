
using System.Collections.Concurrent;
using System.Reflection;
using CommonLib.Lib.vo;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.JoyHTTPClient;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Network;
using CommonLib.Lib.Util;
using NLog;

namespace CommonLib.Lib.Worker;

public class ModuleCommunicationWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static ModuleCommunicationWorker me = new ModuleCommunicationWorker();
    private  List<UDPDiscoveryService> _discoveryServices = new List<UDPDiscoveryService>();
     
    public static ModuleCommunicationWorker getInstance()
    {
        return me;
    }
    private ModuleCommunicationWorker()
    {
     
        init();

    }


    private void init() {
        
        var webPort = ConfigUtil.getModuleConfig().NetworkConfig.RpcPort;
        var tcp_port = ConfigUtil.getModuleConfig().NetworkConfig.TcpPort;
        var proxyRpcPort = ConfigUtil.getModuleConfig().NetworkConfig.RpcProxyPort;
        if (proxyRpcPort == 0) proxyRpcPort = webPort;
        var udp_ports =  ConfigUtil.getModuleConfig().NetworkConfig.DiscoveryPorts;
        var moduleName = ConfigUtil.getModuleConfig().Name;
        OnRemove+=onRemoveEndPoint;
        foreach (var udpPort in udp_ports)
        {
            var uppDiscoverService = new UDPDiscoveryService(webPort==proxyRpcPort?webPort:proxyRpcPort,tcp_port,udpPort,moduleName);
            uppDiscoverService.EndPointDiscoverFound += onDiscoverEndPoint;
           
            _discoveryServices.Add(uppDiscoverService);
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                uppDiscoverService.StartListen();
            });
           
        }

        checkAlive();
    }


    private int checkAliveCounter = 0;
    private  void checkAlive()
    {

        Task.Run(async () =>
        {
            while (true)
            {
                Thread.Sleep(10000* (int)(Math.Sqrt(checkAliveCounter+++1)));
                foreach ((var module, var rdps) in rpcEndPoints)
                {

                    Dictionary<string,RpcEndPoint> expiredEndpoints = new Dictionary<string,RpcEndPoint>();
                    foreach ((var Key, var item) in rdps)
                    {
                        var remoteURI =buildUri( item.Address ,item.WebPort,"/isAlive") ;
                        var result = await (new JoyHTTPClient.JoyHTTPClient()).GetFromRemote<WebControllerResult>(remoteURI);
                        if (result == null)
                        {
                            expiredEndpoints.Add(Key,item);
                            checkAliveCounter = 0;
                            //logger.Info($"Endpoint {Key} expired, removing it from Registry");
                        }
                    }

                    foreach ((var key,var value) in expiredEndpoints)
                    {
                        RpcEndPoint? tmp = null;
                        if (rdps.Remove(key, out tmp))
                        {
                            OnRemove?.Invoke(this, module);
                            logger.Info($"Invalid Enpoint has been removed from Registry {tmp.Key()}");
                        }
                    }
                }
            }
        });

        
    }

    private async Task registerRPCEndPoint(string ipAddr,int webPort,int tcpPort,string uuid)
    {
        var url = buildUri(ipAddr, webPort, "/config/module");
        var result = await (new JoyHTTPClient.JoyHTTPClient()).GetFromRemote<ModuleConfig>(url);
        if (result == null) return;
        
        var rpcEndpoint = new RpcEndPoint(webPort, tcpPort,ipAddr, uuid);
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

        logger.Info($"Registering remote endpoint {rpcEndpoint.ModuleConfig.Name} with address info {rpcEndpoint.Address}:{rpcEndpoint.WebPort} tcp port {rpcEndpoint.TcpPort} on Module {Enum.GetName(rpcEndpoint.ModuleConfig.Module)}");



        OnDiscovery?.Invoke(this,rpcEndpoint);
    }

    private  string buildUri(string ipAddr, int port,string endPoint)
    {
        return "http://" + ipAddr + ":" + port + "" + endPoint;
    }
    
    private ConcurrentDictionary<JoyModule,ConcurrentDictionary<string,RpcEndPoint>> rpcEndPoints = new ConcurrentDictionary<JoyModule,ConcurrentDictionary<string,RpcEndPoint>>();
  
    public ConcurrentDictionary<JoyModule, ConcurrentDictionary<string,RpcEndPoint>> RpcEndPoints => rpcEndPoints;

  
    private void onDiscoverEndPoint(object? sender, EndPointChangedArgs arg)
    {
        Task.Run(() =>
        {
            //delay discovery for 2 seconds for web api to properly startup
            Thread.Sleep(3000);
            registerRPCEndPoint(arg.ipAddr, arg.rpcPort,arg.tcpPort,arg.uuid);   
        });
       
    }

    private void onRemoveEndPoint(object? sender, JoyModule arg)
    {
        if(ConfigUtil.getModuleConfig().Module==JoyModule.Camera && arg == JoyModule.Upper){
            
            if(ProjectManager.getInstance().ProjectState!=ProjectState.stop)
                ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        }
    }


    public   event EventHandler<RpcEndPoint> OnDiscovery;
    public   event EventHandler<JoyModule> OnRemove;
}

public class RpcEndPoint
{
    private int webPort;
    private int tcpPort;

    public int TcpPort
    {
        get => tcpPort;
        set => tcpPort = value;
    }

    private string address;
    private string uuid;
    private ModuleConfig moduleConfig;

    public ModuleConfig ModuleConfig
    {
        get => moduleConfig;
        set => moduleConfig = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int WebPort => webPort;

    public string Uuid
    {
        get => uuid;
        set => uuid = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Address => address;

    public RpcEndPoint(int webPort,int tcpPort, string address,string uuid)
    {
        this.webPort = webPort;
        this.address = address;
        this.uuid = uuid;
        this.TcpPort = tcpPort;
    }

    public string Key()
    {
        return $"{this.Address}:{this.WebPort}:{this.TcpPort}";
    }
}