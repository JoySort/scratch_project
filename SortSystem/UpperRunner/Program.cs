// See https://aka.ms/new-console-template for more information





using NetworkLib.Discovery;
using NLog;

LogManager.LoadConfiguration("config/logger.config"); 
Logger logger = LogManager.GetCurrentClassLogger();

logger.Trace("Trace Message");
logger.Debug("Debug Message");
logger.Info("Info Message");
logger.Error("Error Message");
logger.Fatal("Fatal Message");
var discoverService = new UDPDiscoveryService(DiscoverMSG.RPC_PORT);
discoverService.StartListen();
Console.WriteLine("Hello, World!");