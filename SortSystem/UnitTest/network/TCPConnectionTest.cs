using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Network;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.Worker;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace UnitTest.network;

public class TCPConnectionTest
{
    private Logger? logger;
    [SetUp]
    public  void  setup()
    {
        LogManager.LoadConfiguration("config/logger.config");
        logger = LogManager.GetCurrentClassLogger();    

        
    }

    private bool blocking = true;
    [Test]
    public void test1()
    {
        
   
            var data = new CameraPayLoad();
            data.CamConfig = ConfigUtil.getModuleConfig().CameraConfigs[0];
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                "assets/" + 1 + ".bmp");
            byte[] picture = File.ReadAllBytes(path);
            logger.Info($"picture length {picture.Length}");
            //data.PictureData = picture;
            data.TriggerId = 1;//4681014
            var tmpList = new List<CameraPayLoad>();
            tmpList.Add(data);
            
            Console.WriteLine(JsonConvert.SerializeObject(tmpList));

    }
}