using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CameraLib.Lib.ConfigVO;
using CameraLib.Lib.LowerMachine;
using CameraLib.Lib.Sort;
using CameraLib.Lib.Sort.ResultVO;
using CameraLib.Lib.Util;
using CameraLib.Lib.vo;
using CameraLib.Lib.Worker;
using CameraLib.Lib.Worker.Upper;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;
using static Newtonsoft.Json.Formatting;

namespace UnitTest.Worker;

public class ProjectConfigVersionTest
{
    private Logger logger ;
   
    private string? porjectJsonString;
    private ProjectParser? jparser;
    private Project project;
    private bool threadBlocking1=true;
    private bool threadBlocking2=true;

    [SetUp]
    public void setup()
    {
        LogManager.LoadConfiguration("config/logger.config");
        logger = LogManager.GetCurrentClassLogger();
        logger.Info("setup test for Lower Machine ");


        
        //Piple line wireup;
        UpperWorkerManager.getInstance();
        
        
        
    }
    
    [Test]
    public void testV1()
    {
        string JsonFilePath = @"./fixtures/project_start_v1.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V1);
        project = parser.getProject();
        var start = 0;
        var count = 14 ;
        var columnCount = 1;
        var perTargetPictureCount = 4;
        var columCountPerSection = 6;
        try
        {
            performTest(0, start, count, columnCount, perTargetPictureCount, columCountPerSection);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }

        while (threadBlocking1)
        {
            Thread.Sleep(100);
        }
    }

    [Test]
    public void testV2()
    {
        string JsonFilePath = @"./fixtures/project_configv2_pd_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        project = parser.getProject();
        var start = 0;
        var count = 14 ;
        var columnCount = 1;
        var perTargetPictureCount = 4;
        var columCountPerSection = 6;
        
        try
        {
            performTest(1, start, count, columnCount, perTargetPictureCount, columCountPerSection);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
        while (threadBlocking2)
        {
            Thread.Sleep(100);
        }
    }

    [TearDown]
    public void TearDown()
    {
        if(ProjectManager.getInstance().ProjectState!=ProjectState.stop)
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        
        UpperWorkerManager.getInstance().tearDown();
    }


    public void performTest(int index,int start,int count,int columnCount,int perTargetPictureCount,int columCountPerSection)
    {
        long startTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();;
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        ConsolidateWorker consolidateWorker = ConsolidateWorker.getInstance();
        List<Object> ppdResult = Utilizer.prepareData(project,start, count, columnCount, perTargetPictureCount, columCountPerSection);
        var expectedTriggerOutletMapping = (Dictionary<long, int>) ppdResult.First();
        var rrs = (List<RecResult>) ppdResult.Last();
        
        LBWorker lbWorker = LBWorker.getInstance();
        lbWorker.OnResult += lbEventHandler;
        
        var counter = 0;
        foreach (var item in rrs)
        {
           // if(counter++<48) logger.Debug("rec result {}",item.toLog());
        }
        logger.Info("generate data took  {}", DateTimeOffset.Now.ToUnixTimeMilliseconds()-startTimestamp);
        startTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
       
        consolidateWorker.processBulk(rrs);
        void lbEventHandler(object sender, LBResultEventArg lbResultEventArg)
        {
            //ConsolidateWorker consolidateWorker = ConsolidateWorker.getInstance();
            var finishTimestamp =DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var timeTook =finishTimestamp  - startTimestamp;
            logger.Info("Time took for task :{} stop at {}",timeTook,finishTimestamp);
            var outlets = project.Outlets;
            var consolidatePolicy = ConfigUtil.getModuleConfig().ConsolidatePolicy;

            var selectedOutletIndex = 0;
            var currentOutletIndex = 0;
            
            var lbResults = lbResultEventArg.Results.OrderBy(value=>value.Coordinate.TriggerId).ThenBy(value=>value.Coordinate.Column).ToList();
            Assert.AreEqual(count*columnCount,lbResults.Count);
            var lbResultIndex = 0;
            //var actualTriggerIds = lbResults.Select(value => value.Coordinate.TriggerId).ToArray();
            //var actualcolmNOs = lbResults.Select(value => value.Coordinate.Column).ToArray();
            //var actualChannelNOs = lbResults.Select(value => int.Parse(value.LoadBalancedOutlet.First().ChannelNo) -1).ToArray();
            var expectedTriggerIDs = new List<long>();
            var expectedColmNOs = new List<int>();
            var filters = project.Outlets.Select(value =>  new KeyValuePair<string,Object>(value.ChannelNo, value.Filters.Select(value =>
                value.Select(value =>
                    new KeyValuePair<int, float[][]>(value.Criteria.Index , value.FilterBoundaries)))));
            var printableFilter = JsonConvert.SerializeObject(filters, Indented);

            var actualChannels = lbResults.Select(value => value.Outlets.First().ChannelNo);
            //logger.Info("filters for channel:{}", printableFilter);
            for(var triggerId = start; triggerId<count;triggerId++)
            {
                for (var col = 0; col < columnCount; col++)
                {
                    
                    Assert.AreEqual(triggerId,lbResults[lbResultIndex].Coordinate.TriggerId); 
                    Assert.AreEqual(col,lbResults[lbResultIndex].Coordinate.Column);
                    var actualLBChannel = int.Parse(lbResults[lbResultIndex].LoadBalancedOutlet.First().ChannelNo) -1;
                    Assert.AreEqual(expectedTriggerOutletMapping[triggerId],actualLBChannel);
                    lbResultIndex++;
                }
            }
            logger.Info("{} assert finished with count {}",index,lbResults.Count);
            lbWorker.OnResult -= lbEventHandler;
            if (index == 0) this.threadBlocking1 = false;
            if (index == 1) this.threadBlocking2 = false;
        }
    }
    
    
}