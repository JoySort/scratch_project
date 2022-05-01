using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonLib.Lib.Network;
using CommonLib.Lib.Util;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace UnitTest.network;

public class DiscoveryTest
{


    private Logger? logger;
    private readonly int testCycle = 3 * 1;

    public DiscoveryTest()
    {
        LogManager.LoadConfiguration("config/logger.config");
        logger = LogManager.GetCurrentClassLogger();
    }

    [SetUp]
    public void Main()
    {
        //discoverService.StartListen();
        
    }

    [Test]
    public void Send1StCmd()
    {
        var blocking1 = true;
        var blocking2 = true;
        
        
        
        UDPDiscoveryService discoverService1 = new UDPDiscoveryService(ConfigUtil.getModuleConfig().NetworkConfig.RpcPort, ConfigUtil.getModuleConfig().NetworkConfig.DiscoveryPorts.First(), "unitTestInstance1");
        discoverService1.Uuid = Guid.NewGuid().ToString();
        //discoverService1.UnitTestFlag = true;
        discoverService1.StartListen();
        EndPointChangedArgs eventArgsFromInside1 = null;
        discoverService1.EndPointDiscoverFound += (object sender, EndPointChangedArgs e) =>
        {
            eventArgsFromInside1 = e;
            blocking1 = false;
        };
        
        
        UDPDiscoveryService discoverService = new UDPDiscoveryService(ConfigUtil.getModuleConfig().NetworkConfig.RpcPort, ConfigUtil.getModuleConfig().NetworkConfig.DiscoveryPorts.First(), "unitTestInstance2");
        discoverService.KeepAliveInterval = 10;
        //discoverService.UnitTestFlag = true;
        discoverService.StartListen();
        EndPointChangedArgs eventArgsFromInside = null;

        discoverService.EndPointDiscoverFound += (object sender, EndPointChangedArgs e) =>
        {
            eventArgsFromInside = e;
            blocking2 = false;
        };

        var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        while (blocking1 || blocking2)
        {
            Thread.Sleep(10);
        }
        logger.Info("Blocking took {}",DateTimeOffset.Now.ToUnixTimeMilliseconds()-startTime);
        
        Assert.NotNull(eventArgsFromInside);
        Assert.NotNull(eventArgsFromInside1);
        
        //Assert.AreEqual(eventArgsFromInside.rpcPort,ConfigUtil.getModuleConfig().NetworkConfig.RpcPort);
        
    }
}