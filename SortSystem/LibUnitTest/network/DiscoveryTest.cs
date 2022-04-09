using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonLib.Lib.Network;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace LibUnitTest.network;

public class DiscoveryTest
{
    private readonly UDPDiscoveryService discoverService = new UDPDiscoveryService(13298, 13299, "unitTestInstance");

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
        discoverService.KeepAliveInterval = 10;
        discoverService.UnitTestFlag = true;
        discoverService.StartListen();
        DiscoverFoundEventArgs eventArgsFromInside = null;

        discoverService.EndPointDiscoverFound += (object sender, DiscoverFoundEventArgs e) =>
        {
            eventArgsFromInside = e;
        };

        Thread.Sleep(1000);
        Assert.NotNull(eventArgsFromInside);
        Assert.AreEqual(eventArgsFromInside.rpcPort,13298);
        
    }
}