using System.Net;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.Worker;
using GenericProtocol;
using GenericProtocol.Implementation;
using NLog;
using RecognizerLib.Lib.Worker;

namespace CommonLib.Lib.Network;

public class TCPChannelService
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static ProtoServer<List<CameraPayLoad>> _server;

    private TCPChannelService()
    {
    }

    private static TCPChannelService me = new TCPChannelService();
    public static TCPChannelService getInstance()
    {
        return me;
    }

    public void initServer()
    {
       

        _server = new ProtoServer<List<CameraPayLoad>>(getBindInfo().ipAddress, getBindInfo().tcpPort); 
        _server.Start();
        logger.Info($"TCP Server started! with {getBindInfo().ipAddress.ToString()}:{getBindInfo().tcpPort}");
        _server.ClientConnected += ClientConnected;
        _server.ClientDisconnected += ClientDisconnected;
        _server.ReceivedMessage += ServerMessageReceived;
       
    }

    private void ClientDisconnected(IPEndPoint endpoint)
    {
        logger.Info($"[Disconnected]Remote TCP client disconnected:{endpoint.Address}:{endpoint.Port}");
    }

    private void ClientConnected(IPEndPoint endpoint)
    {
        logger.Info($"Remote TCP client connected:{endpoint.Address}:{endpoint.Port}");
    }

    private void ServerMessageReceived(IPEndPoint senderendpoint, List<CameraPayLoad> message)
    {
        logger.Debug($"CameraPayload recieved: {message.Last().TriggerId}");
        RawDataBridge.processCameraDataFromWeb(message);
    }

    private Dictionary<RpcEndPoint, ProtoClient<List<CameraPayLoad>>> tcpClients =
        new Dictionary<RpcEndPoint, ProtoClient<List<CameraPayLoad>>>();
    public ProtoClient<List<CameraPayLoad>> initClient(RpcEndPoint rpcEndPoint)
    {
        
        IPAddress ServerIp = IPAddress.Parse(rpcEndPoint.Address);
        ProtoClient<List<CameraPayLoad>> _client = new ProtoClient<List<CameraPayLoad>>(ServerIp, rpcEndPoint.TcpPort) { AutoReconnect = true };
        _client.SendBufferSize = 2048;
        _client.Connect();//.GetAwaiter().GetResult();
        _client.ConnectionLost += ClientConnectionLost;
        logger.Info($"Local TCP client initialized:{rpcEndPoint.Address}:{rpcEndPoint.TcpPort}");
        if (!tcpClients.ContainsKey(rpcEndPoint))
        {
            tcpClients.Add(rpcEndPoint,_client);
        }
        return _client;
    }

    private void ClientConnectionLost(IPEndPoint endpoint)
    {
        logger.Info($"TCP Local Client connection lsot {endpoint.Address} {endpoint.Port}");
    }

    public async Task onSendCameraData(RpcEndPoint rpcEndPoint,List<CameraPayLoad> message)
    {
        if (!tcpClients.ContainsKey(rpcEndPoint))
        {
            tcpClients.Add(rpcEndPoint,initClient(rpcEndPoint));
        }
        var _client = tcpClients[rpcEndPoint];
        
        _client.Send(message).GetAwaiter().GetResult();
        logger.Debug($"Data sent to remote endpint count {message.Count} last triggerID {message.Last().TriggerId}");
    }

    public void closeClientConnection(RpcEndPoint rpcEndPoint)
    {
        var _client = tcpClients[rpcEndPoint];
        _client.Dispose();
    }

    private TCPInfo getBindInfo()
    {
        var serverPort = ConfigUtil.getModuleConfig().NetworkConfig.TcpPort;
        var bindAddressString = ConfigUtil.getModuleConfig().NetworkConfig.TcpBindIp;
        IPAddress ipaddress = null;
        if (!(IPAddress.TryParse(bindAddressString, out ipaddress)))
        {
            if (bindAddressString == null || bindAddressString == "*")
            {
                ipaddress=IPAddress.Any;
            }
        }
        return new TCPInfo{
                ipAddress=ipaddress,
                tcpPort=serverPort
             }
        ;

    }
}

public class TCPInfo
{
    public IPAddress ipAddress;
    public int tcpPort;
}