// See https://aka.ms/new-console-template for more information


using CommonLib.Lib.Network;
using NLog;

LogManager.LoadConfiguration("config/logger.config"); 
Logger logger = LogManager.GetCurrentClassLogger();

logger.Trace("Trace Message");
logger.Debug("Debug Message");
logger.Info("Info Message");
logger.Error("Error Message");
logger.Fatal("Fatal Message");

var rpc_port = 13456;
var udp_port = 13457;
var keepAlive = 3000;
if (args.Length > 0)
{
    rpc_port = int.Parse(args[0]);
    udp_port = int.Parse(args[1]);
    if (args.Length > 2)
    {
        keepAlive = int.Parse(args[2]);
    }
}

var uppDiscoverService = new UDPDiscoveryService(rpc_port,udp_port,"upperDiscovery");
uppDiscoverService.KeepAliveInterval = keepAlive;
uppDiscoverService.EndPointDiscoverFound += (object sender, DiscoverFoundEventArgs e) =>
{
    logger.Info("Event catched "+e.ipAddr +" "+ e.rpcPort);
};
uppDiscoverService.StartListen();

var CamDiscoverService = new UDPDiscoveryService(rpc_port+1,udp_port+1,"CamDiscovery");
CamDiscoverService.KeepAliveInterval = keepAlive;
CamDiscoverService.EndPointDiscoverFound += (object sender, DiscoverFoundEventArgs e) =>
{
    logger.Info("Event catched "+e.ipAddr +" "+ e.rpcPort);
};
CamDiscoverService.StartListen();


while ( true)// keep discovery running
{
    Thread.Sleep(1000);
}