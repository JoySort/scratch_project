using System.Diagnostics.Metrics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using NLog;

namespace NetworkLib.Discovery;

public class UDPDiscoveryService
{
    //LogManager.LoadConfiguration("config/logger.config"); 
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private UdpClient udpClient;
    private string serviceName = "default name";
    private DiscoverMSG localDiscoverMsg;
    private Dictionary<string, int> firstMsgID = new Dictionary<string, int>();
    private Dictionary<string, int> lastMsgID = new Dictionary<string, int>();
    private Dictionary<string, Dictionary<int, int>> lastDiff = new Dictionary<string, Dictionary<int, int>>();
    private List<string> localIps = new List<string>();
    private int counter = 0;

    private Dictionary<string, DiscoverMSG> newTargetInfo =
        new Dictionary<string, DiscoverMSG>();
    public bool UnitTestFlag { get; set; } = false;
    public int ListenPort { get; set; }
    
    public int KeepAliveInterval { get; set; } = 1000 * 3;
    public int Counter => counter;
    private Dictionary<string, long> lastAnouncementSent = new Dictionary<string, long>();
    private Dictionary<string, int> MsgCounter { get; } = new Dictionary<string, int>();

    private Dictionary<string, Dictionary<int, int>> LastDiff => lastDiff;
    public bool ExitFlag { get; set; } = false;
    public event EventHandler<DiscoverFoundEventArgs> EndPointDiscoverFound;

    protected virtual void OnEndPointDiscoverFound(DiscoverFoundEventArgs e)
    {
        var handler = EndPointDiscoverFound;
        handler?.Invoke(this,e);
    }

    public UDPDiscoveryService(int rpc_port, int listenPort,string serviceName)
    {

        this.serviceName = serviceName;
        this.ListenPort = listenPort;
        localDiscoverMsg = new DiscoverMSG(rpc_port, listenPort, DiscoverMSG.MSG_TYPE_BRD, 0);
    }

    private IPAddress bindAddress;
    public void StartListen()
    {   
        StartListen(IPAddress.Any);
    }

