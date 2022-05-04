using System.Linq;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.ConfigVO.Emission;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Util;
using Newtonsoft.Json;
using NLog;

namespace UnitTest.Parser;
using NUnit.Framework;
public class ConfigLoaderSubconfigTest
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
        var cfgToken = ConfigUtil.loadSubConfig("module.json");

        var cfgActual = JsonConvert.DeserializeObject<ModuleConfig>(cfgToken.ToString());
        
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
       Assert.AreEqual(cfg.ElasticSearchConfig.url,"http://es.lan:9200");
       
    }
    
   
}