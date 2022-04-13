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

public class LBWorkerTest
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

        LBWorker.getInstance();

    }

    [Test]
    public void LBTest()
    {
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
                   
                    
                    LBWorker.getInstance().processBulk(args.Results);

                    void LBEventHandler(object sender, LBResultEventArg args)
                    {
                        
                        var expected = (priority == OutletPriority.ASC) ? new string[] {"1","2","1","3","2" } : new string[] {"1","3","1","2","5" };
                        
                        for (var i = 0; i < args.Results.Count; i++)
                        {
                           Assert.AreEqual(expected[i], args.Results[i].LoadBalancedOutlet.First().ChannelNo);
                        }
                        
                        blocking = false;
                        
                    }

                    LBWorker.getInstance().OnResult += LBEventHandler;


                }
                catch (Exception e)
                {
                    logger.Info(e.Message);
                }
            }
            
            sortingWorker.OnResult += pdEventHanlder;
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
}