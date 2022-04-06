
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace network;

public class NetworkScratch
{
    int PORT = 9876;
    UdpClient udpClient = new UdpClient();
    [SetUp]
    public  void main () {
       
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

        var from = new IPEndPoint(0, 0);
        Task.Run(() =>
        {
            while (true)
            {
                var recvBuffer = udpClient.Receive(ref from);
                Console.WriteLine("Recive"+Encoding.UTF8.GetString(recvBuffer));
            }
        });
    }

    [Test]
    public void send1stCmd()
    {
        var count = 0;
        
            while (count++<5)
            {
                Thread.Sleep(1000);
                var data = Encoding.UTF8.GetBytes("{'port':"+count+"}");
                udpClient.Send(data, data.Length, "255.255.255.255", PORT);
            }
        
       
    }
    

}