using System.Diagnostics.Metrics;
using System.Net;
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
    private DiscoverMSG localDiscoverMsg;

    private int keepAliveInterval = 1000* 3;//seconds
    private bool exitFlag = false;
    private int counter = 0;
    private Dictionary<string, int> msgCounter= new Dictionary<string,int>();
    private Dictionary<string, int> firstMsgID= new Dictionary<string,int>();
    private Dictionary<string, int> lastMsgID= new Dictionary<string,int>();
    private  Dictionary<string,Dictionary<int,int>> lastDiff  = new Dictionary<string,Dictionary<int,int>>();
    private List<string> localIps = new List<string>();
    
    public bool ExitFlag
    {
        get => exitFlag;
        set => exitFlag = value;
    }

    public UDPDiscoveryService(int rpc_port)
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIps.Add( ip.ToString());
            }
        }
        localDiscoverMsg = new DiscoverMSG(rpc_port,DiscoverMSG.MSG_TYPE_BRD,0);
    }

    public void StartListen()
    {
        
        udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoverMSG.DISCOVER_PORT ));
        
        var from = new IPEndPoint(0, 0);
        Task.Run(() =>
        {
            while (!exitFlag)
            {
                var recvBuffer = udpClient.Receive(ref from);
                
                var msg = Encoding.UTF8.GetString(recvBuffer);
                var peerDiscoverMsg = JsonConvert.DeserializeObject<DiscoverMSG>(msg);
                
                var fromIP = from.Address.ToString();
                if(localIps.Contains(fromIP)) continue; // ignore local msgs;
                
                if (peerDiscoverMsg.Type == DiscoverMSG.MSG_TYPE_BRD) {
                    sendResponds(fromIP,peerDiscoverMsg.Count);
                    lastMsgID[fromIP] = peerDiscoverMsg.Count;
                    logger.Debug("Recive from ip : "+fromIP+":"+from.Port.ToString() + " msg count: "+msgCounter[fromIP]+" msg diff:"+ (peerDiscoverMsg.Count- msgCounter[fromIP])+"  content: "+peerDiscoverMsg.ToString());
                }
                else
                {
                    logger.Debug("Recive from ip : "+fromIP+":"+from.Port.ToString() + "  content: "+peerDiscoverMsg.ToString());
                }

            }
        });
        
        
        
        while (!exitFlag)
        {   if(counter%10 == 0)printStats();
            SendAnnouncement();
            Thread.Sleep(keepAliveInterval);
        }
    }

    private void sendResponds(string fromIP,int msgID)
    {
        if (!msgCounter.ContainsKey(fromIP))
        {
            msgCounter[fromIP] = 0;
            
        }
        else
        {
            msgCounter[fromIP]++;
        }
        if (!firstMsgID.ContainsKey(fromIP))
        {
            firstMsgID[fromIP] = msgID;
        }
        
        
        var respondDiscoverMsg = new DiscoverMSG(localDiscoverMsg.RpcPort,DiscoverMSG.MSG_TYPE_ACK,msgCounter[fromIP]);
        var msg = JsonConvert.SerializeObject(respondDiscoverMsg);
        var data = Encoding.UTF8.GetBytes(msg);
        

        udpClient.Send(data, data.Length, DiscoverMSG.BROADCAST_ADDR, DiscoverMSG.DISCOVER_PORT);
    }
    

    public void SendAnnouncement()
    {
            
            counter++;
            localDiscoverMsg.Type = DiscoverMSG.MSG_TYPE_BRD;
            localDiscoverMsg.Count = counter;
            var msg = JsonConvert.SerializeObject(localDiscoverMsg);
            var data = Encoding.UTF8.GetBytes(msg);
            udpClient.Send(data, data.Length, DiscoverMSG.BROADCAST_ADDR, DiscoverMSG.DISCOVER_PORT);
             

    }

    public void printStats()
    {
        var ackCounter  = new Dictionary<string,int>();
        foreach ((var key, var value) in firstMsgID)
        {
            ackCounter.Add(key,value-lastMsgID[key]);
        }

        
            foreach ((var key, var value)in firstMsgID)
            {
                var diff_value = lastMsgID[key] - value - msgCounter[key];
                var diff_key = lastMsgID[key];
                
                
                
                if (!lastDiff.ContainsKey(key))
                {
                    var diff_dic = new Dictionary<int, int>();
                    diff_dic.Add(diff_key,diff_value);
                    lastDiff.Add(key, diff_dic);
                }
                else
                {
                    if(lastDiff[key].Last().Value!=diff_value)
                    {
                        if (lastDiff[key].ContainsKey(diff_key)) lastDiff[key][diff_key]=diff_value;
                        lastDiff[key].Add(diff_key,diff_value);
                        
                    }
                }
            }
        


        
            logger.Info(" Message stats: " + 
                        "\nresponse sent"+JsonConvert.SerializeObject(msgCounter) + 
                        "\n" + JsonConvert.SerializeObject(lastDiff));
        
    }
    

}