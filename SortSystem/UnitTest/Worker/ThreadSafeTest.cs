using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using CommonLib.Lib.Worker;
using CommonLib.Lib.Worker.HTTP;
using CommonLib.Lib.Worker.Upper;
using NLog;
using NUnit.Framework;

namespace UnitTest.Worker;

public class ThreadSafeTest
{
     private Logger logger ;
   
    private string? porjectJsonString;
    private ProjectParser? jparser;
    private Project project;
    
    [SetUp]
    public void setup()
    {
        LogManager.LoadConfiguration("config/logger.config");
        logger = LogManager.GetCurrentClassLogger();
        logger.Info("setup test for Lower Machine ");


        
        //Piple line wireup;
        UpperWorkerManager.getInstance().setup();
        
        
        
    }
    
    [Test]
    public void testV1()
    {
        string JsonFilePath = @"./fixtures/project_start_v1.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V1);
        project = parser.getProject();
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        long startTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        LBWorker lbWorker = LBWorker.getInstance();
        lbWorker.OnResult += lbEventHandler;

        var count = 14 * 1;
        var columnCount = 24;

        var testRound = 10;
        for (int loopCycle = 0; loopCycle < testRound; loopCycle++)
        {
            Task.Run(() => issueRequest(count, columnCount));
        }

        var expectedCount = (testRound * count) * columnCount;

        while (threadBlocking1||threadBlocking2||threadBlocking3)
        {
            if (overAllGeneratedCount == overAllActualCount && expectedCount == overAllActualCount)
            {
                Assert.AreEqual(overAllGeneratedCount,overAllActualCount);
                Assert.AreEqual(expectedCount,(lastTriggerId + 1) * columnCount);
                
                
                break;
            }
            
            Thread.Sleep(100);
        }

        var resultSet = new List<LBResult>();
        foreach (var item in allResults)
        {
            resultSet = resultSet.Concat(item).ToList();
        }

        var resultTriggerIdMapping = new Dictionary<long, int>();
        foreach (var item in allTrigerIDs)
        {
            resultTriggerIdMapping = resultTriggerIdMapping.Concat(item).ToDictionary(g => g.Key, g => g.Value);
        }

        resultSet = resultSet.OrderBy(value => value.Coordinate.TriggerId).ThenBy(value => value.Coordinate.Column).ToList();
        resultTriggerIdMapping =
            resultTriggerIdMapping.OrderBy(item => item.Key).ToDictionary(g => g.Key, g => g.Value);

        var startIndex = 0;
        for (var triggerId = 0; triggerId < count; triggerId++)
        {
            for (var col = 0; col < columnCount; col++)
            {
                var actualLBChannel = int.Parse(resultSet[startIndex].LoadBalancedOutlet.First().ChannelNo) -1;
                Assert.AreEqual(resultTriggerIdMapping[triggerId],actualLBChannel);
                startIndex++;
            }
        }

        var finishTimestamp =DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var timeTook =finishTimestamp  - startTimestamp;
            logger.Info("Time took for task :{} stop at {}",timeTook,finishTimestamp);
        
    }

    private int startCounter = 0;
    private void issueRequest(int count,int columnCount)
    {
        var i = startCounter++;
        Thread.Sleep(20*i);
        var start =i * count;
        performTest(i,start,count,columnCount,perTargetPictureCount,columCountPerSection);
    }

    private List<List<LBResult>> allResults = new List<List<LBResult>>();
    private List<Dictionary<long, int>> allTrigerIDs = new List<Dictionary<long, int>>();
    private int overAllGeneratedCount = 0;
    private int overAllActualCount = 0;
    private bool threadBlocking1 = true;
    private bool threadBlocking2 = true;
    private bool threadBlocking3 = true;
    private int perTargetPictureCount = 4;
    private int columCountPerSection = 6;
    private long lastTriggerId;
    [TearDown]
    public void TearDown()
    {
        if(ProjectManager.getInstance().ProjectState != ProjectState.stop)
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        UpperWorkerManager.getInstance().tearDown();
    }


    public void performTest(int Index,int start,int count,int columnCount,int perTargetPictureCount,int columCountPerSection)
    {
        
        
        List<Object> ppdResult = Utilizer.prepareData(project,start, count, columnCount, perTargetPictureCount, columCountPerSection);
        var expectedTriggerOutletMapping = (Dictionary<long, int>) ppdResult.First();
        allTrigerIDs.Add(expectedTriggerOutletMapping);
        var rrs = (List<RecResult>) ppdResult.Last();
        var tempLastTrigerID = expectedTriggerOutletMapping.Keys.OrderBy(value => value).Last();
        if (tempLastTrigerID > lastTriggerId) lastTriggerId = tempLastTrigerID;
        
        overAllGeneratedCount = overAllGeneratedCount + rrs.Count/(perTargetPictureCount+1);
        ConsolidateWorker consolidateWorker = ConsolidateWorker.getInstance();
        consolidateWorker.processBulk(rrs);
    }
    
    public void lbEventHandler(object sender, LBResultEventArg lbResultEventArg)
    {
        //ConsolidateWorker consolidateWorker = ConsolidateWorker.getInstance();
        var lbResults = lbResultEventArg.Results.OrderBy(value=>value.Coordinate.TriggerId).ThenBy(value=>value.Coordinate.Column).ToList();
        allResults.Add(lbResults);
        overAllActualCount =overAllActualCount+ lbResults.Count;
        logger.Info("Test complete with actual count{}/expected overall count {}",overAllActualCount,overAllGeneratedCount);
    }
    
}