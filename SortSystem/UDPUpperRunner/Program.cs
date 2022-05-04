using System;
using System.Diagnostics.Metrics;
using System.Net;
using System.Reflection;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using GenericProtocol.Implementation;

namespace GenericProtocolTest
{
    public static class Program
    {
        private static ProtoServer<CameraPayLoad> _server;
        private static ProtoClient<CameraPayLoad> _client;
        private static readonly IPAddress ServerIp = IPAddress.Loopback;
        private static bool TestServer { get; set; } = false;
        private static bool TestClient { get; set; } = false;

        private static void Main(string[] args)
        {
            //INetworkDiscovery discovery = new NetworkDiscovery();
            //discovery.Host(IPAddress.Any);
            //discovery.Discover();
            if (args.Length > 0)
            {
                TestServer = true;
            }
            else
            {
                TestClient = true;
            }

            if (TestServer)
                StartServer();
            if (TestClient)
                StartClient();

            Console.WriteLine("\n");

            while (true)
            {
                Thread.Sleep(1000);
            }
        }


        private static void StartClient()
        {
            _client = new ProtoClient<CameraPayLoad>(ServerIp, 55024) { AutoReconnect = true };
            _client.ReceivedMessage += ClientMessageReceived;
            _client.ConnectionLost += Client_ConnectionLost;

            Console.WriteLine("Connecting");
            _client.Connect();
            Console.WriteLine("Connected!");
           // _client.Send("Hello Server!").GetAwaiter().GetResult();
        }

        private static void SendToServer(CameraPayLoad message)
        {
            _client?.Send(message);
        }

        private static void SendToClients(CameraPayLoad message)
        {
            _server?.Broadcast(message);
        }

        private static void Client_ConnectionLost(IPEndPoint endPoint)
        {
            Console.WriteLine($"Connection lost! {endPoint.Address}");
        }

        private static void StartServer()
        {
            _server = new ProtoServer<CameraPayLoad>(IPAddress.Any, 55024);
            Console.WriteLine("Starting Server...");
            _server.Start();
            Console.WriteLine("Server started!");
            _server.ClientConnected += ClientConnected;
            _server.ReceivedMessage += ServerMessageReceived;
        }

        private static async void ServerMessageReceived(IPEndPoint sender, CameraPayLoad message)
        {
            message.TriggerId = counter++;
            Console.WriteLine($"{sender}: {message.TriggerId}");
            
            await _server.Send(message, sender);
        }

        private static int counter = 0;
        private static void ClientMessageReceived(IPEndPoint sender, CameraPayLoad message)
        {
            message.TriggerId = counter++;
            Console.WriteLine($"{sender}: {message}");
            Thread.Sleep(1000);
            SendToServer(message);
        }

        private static async void ClientConnected(IPEndPoint address)
        {
            var data = new CameraPayLoad();
            ConfigUtil.setConfigFolder("modules/");
            data.CamConfig = new CameraConfig("a", "b", "c", "d", new int[] {0, 1}, new int[] {0, 1},
                CameraPosition.middle, true, "c", 100, 100, 1, CameraType.Other);
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                "assets/" + 1 + ".bmp");
            byte[] picture = File.ReadAllBytes(path);
            Console.WriteLine($"picture length {picture.Length}");
            data.PictureData = picture;
            data.TriggerId = 1;//4681014
           // Thread.Sleep(1000);
           Console.WriteLine("client connected!");
           await _server.Send(data, address);
        }
    }
}