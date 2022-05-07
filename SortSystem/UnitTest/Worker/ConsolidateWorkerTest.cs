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

public class ConsolidateWorkerTest
{
    private Logger logger ;
   
    private string? porjectJsonString;
    private ProjectParser? jparser;
    private Project project;
    
    private ConsolidateWorker worker = ConsolidateWorker.getInstance();
    private bool threadBlocking1=true;
    private bool threadBlocking2=true;

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

        //ConfigUtil.getModuleConfig().WorkingMode = WorkingMode.EjectDefault;
        
        string JsonFilePath = @"./fixtures/project_apple_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        project = parser.getProject();
        
        
        logger.Info("APPLE Test begin");
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        string recResultJsonFixture = @"./fixtures/apple_rec_result.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        RecResult[] recResults = JsonConvert.DeserializeObject<RecResult[]>(jsonString);
        worker.OnResult += appleEventHanlder;
        worker.processBulk(new List<RecResult>(recResults));
        while (threadBlocking1)
        {
            Thread.Sleep(100);
        }
        
        logger.Info("APPLE Test stop");
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
    }
    
    [Test]
    public void pdConsolidationTest()
    {
        logger.Info("PD Test begin");
        
        string JsonFilePath = @"./fixtures/project_pd_rec_start.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        project = parser.getProject();
        
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        
        string recResultJsonFixture = @"./fixtures/pd_rec_result.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        RecResult[] recResults = JsonConvert.DeserializeObject<RecResult[]>(jsonString);


        worker.OnResult += pdEventHanlder;
        worker.processBulk(new List<RecResult>(recResults));
       

        while (threadBlocking2)
        {
            Thread.Sleep(100);
        }
        
        logger.Info("PD Test stop");
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
    }
    
    public void appleEventHanlder(Object sender, ResultEventArg args)
    {
        Assert.AreEqual(args.Results.First().Features.Where(value=>value.CriteriaIndex==1).First().Value,15);
        Assert.AreEqual(args.Results.First().Features.Where(value=>value.CriteriaIndex==12).First().Value,4);
            
        Assert.AreEqual(args.Results.Last().Features.Where(value=>value.CriteriaIndex==1).First().Value,20);
        Assert.AreEqual(args.Results.Last().Features.Where(value=>value.CriteriaIndex==12).First().Value,24);
        logger.Info("APPLE assert finished");
        threadBlocking1 = false;
        worker.OnResult -= appleEventHanlder;
    }
    
    public void pdEventHanlder(Object sender, ResultEventArg args)
    {
        Assert.AreEqual(args.Results.First().Features.Where(value=>value.CriteriaIndex==1).First().Value,25);
        Assert.AreEqual(args.Results.First().Features.Where(value=>value.CriteriaIndex==7).First().Value,25);
        Assert.AreEqual(args.Results.First().Features.Where(value=>value.CriteriaIndex==8).First().Value,15);
        
        Assert.AreEqual(args.Results.Last().Features.Where(value=>value.CriteriaIndex==1).First().Value,10);
        Assert.AreEqual(args.Results.Last().Features.Where(value=>value.CriteriaIndex==7).First().Value,22.5);
        Assert.AreEqual(args.Results.Last().Features.Where(value=>value.CriteriaIndex==8).First().Value,12.5);
        logger.Info("PD assert finished process time {} ms",(DateTime.Now.ToFileTime()-args.Results.Last().RecTimestamp)/10000);
        threadBlocking2 = false;
        worker.OnResult -= pdEventHanlder;
    }

}