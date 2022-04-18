using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;
using NUnit.Framework;

namespace LibUnitTest.Worker;

public class LowerMachineWorkerTest
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
        UpperPipelineWireUtil.setup();
        
        
        
    }

    private List<RecResult> prepareData(int startTriggerID, long count,int columnCount,int perTargetPictureCount,int columCountPerSection )
    {

        
        
        var result = new List<RecResult>();
        var critieraList = project.Criterias;
        var outlets = project.Outlets;
        var consolidatePolicy = ConfigUtil.getModuleConfig().ConsolidatePolicy;
        Random rdn = new Random();
        var selectedOutletIndex = 0;

        for(var triggerIdex=startTriggerID; triggerIdex< (startTriggerID+count) ; triggerIdex++){
            
            //针对每个triggerid,找到一个filter不为空的outlet,以这个outlet的filter来生成一个符合这个filter的RecResult
            while(true)
            {
                selectedOutletIndex = triggerIdex % outlets.Length;
               // logger.Debug("selectedOutletIndex,currentOutletIndex {}.{}",selectedOutletIndex,currentOutletIndex);
                if (outlets[selectedOutletIndex].Filters.Length > 0)
                {
                    break;
                }

            }

            
            for(var col=0; col<columnCount;col++)
            {
               
                for (var row = 0; row < perTargetPictureCount; row++)
                {
                    float section = col / columCountPerSection;
                    var coordinate = new Coordinate((int)Math.Floor(section), col, row, triggerIdex);
                    var expectedFeatureCount = critieraList.Length;
                    
                    Dictionary<string,Feature> features= new Dictionary<string,Feature> ();
                    
                    
                    for (var featureIndex = 0; featureIndex < expectedFeatureCount; featureIndex++)
                    {
                        
                        var found = false;
                        
                        foreach (var OrFilters in outlets[selectedOutletIndex].Filters)
                        {
                            foreach (var andFilter in OrFilters)
                            {
                                if (andFilter.Criteria.Code == critieraList[featureIndex].Code)
                                {
                                    //目前filter为了支持以后更富在的filter,允许在andFilter提供2个范围，实际上，如果是两个范围，是无法形成and的，所以，我们
                                    //目前虽然按照数组给值，仍然是给一个值，所以下面用First()拿到的就是这个Filter的界限。
                                    var diff = andFilter.FilterBoundaries.First().Last() -
                                               andFilter.FilterBoundaries.First().First();
                                    //生成一个不小于下边界，不大于上边界的随机数。
                                    var featureValue = andFilter.FilterBoundaries.First().First() + ((float)rdn.Next((int)diff*100))/100;
                                    
                                    if(!features.ContainsKey(critieraList[featureIndex].Code))features.Add(critieraList[featureIndex].Code,new Feature(critieraList[featureIndex].Index,featureValue));
                                    found = true;
                                }
                            }
                            
                        }

                        if (!found)
                            features.Add(critieraList[featureIndex].Code,
                                new Feature(critieraList[featureIndex].Index, 0)); 
                        //if(coordinate.TriggerId<8 && coordinate.Section==0 && coordinate.Column == 0)logger.Debug("feature {},{},{},{}",selectedOutletIndex,coordinate.Key(),features.Last().Value.CriteriaIndex,features.Last().Value.Value);

                    }
                    
                    var recResult = new RecResult(coordinate, expectedFeatureCount, features.Values.ToList());
                    result.Add(recResult);
                }
            }
        }
        return result;
    }


    [Test]
    public void testV1()
    {
        string JsonFilePath = @"./fixtures/project_start_v1.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V1);
        project = parser.getProject();
        performTest();
    }

    [Test]
    public void testV2()
    {
        string JsonFilePath = @"./fixtures/project_pd_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        project = parser.getProject();
        performTest();
    }

    public void performTest()
    {


        
        var start = 0;
        var count = 14 * 60;
        var columnCount = 24;
        var perTargetPictureCount = 4;
        var columCountPerSection = 6;
        
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        ConsolidateWorker consolidateWorker = ConsolidateWorker.getInstance();
        LBWorker lbWorker = LBWorker.getInstance();
        lbWorker.OnResult += lbEventHandler;
        consolidateWorker.processBulk(prepareData(start,count,columnCount,perTargetPictureCount,columCountPerSection));

        void lbEventHandler(object sender, LBResultEventArg lbResultEventArg)
        {
            
            var outlets = project.Outlets;
            var consolidatePolicy = ConfigUtil.getModuleConfig().ConsolidatePolicy;

            var selectedOutletIndex = 0;
            var currentOutletIndex = 0;
            
            var lbResults = lbResultEventArg.Results;
            Assert.AreEqual(lbResults.Count,count*columnCount);
            var lbResultIndex = 0;
            for(var triggerId = start; triggerId<count;triggerId++)
            {
                while(true)
                {
                    selectedOutletIndex = triggerId % outlets.Length;
                    // logger.Debug("selectedOutletIndex,currentOutletIndex {}.{}",selectedOutletIndex,currentOutletIndex);
                    if (outlets[selectedOutletIndex].Filters.Length > 0)
                    {
                        break;
                    }
                }
                
                for (var col = 0; col < columnCount; col++)
                {
                    Assert.AreEqual(triggerId,lbResults[lbResultIndex].Coordinate.TriggerId); 
                    Assert.AreEqual(col,lbResults[lbResultIndex].Coordinate.Column);
                    Assert.AreEqual(""+(selectedOutletIndex+1),lbResults[lbResultIndex].Outlets.First().ChannelNo);
                    
                    
                    lbResultIndex++;


                }
            }

            Task.Run(() =>
            {
                Thread.Sleep(1000);
                ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
            });
        }
        

    }
}