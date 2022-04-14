using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace LibUnitTest.Worker;

public class ConsolidateWorkerTest
{
    private Logger logger ;
   
    private string? porjectJsonString;
    private ProjectParser? jparser;
    private Project project;
    
    private ConsolidateWorker worker = ConsolidateWorker.getInstance();
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
        string recResultJsonFixture = @"./fixtures/apple_rec_result.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        RecResult[] recResults = JsonConvert.DeserializeObject<RecResult[]>(jsonString);

        bool blocking = true;
        
        worker.processSingle(new List<RecResult>(recResults));

        worker.OnResult += ((sender, args) =>
        {
            blocking = false;
        });

        worker.OnResult += appleEventHanlder;
        
        while (blocking)
        {
            Thread.Sleep(100);
        }
        worker.OnResult -= appleEventHanlder;

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
        
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        
        string recResultJsonFixture = @"./fixtures/pd_rec_result.json";
        string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,recResultJsonFixture);
        string jsonString = File.ReadAllText(_path);
        RecResult[] recResults = JsonConvert.DeserializeObject<RecResult[]>(jsonString);

        bool blocking = true;
        
        worker.processSingle(new List<RecResult>(recResults));
        worker.OnResult += pdEventHanlder;
        worker.OnResult += ((sender, args) =>
        {
            logger.Info("PD assert finished {}",JsonConvert.SerializeObject(args.Results));
            blocking = false;
        });
        
        while (blocking)
        {
            Thread.Sleep(100);
        }
        worker.OnResult -= pdEventHanlder;
        
        logger.Info("PD Test stop");
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
    }
    
    public void appleEventHanlder(Object sender, ResultEventArg args)
    {
        Assert.AreEqual(args.Results.First().Features.First().Value,15);
        Assert.AreEqual(args.Results.First().Features.Last().Value,4);
            
        Assert.AreEqual(args.Results.Last().Features.First().Value,20);
        Assert.AreEqual(args.Results.Last().Features.Last().Value,24);
        logger.Info("APPLE assert finished");
    }
    
    public void pdEventHanlder(Object sender, ResultEventArg args)
    {
        Assert.AreEqual(args.Results.First().Features.First().Value,25);
        Assert.AreEqual(args.Results.First().Features[1].Value,25);
        Assert.AreEqual(args.Results.First().Features.Last().Value,15);
        
        Assert.AreEqual(args.Results.Last().Features.First().Value,10);
        Assert.AreEqual(args.Results.Last().Features[1].Value,22.5);
        Assert.AreEqual(args.Results.Last().Features.Last().Value,12.5);
        logger.Info("PD assert finished process time {} ms",(DateTime.Now.ToFileTime()-args.Results.Last().ProcessTimestamp)/10000);
    }

}