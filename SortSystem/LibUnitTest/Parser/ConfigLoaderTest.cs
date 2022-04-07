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
    public void LoadConfigTest()
    {
       Configuration cfg =  ConfigLoader.load();
       Assert.AreEqual(cfg.Name,"lowerRunner1");
    }
}