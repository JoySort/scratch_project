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
        foreach (var service in NetworkUtil.DiscoveryServices)
        {
            service.EndPointDiscoverFound += onDiscoverEndPoint;
        }
    }

    private List<RpcEndPoint> rpcEndPoints = new List<RpcEndPoint>();
    private void onDiscoverEndPoint(object sender, DiscoverFoundEventArgs arg)
    {
        
    }

    private void OnProjectStatusChange(object? sender, ProjectStatusEventArgs e)
    {
        throw new NotImplementedException();
    }
    
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

    public RpcEndPoint(int port, string address)
    {
        this.port = port;
        this.address = address;
    }
}