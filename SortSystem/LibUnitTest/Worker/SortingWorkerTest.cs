using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace LibUnitTest.Worker;

public class SortingWorkerTest
{
    private Logger logger ;
   
    private string? porjectJsonString;
    private ProjectParser? jparser;
    private Project project;
    
  
    private SortingWorker sortingWorker = SortingWorker.getInstance();
    [SetUp]
    public void setup()
    {
        LogManager.LoadConfiguration("config/logger.config");
        logger = LogManager.GetCurrentClassLogger();
        logger.Info("setup test ");

    }

    [Test]
    public void appleConsolidationTest()
    {
        string JsonFilePath = @"./fixtures/project_apple_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString);
        project = parser.getProject();
        
        
        logger.Info("APPLE Test begin");
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        string recResultJsonFixture = @"./fixtures/apple_sorttest_data.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        RecResult[] recResults = JsonConvert.DeserializeObject<RecResult[]>(jsonString);

        bool blocking = true;
        
        sortingWorker.processBulk(new List<RecResult>(recResults));

        sortingWorker.OnResult += ((sender, args) =>
        {
            
            blocking = false;
        });

        sortingWorker.OnResult += appleEventHanlder;
        
        while (blocking)
        {
            Thread.Sleep(100);
        }
        sortingWorker.OnResult -= appleEventHanlder;

        logger.Info("APPLE Test stop");
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
    }
    
    [Test]
    public void pdConsolidationTest()
    {
        logger.Info("PD Test begin");
        
        string JsonFilePath = @"./fixtures/project_pd_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString);
        project = parser.getProject();
        
       
        
        string recResultJsonFixture = @"./fixtures/pd_sorttest_data.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        RecResult[] recResults = JsonConvert.DeserializeObject<RecResult[]>(jsonString);

       

        void outletPriorityChange(OutletPriority priority)
        {
           
            
            bool blocking = true;
            ConfigUtil.getModuleConfig().SortConfig.OutletPriority = priority;


            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            
            ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
            sortingWorker.processBulk(new List<RecResult>(recResults));
            
            string[] ascExpected  = new string[] {"1", "2", "1", "2", "2"};
            string[] descExpected = new string[] {"1", "3", "1", "3", "5"};
            
            void pdEventHanlder(Object sender, SortingResultEventArg args)
            {
                try
                {
                    logger.Info("time consumed (ms):{}",( new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() - currentTimeStamp));
                    var expected = (priority == OutletPriority.ASC) ? ascExpected : descExpected;
                    
                    for (var i = 0; i < args.Results.Count; i++)
                    {
                        List<string> result = args.Results[i].Outlets.Select(outlet => outlet.ChannelNo).ToList();
                
                        
                        Assert.AreEqual(expected[i], String.Join(",",result.OrderBy(x=>x).ToArray()));
                
                    }

                    logger.Info("PD assert finished {}", JsonConvert.SerializeObject(args.Results));
                }
                catch (Exception e)
                {
                    logger.Info(e.Message);
                }
            }
            
            sortingWorker.OnResult += pdEventHanlder;
            sortingWorker.OnResult += ((sender, args) =>
            {
              logger.Info("PD stop blocking");
                blocking = false;
            });
            
            while (blocking)
            {
                Thread.Sleep(1000);
                logger.Info("blocking...");
            }
            sortingWorker.OnResult -= pdEventHanlder;
            
            logger.Info("PD Test stop");
            ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
        }

        outletPriorityChange(OutletPriority.ASC);
        outletPriorityChange(OutletPriority.DESC);

    }
    
    
    public void appleEventHanlder(Object sender, SortingResultEventArg args)
    {
        try
        {
            Assert.AreEqual("1", args.Results.First().Outlets.First().ChannelNo);
            logger.Info("time consumed (ms):{}",
                (DateTime.Now.ToFileTime() - args.Results.First().ProcessTimestamp) / 100);
            logger.Info("APPle assert finished {}", JsonConvert.SerializeObject(args.Results));
        }
        catch (Exception e)
        {
            logger.Error(e.Message);
        }
    }




}