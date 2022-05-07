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
using CommonLib.Lib.Worker.Upper;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace UnitTest.Worker;

public class SortingWorkerTest
{
    private Logger logger ;
   
    private string? porjectJsonString;
    private ProjectParser? jparser;
    private Project appleProject;
    private Project pdProject;
    private ConsolidatedResult[] appleConsolidatedResults;
    private ConsolidatedResult[] pdConsolidatedResults;
    private SortingWorker sortingWorker = SortingWorker.getInstance();
    private bool threadBlocking1=true;
    private bool threadBlocking2=true;
    private bool threadBlocking3=true;

    [SetUp]
    public void setup()
    {
        LogManager.LoadConfiguration("config/logger.config");
        logger = LogManager.GetCurrentClassLogger();
        logger.Info("setup test ");

       
    }

    [Test,Order(1)]
    public void appleConsolidationTest()
    {
        setupProjectForApple();
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(appleProject,ProjectState.start);
        setupDataForApple();
        sortingWorker.OnResult += appleEventHanlder;
        sortingWorker.processBulk(new List<ConsolidatedResult>(appleConsolidatedResults));
        while (threadBlocking1)
        {
            Thread.Sleep(100);
        }
        logger.Info("APPLE Test stop");
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(appleProject,ProjectState.stop);
    }
    [Test,Order(2)]
    public void pdConsolidationDESCTest()
    {
        setupProjectForPD();
       
        setupDataForPD();
        outletPriorityChange(OutletPriority.DESC,pdConsolidatedResults);
        
        while (threadBlocking2)
        {
            Thread.Sleep(100);
        }
        
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(pdProject,ProjectState.stop);
    }
    
    [Test,Order(3)]
    public void pdConsolidationASCTest()
    {
        setupProjectForPD();
        
        setupDataForPD();
        outletPriorityChange(OutletPriority.ASC,pdConsolidatedResults); 
        
        while (threadBlocking3)
        {
            Thread.Sleep(100);
        }
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(pdProject,ProjectState.stop);
    }
    
    
    private void setupProjectForApple()
    {
        string JsonFilePath = @"./fixtures/project_apple_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        appleProject = parser.getProject();
    }

    private void setupDataForApple()
    {
        logger.Info("APPLE Test begin");
        
        string recResultJsonFixture = @"./fixtures/apple_sorttest_data.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        appleConsolidatedResults= JsonConvert.DeserializeObject<ConsolidatedResult[]>(jsonString);

    }

    private void setupProjectForPD()
    {
        logger.Info("PD Test begin");
        
        string JsonFilePath = @"./fixtures/project_pd_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        pdProject = parser.getProject();
    }

    private void setupDataForPD()
    {
        string recResultJsonFixture = @"./fixtures/pd_sorttest_data.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        pdConsolidatedResults = JsonConvert.DeserializeObject<ConsolidatedResult[]>(jsonString);
    }

 

    [TearDown]
    public void TearDown()
    {
        if(ProjectManager.getInstance().ProjectState!=ProjectState.stop)
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
    }
    public void appleEventHanlder(Object sender, SortingResultEventArg args)
    {
        try
        {
            Assert.AreEqual("1", args.Results.First().Outlets.First().ChannelNo);
            logger.Info("time consumed (ms):{}",
                (DateTime.Now.ToFileTime() - args.Results.First().RecTimestamp) / 100);
            logger.Info("APPle assert finished {}", JsonConvert.SerializeObject(args.Results));
        }
        catch (Exception e)
        {
            logger.Error(e.Message);
        }

        threadBlocking1 = false;
        sortingWorker.OnResult -= appleEventHanlder;
    }


    void outletPriorityChange(OutletPriority priority,ConsolidatedResult[] consolidatedResult)
        {
           
            
            ConfigUtil.getModuleConfig().SortConfig.OutletPriority = priority;
            //必须在改变优先级后再启动，否则无法生效。
            ProjectManager.getInstance().dispatchProjectStatusStartEvent(pdProject,ProjectState.start);
            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            string[] ascExpected  = new string[] {"1", "2", "1", "2", "2","2","2","2"};
            string[] descExpected = new string[] {"1", "3", "1", "3", "5","5","3","3"};

            sortingWorker.OnResult += pdEventHanlder;
            sortingWorker.processBulk(new List<ConsolidatedResult>(consolidatedResult));
            
          
            void pdEventHanlder(Object sender, SortingResultEventArg args)
            {
                try
                {
                    logger.Info("time consumed (ms):{}",( new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - currentTimeStamp));
                    var expected = (priority == OutletPriority.ASC) ? ascExpected : descExpected;
                    

                    var actual = new string[args.Results.Count];
                    for (var i = 0; i < args.Results.Count; i++)
                    {
                        List<string> result = args.Results[i].Outlets.Select(outlet => outlet.ChannelNo).ToList();
                        actual[i] = String.Join(",", result.OrderBy(x => x).ToArray());
                        
                    }
                    
                    logger.Info("PD assert finished priority {} expected {} acture {}", Enum.GetName(priority),expected,actual);
                    
                    Assert.AreEqual(expected,actual);
                    
                       
                }
                catch (Exception e)
                {
                    logger.Info(e.Message);
                }
                sortingWorker.OnResult -= pdEventHanlder;
                if (priority == OutletPriority.DESC) threadBlocking2 = false;
                if (priority == OutletPriority.ASC) threadBlocking3 = false;

            }
            
            
            logger.Info("PD Test stop");
            
            
        }


}