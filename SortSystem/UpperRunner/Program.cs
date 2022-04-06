// See https://aka.ms/new-console-template for more information

using System;
using NetworkLib.Discovery;

var discoverService = new UDPDiscoveryService(Announcement.ROLE_UPPER);
discoverService.StartListen();
Console.WriteLine("Hello, World!");