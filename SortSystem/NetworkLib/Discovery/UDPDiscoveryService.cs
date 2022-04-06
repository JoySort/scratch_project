using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace NetworkLib.Discovery;

public class UDPDiscoveryService
{

    private UdpClient udpClient;
    private DiscoverMSG localDiscoverMsg;

    private int keepAliveInterval = 1000* 3;//seconds
    private bool exitFlag = false;
    private int counter = 0;
    private Dictionary<string, int> msgCounter= new Dictionary<string,int>();

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
                    sendResponds(fromIP);
                    Console.WriteLine("Recive from ip : "+fromIP+":"+from.Port.ToString() + " msg count: "+ msgCounter[fromIP]+"  content: "+peerDiscoverMsg.ToString());
                }
                
                
                
              

              
               
            }
        });
        
        
        
        while (!exitFlag)
        {
            SendAnnouncement();
            Thread.Sleep(keepAliveInterval);
        }
    }

    private void sendResponds(string fromIP)
    {
        if (!msgCounter.ContainsKey(fromIP))
        {
            msgCounter[fromIP] = 0;
        }
        else
        {
            msgCounter[fromIP]++;
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
            Console.WriteLine("Send  msg count: "+ counter+"  content: "+localDiscoverMsg.ToString());
            if(counter%10 == 0)
                Console.WriteLine("Message kept alive from ip: \n"+JsonConvert.SerializeObject(msgCounter));

    }
    
}