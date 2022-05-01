using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Network;
using CommonLib.Lib.Worker;
using NLog;
using NUnit.Framework;

namespace UnitTest.network;

public class HTTPClientTest
{
    private Logger? logger;
    [SetUp]
    public  void  setup()
    {
        LogManager.LoadConfiguration("config/logger.config");
        logger = LogManager.GetCurrentClassLogger();    

        
    }

    private bool blocking = true;
    [Test]
    public void test1()
    {
        
        ModuleCommunicationWorker.getInstance().OnDiscovery += ((Object sender,RpcEndPoint args) =>
        {
            blocking = false;
        });
        

        while (blocking)
        {
            
            Thread.Sleep(500);
            
        }
        ModuleConfig value = ModuleCommunicationWorker.getInstance().RpcEndPoints.First().Value.First().Value.ModuleConfig;
        Assert.IsNotNull(value);
        Assert.False(blocking);
    }
}