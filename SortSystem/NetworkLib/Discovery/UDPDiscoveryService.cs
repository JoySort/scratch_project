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
    private long counter = 0;
    private Dictionary<string, int> msgCounter= new Dictionary<string,int>();

    public bool ExitFlag
    {
        get => exitFlag;
        set => exitFlag = value;
    }

    public UDPDiscoveryService(int rpc_port)
    {
        localDiscoverMsg = new DiscoverMSG(rpc_port);
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
                
                
                
                Console.WriteLine("Recive from ip : "+fromIP+"  "+from.Port.ToString() + "  content: "+peerDiscoverMsg);
                sendResponds(fromIP);
            }
        });
        var count = 0;
        
        
        while (!exitFlag)
        {
            SendAnnouncement();
            Thread.Sleep(keepAliveInterval);
        }
    }

    private void sendResponds(string fromIP)
    {
        var msg = JsonConvert.SerializeObject(localDiscoverMsg);
        var data = Encoding.UTF8.GetBytes(msg);
        if (msgCounter[fromIP] == null)
        {
            msgCounter[fromIP] = 0;
        }
        else
        {
            msgCounter[fromIP]++;
        }

        udpClient.Send(data, data.Length, DiscoverMSG.BROADCAST_ADDR, DiscoverMSG.DISCOVER_PORT);
    }
    

    public void SendAnnouncement()
    {
            counter++;
            var msg = JsonConvert.SerializeObject(localDiscoverMsg);
            var data = Encoding.UTF8.GetBytes(msg);
            udpClient.Send(data, data.Length, DiscoverMSG.BROADCAST_ADDR, DiscoverMSG.DISCOVER_PORT);
            if(counter%10 == 0)
                Console.WriteLine("Message kept alive from ip: \n"+JsonConvert.SerializeObject(msgCounter));

    }
    
}