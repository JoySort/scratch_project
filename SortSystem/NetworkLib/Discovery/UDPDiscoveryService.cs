using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace NetworkLib.Discovery;

public class UDPDiscoveryService
{

    private UdpClient udpClient;
    private Announcement localAnnouncement;
    private string BROADCAST_ADDR = "255.255.255.255";
    private int announcementInterval = 5;//seconds
    
    public UDPDiscoveryService(string role)
    {
        localAnnouncement = new Announcement(role);
    }

    public void StartListen()
    {
        
        udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, Announcement.DISCOVER_PORT ));
        
        var from = new IPEndPoint(0, 0);
        Task.Run(() =>
        {
            while (true)
            {
                var recvBuffer = udpClient.Receive(ref from);
                var fromIP = from.Address.ToString();
                var msg = Encoding.UTF8.GetString(recvBuffer);
                var peerAnnouncement = (Announcement)JsonConvert.DeserializeObject(msg);
                peerAnnouncement.RemoteAddress = fromIP;
                sendResponds(peerAnnouncement);
                // udpClient
                Console.WriteLine("Recive from ip : "+from.Address.ToString()+"  "+from.Port.ToString() + "  content: "+Encoding.UTF8.GetString(recvBuffer));
            }
        });
        var count = 0;
        
        
        while (count++<announcementInterval)
        {
            SendAnnouncement();
            Thread.Sleep(announcementInterval);
        }
    }

    private void sendResponds(Announcement peer_announcement)
    {
        var msg = JsonConvert.SerializeObject(localAnnouncement);
        var data = Encoding.UTF8.GetBytes(msg);
        udpClient.Send(data, data.Length, peer_announcement.RemoteAddress, Announcement.DISCOVER_PORT);
    }
    

    public void SendAnnouncement()
    {
            var msg = JsonConvert.SerializeObject(localAnnouncement);
            var data = Encoding.UTF8.GetBytes(msg);
            udpClient.Send(data, data.Length, "255.255.255.255", Announcement.DISCOVER_PORT);
            
    }
    
}