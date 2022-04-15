using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonLib.Lib.Network;
using CommonLib.Lib.Util;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace LibUnitTest.network;

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
        
        
        
        UDPDiscoveryService discoverService1 = new UDPDiscoveryService(ConfigUtil.getModuleConfig().NetworkConfig.RpcPort, ConfigUtil.getModuleConfig().NetworkConfig.DiscoveryPorts.First(), "unitTestInstance");
        discoverService1.KeepAliveInterval = 10;
        discoverService1.UnitTestFlag = true;
        discoverService1.StartListen();
        DiscoverFoundEventArgs eventArgsFromInside1 = null;
        discoverService1.EndPointDiscoverFound += (object sender, DiscoverFoundEventArgs e) =>
        {
            eventArgsFromInside1 = e;
            blocking1 = false;
        };
        
        
        UDPDiscoveryService discoverService = new UDPDiscoveryService(ConfigUtil.getModuleConfig().NetworkConfig.RpcPort, ConfigUtil.getModuleConfig().NetworkConfig.DiscoveryPorts.First(), "unitTestInstance");
        discoverService.KeepAliveInterval = 10;
        discoverService.UnitTestFlag = true;
        discoverService.StartListen();
        DiscoverFoundEventArgs eventArgsFromInside = null;

        discoverService.EndPointDiscoverFound += (object sender, DiscoverFoundEventArgs e) =>
        {
            eventArgsFromInside = e;
            blocking2 = false;
        };

        var startTime = DateTime.Now.Millisecond;
        while (blocking1 || blocking2)
        {
            Thread.Sleep(10);
        }
        logger.Info("Blocking took {}",DateTime.Now.Millisecond-startTime);
        
        Assert.NotNull(eventArgsFromInside);
        Assert.NotNull(eventArgsFromInside1);
        
        Assert.AreEqual(eventArgsFromInside.rpcPort,ConfigUtil.getModuleConfig().NetworkConfig.RpcPort);
        
    }
}