// See https://aka.ms/new-console-template for more information

using System;
using System.Net;
using System.Net.Sockets;
using NetworkLib.Discovery;


 

var discoverService = new UDPDiscoveryService(DiscoverMSG.RPC_PORT);
discoverService.StartListen();
Console.WriteLine("Hello, World!");