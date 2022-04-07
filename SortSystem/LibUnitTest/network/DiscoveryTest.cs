using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetworkLib.Discovery;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace LibUnitTest.network;

public class DiscoveryTest
{
    private readonly UDPDiscoveryService discoverService = new UDPDiscoveryService(13298, 13299);

    private Logger? logger;
    private readonly int testCycle = 300 * 1;

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
        while (discoverService.Counter < testCycle) // keep discovery running
        {
            Thread.Sleep(100);
            if (discoverService.Counter > testCycle - 1)
            {
                discoverService.ExitFlag = true;


                logger?.Info(JsonConvert.SerializeObject(discoverService.LastDiff));
                Assert.AreEqual(discoverService.LastDiff.Last().Value.Last().Value, 0);
                Assert.NotNull(eventArgsFromInside);
            }
        }
    }
}