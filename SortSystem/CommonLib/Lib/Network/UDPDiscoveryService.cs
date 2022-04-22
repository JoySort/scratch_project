using System.Net;
using System.Net.Sockets;
using System.Text;
using CommonLib.Lib.Util;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.Network;

public class UDPDiscoveryService
{
    //LogManager.LoadConfiguration("config/logger.config"); 
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private IPAddress bindAddress;
    private long lastRestartTimestamp;
    private readonly DiscoverMSG localDiscoverMsg;
    private readonly List<string> localIps = new();

    private int restartNetworkAdaptorCount;
    private readonly string serviceName = "default name";
    private UdpClient udpClient;
    private string uuid;

    public string Uuid
    {
        get => uuid;
        set => uuid = value ?? throw new ArgumentNullException(nameof(value));
    }

    public UDPDiscoveryService(int rpc_port, int listenPort, string serviceName)
    {
        this.serviceName = serviceName;
        this.ListenPort = listenPort;
        
        
       
        uuid=ConfigUtil.getModuleConfig().Uuid;
        localDiscoverMsg = new DiscoverMSG(uuid,rpc_port, listenPort, DiscoverMSG.MSG_TYPE_BRD, 0);
        logger.Info("Discovery Service {} listen at {} reporting rpc {} with uuid {}initialized",serviceName,listenPort,rpc_port,uuid);
    }

    public bool UnitTestFlag { get; set; } = false;
    public int ListenPort { get; set; }

    public int KeepAliveInterval { get; set; } = 1000 * 3;
    private int msgCounter = 0;


    public bool ExitFlag { get; set; }
    public event EventHandler<DiscoverFoundEventArgs> EndPointDiscoverFound;

    protected virtual void OnEndPointDiscoverFound(DiscoverFoundEventArgs e)
    {
        var handler = EndPointDiscoverFound;
        handler?.Invoke(this, e);
    }

    public void StartListen()
    {
        StartListen(IPAddress.Any);
    }

    public void StartListen(IPAddress bindAddr)
    {
        initLocalIps();
        bindAddress = bindAddr;
        udpClient = new UdpClient();
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udpClient.Client.Bind(new IPEndPoint(bindAddress, ListenPort));

        var from = new IPEndPoint(0, 0);
        Task.Run(() =>
        {
            while (!ExitFlag)
            {
                var recvBuffer = udpClient.Receive(ref from);

                var msg = Encoding.UTF8.GetString(recvBuffer);
                var peerDiscoverMsg = JsonConvert.DeserializeObject<DiscoverMSG>(msg);

                var fromIP = from.Address.ToString();
                var fromPort = int.Parse(from.Port.ToString());

                var targetKey = fromIP + ":" + fromPort;

                //if (!UnitTestFlag && localIps.Contains(fromIP)) continue; // ignore local msgs;
                if (this.uuid.Equals(peerDiscoverMsg.Uuid)) continue;
                if (peerDiscoverMsg.Type == DiscoverMSG.MSG_TYPE_BRD)
                {
                    sendResponds(fromIP, fromPort, peerDiscoverMsg.Count);

                    
                    dispatchDiscovery(peerDiscoverMsg.RpcPort, fromIP,peerDiscoverMsg.Uuid);

                    logger.Debug("[" + serviceName + "]" + "BROADCAST From : " + fromIP + ":" + from.Port +
                                 " msg count: " +
                                 peerDiscoverMsg.Count + "  content: " + peerDiscoverMsg.ToString());
                }
                else
                {
                    dispatchDiscovery(peerDiscoverMsg.RpcPort, fromIP,peerDiscoverMsg.Uuid);
                    logger.Debug("[" + serviceName + "]" + "ACK From : " + fromIP + ":" + from.Port +
                                 "  content: " +
                                 peerDiscoverMsg.ToString());
                }
            }
        });

        Task.Run(() =>
        {
            SendAnnouncement();
            while (!ExitFlag)
            {
                try
                {
                    checkForNetWorkAdaptorChange();
                }
                catch (Exception e)
                {
                    logger.Error("Exception when detect network status change, error msg:{}",e.Message);
                }

                Thread.Sleep(KeepAliveInterval*10);
            }
        });
    }

    private void dispatchDiscovery(int rpcPort, string ip,string UUID)
    {

        //simulator use rpcPort =-1 to avoid being found.
        if (rpcPort == -1) return;
        var discoverEventArgs = new DiscoverFoundEventArgs
        {
            ipAddr = ip,
            rpcPort = rpcPort,
            uuid = UUID
        };
        OnEndPointDiscoverFound(discoverEventArgs);
    }

    private void sendResponds(string fromIP, int fromPort, int msgID)
    {
        var respondDiscoverMsg = new DiscoverMSG(uuid,localDiscoverMsg.RpcPort, ListenPort, DiscoverMSG.MSG_TYPE_ACK, msgID);
        var msg = JsonConvert.SerializeObject(respondDiscoverMsg);
        var data = Encoding.UTF8.GetBytes(msg);
        udpClient.Send(data, data.Length, DiscoverMSG.BROADCAST_ADDR, fromPort);
    }


    public void SendAnnouncement()
    {
        msgCounter++;
        localDiscoverMsg.Type = DiscoverMSG.MSG_TYPE_BRD;
        localDiscoverMsg.Count = msgCounter;
        var msg = JsonConvert.SerializeObject(localDiscoverMsg);
        var data = Encoding.UTF8.GetBytes(msg);
        udpClient.Send(data, data.Length, DiscoverMSG.BROADCAST_ADDR, ListenPort);
    }


    private void checkForNetWorkAdaptorChange()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var tmpLocalIps = new List<string>();
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                tmpLocalIps.Add(ip.ToString());


        if (localIps.Count > 0 && localIps.Count != tmpLocalIps.Count)
        {
            restartDiscoveryOnNetworkChange(tmpLocalIps);
            //throw new Exception(message);
        }
        else
        {
            localIps.Sort();
            tmpLocalIps.Sort();
            if (localIps.Count > 0 && !localIps.SequenceEqual(tmpLocalIps))
                restartDiscoveryOnNetworkChange(tmpLocalIps);
        }
    }

    private void restartDiscoveryOnNetworkChange(List<string> tempIps)
    {
        var timeDiff = DateTime.Now.ToFileTime() - lastRestartTimestamp;
 

        restartNetworkAdaptorCount++;
        lastRestartTimestamp = DateTime.Now.ToFileTime();
        logger.Info("[" + serviceName + "]" + "Restarting network: \nCurrent ips:" +
                    JsonConvert.SerializeObject(tempIps, Formatting.Indented) + "\n Old ips:" +
                    JsonConvert.SerializeObject(localIps, Formatting.Indented));

        //如果想要停止原来的运行，则需要等待原来循环完全结束退出。设置退出标志为true让原来的while循环退出。
        ExitFlag = true;
        localIps.Clear();
        udpClient.Client.Close();


        //等待2倍原来的间歇时间，确保所有循环都已经退出了，然后再重新启动。
        Thread.Sleep(KeepAliveInterval * 2);
        ExitFlag = false;
        StartListen(bindAddress);
    }

    private void initLocalIps()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                localIps.Add(ip.ToString());
    }
}