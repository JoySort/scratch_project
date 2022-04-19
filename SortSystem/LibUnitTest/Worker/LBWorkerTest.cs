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
    public void LBTest()
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

       

        void outletPriorityChange(OutletPriority priority)
        {
           
            
            bool blocking = true;
            ConfigUtil.getModuleConfig().SortConfig.OutletPriority = priority;


            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            
            ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
            sortingWorker.OnResult += pdEventHanlder;
            sortingWorker.processBulk(new List<ConsolidatedResult>(consolidatedResults));
            

            void pdEventHanlder(Object sender, SortingResultEventArg args)
            {
                try
                {
                    
                    logger.Info("time consumed (ms):{}",( new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() - currentTimeStamp));
                   
                    LBWorker.getInstance().OnResult += LBEventHandler;
                    LBWorker.getInstance().processBulk(args.Results);

                    void LBEventHandler(object sender, LBResultEventArg args)
                    {

                        List<string> lbresults = new List<string>();

                        foreach (var item in args.Results)
                        {
                            lbresults.Add(item.LoadBalancedOutlet.First().ChannelNo);
                        }

                        var expected = (priority == OutletPriority.ASC) ? new string[] {"1","2","1","3","3","2","2","3" } : new string[] {"1","3","1","2","5","4","2","3" };
                        var lbresultArray = lbresults.ToArray();
                        for (var i = 0; i < args.Results.Count; i++)
                        {
                           Assert.AreEqual(expected[i], lbresultArray[i]);
                        }
                        LBWorker.getInstance().OnResult -= LBEventHandler;
                      
                        if(OutletPriority.ASC == priority)outletPriorityChange(OutletPriority.DESC);
                    }

                   


                }
                catch (Exception e)
                {
                    logger.Info(e.Message);
                }
                sortingWorker.OnResult -= pdEventHanlder;
            }
            
            
        
            
            
            logger.Info("PD Test stop");
            ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
        }

        outletPriorityChange(OutletPriority.ASC);
        

    }
}