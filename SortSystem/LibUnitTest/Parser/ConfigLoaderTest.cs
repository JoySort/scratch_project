using System.Linq;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Util;

namespace LibUnitTest.Parser;
using NUnit.Framework;
public class ConfigLoaderTest
{
    [SetUp]
    public void setup()
    {
        
    }

    [Test]
    public void LoadModuleConfigTest()
    {
        string JsonFilePath = @"./config/module.json";
       ModuleConfig cfg =  ConfigLoader.loadModuleConfig(JsonFilePath);
       
       Assert.AreEqual(cfg.Module,JoyModule.Lower);
       Assert.AreEqual(cfg.LowerConfig.Length,2);
       Assert.AreEqual(cfg.Network.RpcPort,5113);
       Assert.True(cfg.Network.DiscoveryPorts.SequenceEqual(new [] {13567,13568,13569}));
       Assert.AreEqual(cfg.Network.RpcBindIp,"*");
       Assert.AreEqual(cfg.Network.UdpBindIp,"*");
       
    }
    
    [Test]
    public void LoadStateConfigTest()
    {
        string JsonFilePath = @"./config/state.json";
        MachineState[] cfg =  ConfigLoader.loadMachineState(JsonFilePath);
        foreach (var state in cfg)
        {
            if (state.Name == StateName.start)
            {
                Assert.AreEqual(state.State.Servos.Length, 1);
                Assert.AreEqual(state.State.Triggers.Length, 1);
                Assert.AreEqual(state.State.StepMotoers.Length, 1);
                Assert.AreEqual(state.IsDefault, false);
            }
        }
        
    }
}