using System.Linq;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.ConfigVO.Emission;
using CommonLib.Lib.LowerMachine;
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


        ModuleConfig cfg = ConfigUtil.getModuleConfig();

        Assert.AreEqual(cfg.Module, JoyModule.Upper);
        Assert.AreEqual(cfg.LowerConfig.Length, 2);
        Assert.AreEqual(cfg.LowerConfig.First().HardwarePort, "/dev/com1");
        Assert.True(cfg.LowerConfig.First().Columns.SequenceEqual(new[]{0, 11 }));
        Assert.AreEqual(cfg.LowerConfig.Last().HardwarePort, "/dev/com2");
        Assert.True(cfg.LowerConfig.Last().Columns.SequenceEqual(new[]{12, 23 }));
        Assert.AreEqual(cfg.NetworkConfig.RpcPort,5113);
       Assert.True(cfg.NetworkConfig.DiscoveryPorts.SequenceEqual(new [] {13567}));
       Assert.AreEqual(cfg.NetworkConfig.RpcBindIp,"*");
       Assert.AreEqual(cfg.NetworkConfig.UdpBindIp,"*");
       Assert.AreEqual(cfg.SortConfig.OutletPriority,OutletPriority.ASC);
       Assert.AreEqual(cfg.ConsolidatePolicy.ConsolidateArg.First(),0);
       Assert.AreEqual(cfg.ConsolidatePolicy.ConsolidationOperation.Last(),ConsolidateOperation.avg);
       Assert.AreEqual(cfg.ConsolidatePolicy.CriteriaCode.Last(),"fm");
       Assert.AreEqual(cfg.CriteriaMapping["fm"].Key,"fm");
       
    }
    
    [Test]
    public void LoadStateConfigTest()
    {
        
        MachineState[] cfg =  ConfigUtil.getMachineState();
        foreach (var state in cfg)
        {
            if (state.StateName == ProjectState.start)
            {
                Assert.AreEqual(state.State.Servos.Length, 1);
                Assert.AreEqual(state.State.Servos.First().Name, "main_motor");
                Assert.AreEqual(state.State.Servos.First().StartTimeSpan, 30000);
                
                Assert.AreEqual(state.State.Triggers.Length, 1);
                Assert.AreEqual(state.State.Triggers.First().Interval, 300);
                Assert.AreEqual(state.State.Triggers.First().Mode, TriggerMode.PhotoelectricSwitch);
                Assert.AreEqual(state.State.StepMotoers.Length, 1);
                Assert.AreEqual(state.IsDefault, false);
            }
        }
        
    }
    
    [Test]
    public void LoadEmitters()
    {
       
        Emitter[] cfg =  ConfigUtil.getEmitters();
        Assert.AreEqual(cfg.Length,8);
        Assert.AreEqual(cfg.First().Delay.Length,4);
        Assert.AreEqual(cfg.Last().Duration.Length,4);
        
        Assert.AreEqual(cfg.First().Delay.First().Length,6);
        Assert.AreEqual(cfg.Last().Duration.First().Length,6);
        
        Assert.AreEqual(cfg.First().Offset.Length,4);
        Assert.AreEqual(cfg.Last().Offset.Length,4);

    }
}