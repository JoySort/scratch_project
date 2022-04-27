using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using CommonLib.Lib.Worker.Upper;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace LibUnitTest.Hardware;

public class AdvancedEmitterTest
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
        LowerMachineWorker.getInstance();
        EmitWorker.getInstance();
        SortingWorker.getInstance();
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
     void outletPriorityChange(OutletPriority priority,ConsolidatedResult[] consolidatedResults)
        {
           
            
            bool blocking = true;
            ConfigUtil.getModuleConfig().SortConfig.OutletPriority = priority;


            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            
            sortingWorker.OnResult += ((sender,e)=>{
                LBWorker.getInstance().processBulk(e.Results);
            });
            LBWorker.getInstance().OnResult+=((sender,e)=>{
                EmitWorker.getInstance().processBulk(e.Results);
            });
            EmitWorker.getInstance().OnResult+=((sender,e)=>{
                LowerMachineWorker.getInstance().processBulk(e.Results);
            });
            
            
            if(ProjectManager.getInstance().ProjectState!=ProjectState.start)
                ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
          
            
            
            
            sortingWorker.processBulk(new List<ConsolidatedResult>(consolidatedResults));
            
            
            

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