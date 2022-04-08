using System.Linq;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.ConfigVO.Emission;
using CommonLib.Lib.Util;
using NLog;

namespace LibUnitTest.Parser;
using NUnit.Framework;
public class ConfigLoaderTest
{
    [SetUp]
    public void setup()
    {
        LogManager.LoadConfiguration("config/logger.config");
        ConfigUtil.setConfigFolder( "../../../config");;
    }

    [Test]
    public void LoadModuleConfigTest()
    { 
        
        
       ModuleConfig cfg =  ConfigUtil.loadModuleConfig();
       
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
        
        MachineState[] cfg =  ConfigUtil.loadMachineState();
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
    
    [Test]
    public void LoadEmitters()
    {
       
        Emitter[] cfg =  ConfigUtil.LoadEmitters();
        Assert.AreEqual(cfg.Length,8);
        Assert.AreEqual(cfg.First().Delay.Length,4);
        Assert.AreEqual(cfg.Last().Duration.Length,4);
        
        Assert.AreEqual(cfg.First().Delay.First().Length,6);
        Assert.AreEqual(cfg.Last().Duration.First().Length,6);
        
        Assert.AreEqual(cfg.First().Offset.Length,4);
        Assert.AreEqual(cfg.Last().Offset.Length,4);

    }
}