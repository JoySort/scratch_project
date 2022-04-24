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
    public void LBTestASC()
    {
        string JsonFilePath = @"./fixtures/project_pd_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        project = parser.getProject();
        
       
        
        string recResultJsonFixture = @"./fixtures/pd_sorttest_data.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        ConsolidatedResult[] consolidatedResults = JsonConvert.DeserializeObject<ConsolidatedResult[]>(jsonString);

        outletPriorityChange(OutletPriority.ASC,consolidatedResults);
    }
    [Test]
    public void LBTestDESC()
    {
        string JsonFilePath = @"./fixtures/project_pd_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        project = parser.getProject();
        
       
        
        string recResultJsonFixture = @"./fixtures/pd_sorttest_data.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        ConsolidatedResult[] consolidatedResults = JsonConvert.DeserializeObject<ConsolidatedResult[]>(jsonString);

        outletPriorityChange(OutletPriority.DESC,consolidatedResults);
    }
    
    void outletPriorityChange(OutletPriority priority,ConsolidatedResult[] consolidatedResults)
        {
           
            
            bool blocking = true;
            ConfigUtil.getModuleConfig().SortConfig.OutletPriority = priority;


            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            if(ProjectManager.getInstance().ProjectState!=ProjectState.start)
                ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
            sortingWorker.OnResult += pdEventHanlder;
            sortingWorker.processBulk(new List<ConsolidatedResult>(consolidatedResults));
            

            void pdEventHanlder(Object sender, SortingResultEventArg args)
            {
                sortingWorker.OnResult -= pdEventHanlder;
                try
                {
                    
                    logger.Info("time consumed (ms):{}",( new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - currentTimeStamp));
                   
                    LBWorker.getInstance().OnResult += LBEventHandler;
                    var serializedSortingResult =
                        JsonConvert.SerializeObject(args.Results.Select(value =>
                            value.Outlets.Select(v2 => v2.ChannelNo)));
                    logger.Info(serializedSortingResult);
                    LBWorker.getInstance().processBulk(args.Results);

                    void LBEventHandler(object sender, LBResultEventArg args)
                    {
                        LBWorker.getInstance().OnResult -= LBEventHandler;
                        List<string> lbresults = new List<string>();
                        var lbres=args.Results.OrderBy(value => value.Coordinate.TriggerId);
                        foreach (var item in lbres)
                        {
                            lbresults.Add(item.LoadBalancedOutlet.First().ChannelNo);
                        }

                        var expected = (priority == OutletPriority.ASC) ? new string[] {"1","2","1","3","3","2","2","3" } : new string[] {"1","3","1","2","5","4","2","3" };
                        var lbresultArray = lbresults.ToArray();
                        Assert.AreEqual(expected.Length,lbresultArray.Length);
                        Assert.AreEqual(expected, lbresultArray);
                        logger.Info("Assert finished expect:{} result:{}",JsonConvert.SerializeObject(expected),JsonConvert.SerializeObject(lbresultArray));
                        blocking=false;
                        
                    }

                   


                }
                catch (Exception e)
                {
                    logger.Info(e.Message);
                }
                
            }


            var counter = 0;
            while (blocking)
            {
                if(counter>200)Assert.Fail("Timeout");
                counter++;
                Thread.Sleep(100);
            }

            
            logger.Info("LB Test stop");
            ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
        }
}