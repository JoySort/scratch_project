using System.Reflection;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.JoyHTTPClient;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Network;
using NLog;

namespace CommonLib.Lib.Worker;

public class ModuleCommunicationWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static ModuleCommunicationWorker me = new ModuleCommunicationWorker();
    public static ModuleCommunicationWorker getInstance()
    {
        return me;
    }
    private ModuleCommunicationWorker()
    {
        init();

    }

    private void init()
    {
        ProjectEventDispatcher.getInstance().ProjectStatusChanged += OnProjectStatusChange;
        foreach (var service in NetworkUtil.getInstance().DiscoveryServices)
        {
            service.EndPointDiscoverFound += onDiscoverEndPoint;
        }
    }
    
    private async Task registerRPCEndPoint(string ipAddr,int port,string uuid)
    {
        var url = buildUri(ipAddr, port, "/config/module");
        var result = await (new HTTPClientWorker()).GetFromRemote<ModuleConfig>(url);
        remoteConfig.Add(result.Module,result);
        var rpcEndpoint = new RpcEndPoint(port, ipAddr, uuid);
        rpcEndPoints.Add(uuid,rpcEndpoint);
        OnDiscovery?.Invoke(this,rpcEndpoint);
    }

    private  string buildUri(string ipAddr, int port,string endPoint)
    {
        return "http://" + ipAddr + ":" + port + "" + endPoint;
    }
    
    private Dictionary<string,RpcEndPoint> rpcEndPoints = new Dictionary<string,RpcEndPoint>();
    private Dictionary<JoyModule, ModuleConfig> remoteConfig = new Dictionary<JoyModule, ModuleConfig>();

    public Dictionary<string, RpcEndPoint> RpcEndPoints => rpcEndPoints;

    public Dictionary<JoyModule, ModuleConfig> RemoteConfig => remoteConfig;

    private void onDiscoverEndPoint(object sender, DiscoverFoundEventArgs arg)
    {
        registerRPCEndPoint(arg.ipAddr, arg.rpcPort,arg.uuid);
    }

    private void OnProjectStatusChange(object? sender, ProjectStatusEventArgs e)
    {
        throw new NotImplementedException();
    }
    public   event EventHandler<RpcEndPoint> OnDiscovery;
}

public class RpcEndPoint
{
    private int port;
    private string address;
    private string uuid;

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
}