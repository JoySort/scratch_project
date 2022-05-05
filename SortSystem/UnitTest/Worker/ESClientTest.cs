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
using CommonLib.Lib.Worker.Analytics;
using CommonLib.Lib.Worker.Upper;
using NLog;
using NUnit.Framework;

namespace UnitTest.Worker;

public class ESClientTest
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
        //UpperPipelineWireUtil.setup();
        
        
        
    }
    
    //[Test]
    public void testV1()
    {
        LowerMachineDriver.getInstance();
        ElasticSearchWorker.getInstance();
        string JsonFilePath = @"./fixtures/project_start_v1.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V1);
        project = parser.getProject();
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        long startTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        

        var count = 14 * 1;
        var columnCount = 24;

        var testRound = 10;
        for (int loopCycle = 0; loopCycle < testRound; loopCycle++)
        {
            Task.Run(() => issueRequest(count, columnCount));
        }

        
        
        while (true)
        {
            
            
            Thread.Sleep(100);
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


    private int perTargetPictureCount = 4;
    private int columCountPerSection = 6;
    [TearDown]
    public void TearDown()
    {
        if(ProjectManager.getInstance().ProjectState != ProjectState.stop)
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
       
    }


    public void performTest(int Index,int start,int count,int columnCount,int perTargetPictureCount,int columCountPerSection)
    {
        
        
        List<Object> ppdResult = Utilizer.prepareData(project,start, count, columnCount, perTargetPictureCount, columCountPerSection);
        var expectedTriggerOutletMapping = (Dictionary<long, int>) ppdResult.First();
        var rrs = (List<RecResult>) ppdResult.Last();
        ElasticSearchWorker.getInstance().processBulkRecResult(rrs);
    }
    
    
}