    public void StartListen(IPAddress bindAddr)
    {
        initLocalIps();
        bindAddress = bindAddr;
        udpClient = new UdpClient();
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
                
                if (!UnitTestFlag && localIps.Contains(fromIP)) continue; // ignore local msgs;

                if (peerDiscoverMsg.Type == DiscoverMSG.MSG_TYPE_BRD)
                {

                    if (!newTargetInfo.ContainsKey(targetKey)) newTargetInfo[targetKey] = peerDiscoverMsg;
                    
                    
                    sendResponds(fromIP, fromPort, peerDiscoverMsg.Count);
                    lastMsgID[targetKey] = peerDiscoverMsg.Count;
                    logger.Debug("["+serviceName+"]"+"BROADCAST From : " + fromIP + ":" + from.Port.ToString() + " msg count: " +
                                 MsgCounter[targetKey] + " msg diff:" + (peerDiscoverMsg.Count - MsgCounter[targetKey]) +
                                 "  content: " + peerDiscoverMsg.ToString());
                }
                else
                {
                    if (lastAnouncementSent.ContainsKey(targetKey))
                    {
                        var timeDiff = DateTime.Now.ToFileTime() - lastAnouncementSent[targetKey];
                        if (timeDiff/100 > 5000*10)
                        {
                            SendAnnouncement();
                        }
                        else
                        {
                            logger.Debug("Last annoucement sent 1s ago, skip this anoucement last:"+lastAnouncementSent[targetKey]);
                        }
                    }
                    else
                    {
                        lastAnouncementSent[targetKey] = DateTime.Now.ToFileTime();
                        SendAnnouncement();
                    }
                    
                    logger.Debug("["+serviceName+"]"+"ACK From : " + fromIP + ":" + from.Port.ToString() + "  content: " +
                                 peerDiscoverMsg.ToString());
                }
            }
        });

        Task.Run(() =>
        {
            SendAnnouncement();
            while (!ExitFlag)
            {
                if (counter % 10 == 0)
                {
                    printStats();
                    checkForNewDiscovery();
                    checkForNetWorkAdaptorChange();
                }
                
                Thread.Sleep(KeepAliveInterval);
            }
        });
    }

    private void sendResponds(string fromIP, int fromPort, int msgID)
    {
        var targetKey = fromIP + ":" + fromPort;
        if (!MsgCounter.ContainsKey(targetKey))
        {
            MsgCounter[targetKey] = 0;
        }
        else
        {
            MsgCounter[targetKey]++;
        }

        if (!firstMsgID.ContainsKey(targetKey))
        {
            firstMsgID[targetKey] = msgID;
            
            var discoverEventArgs = new DiscoverFoundEventArgs
            {
                ipAddr = fromIP,
                rpcPort = newTargetInfo[targetKey].RpcPort
            };
            OnEndPointDiscoverFound(discoverEventArgs);
        }


        var respondDiscoverMsg = new DiscoverMSG(localDiscoverMsg.RpcPort, ListenPort, DiscoverMSG.MSG_TYPE_ACK,
            MsgCounter[targetKey]);
        var msg = JsonConvert.SerializeObject(respondDiscoverMsg);
        var data = Encoding.UTF8.GetBytes(msg);


        udpClient.Send(data, data.Length, fromIP, fromPort);
    }


    public void SendAnnouncement()
    {
        counter++;
        localDiscoverMsg.Type = DiscoverMSG.MSG_TYPE_BRD;
        localDiscoverMsg.Count = counter;
        var msg = JsonConvert.SerializeObject(localDiscoverMsg);
        var data = Encoding.UTF8.GetBytes(msg);
        udpClient.Send(data, data.Length, DiscoverMSG.BROADCAST_ADDR, ListenPort);
    }

    private void checkForNewDiscovery()
    {
        foreach ((var key, var value)in firstMsgID)
        {
            var diff_value = lastMsgID[key] - value - MsgCounter[key];
            var diff_key = lastMsgID[key];


            if (!lastDiff.ContainsKey(key))
            {
                var diff_dic = new Dictionary<int, int>();
                diff_dic.Add(diff_key, diff_value);
                lastDiff.Add(key, diff_dic);
            }
            else
            {
                if (lastDiff[key].Last().Value != diff_value)
                {
                    if (lastDiff[key].ContainsKey(diff_key))
                    {
                        lastDiff[key][diff_key] = diff_value;
                    }
                    else
                    {
                        lastDiff[key].Add(diff_key, diff_value);
                    }
                    var discoverEventArgs = new DiscoverFoundEventArgs
                    {
                        ipAddr = key.Split(":")[0],
                        rpcPort = newTargetInfo[key].RpcPort
                    };
                    OnEndPointDiscoverFound(discoverEventArgs);
                }
            }
        }
    }

    private void checkForNetWorkAdaptorChange()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var tmpLocalIps = new List<string>();
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                tmpLocalIps.Add(ip.ToString());
            }
        }

       
        if (localIps.Count>0&&localIps.Count != tmpLocalIps.Count)
        {
            restartDiscoveryOnNetworkChange(tmpLocalIps);
            //throw new Exception(message);
        }
        else
        {
            localIps.Sort();
            tmpLocalIps.Sort();
            if (localIps.Count > 0 && !localIps.SequenceEqual(tmpLocalIps))
            {
                restartDiscoveryOnNetworkChange(tmpLocalIps);
            }
        }
    }

    private int restartNetworkAdaptorCount = 0;
    private long lastRestartTimestamp = 0;
    private void restartDiscoveryOnNetworkChange(List<string> tempIps)
    {
        var timeDiff = DateTime.Now.ToFileTime() - lastRestartTimestamp;
        if (restartNetworkAdaptorCount > 2 && timeDiff> 10000*KeepAliveInterval*10 ){
            
            throw new Exception("["+serviceName+"]"+"Network adaptor has changed and we retried " + restartNetworkAdaptorCount + "times, Still failing"+"Last restart in "+ timeDiff/1000 + "secs");
        }
        
        restartNetworkAdaptorCount++;
        lastRestartTimestamp=DateTime.Now.ToFileTime();
        logger.Info("["+serviceName+"]"+"Restarting network: \nCurrent ips:" + JsonConvert.SerializeObject(tempIps,Formatting.Indented)+"\n Old ips:"+JsonConvert.SerializeObject(localIps,Formatting.Indented));
        
        //如果想要停止原来的运行，则需要等待原来循环完全结束退出。设置退出标志为true让原来的while循环退出。
        this.ExitFlag = true;
        firstMsgID.Clear();
        lastMsgID.Clear();
        lastDiff.Clear();
        localIps.Clear();
        newTargetInfo.Clear();
        udpClient.Client.Close();
      

        
        //等待2倍原来的间歇时间，确保所有循环都已经退出了，然后再重新启动。
        Thread.Sleep(KeepAliveInterval*2);
        ExitFlag = false;
        StartListen(bindAddress);
    }

    private void initLocalIps()
    {
         var host = Dns.GetHostEntry(Dns.GetHostName());
        
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIps.Add(ip.ToString());
            }
        }
    }

    public void printStats()
    {

       
        
        logger.Info("["+serviceName+"]"+" Message stats: " +
                    "\nresponse sent" + JsonConvert.SerializeObject(MsgCounter,Formatting.Indented) +
                    "\nresponse diff {server:{{lastest_msg_id:difference_of_count_between_lastest_and_first_msgid}}}\n" + JsonConvert.SerializeObject(lastDiff,Formatting.Indented));
    }
}