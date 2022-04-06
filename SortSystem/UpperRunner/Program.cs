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


var discoverService = new UDPDiscoveryService(13456,13455);
discoverService.StartListen();
while ( discoverService.Counter<1000000)// keep discovery running
{
    Thread.Sleep(1000);
}