using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CameraLib.Lib.ConfigVO;
using CameraLib.Lib.LowerMachine;
using CameraLib.Lib.Sort.ResultVO;
using CameraLib.Lib.Util;
using CameraLib.Lib.vo;
using CameraLib.Lib.Worker.Upper;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;

namespace UnitTest.Hardware;

public class AdvancedEmitterTest
{
    private Logger logger ;

    private List<EmitResult> emitResults = new List<EmitResult>();
    [SetUp]
    public void setup()
    {
        LogManager.LoadConfiguration("config/logger.config");
        logger = LogManager.GetCurrentClassLogger();
        logger.Info("setup test ");


        LowerMachineWorker.getInstance();
        
        //setup data
        setupData();

    }

    
    public void setupData()
    {
        for(var triggerid=0; triggerid<10;triggerid++){
            for (var column = 0; column < 23; column++)
            {
                for (var i = 0; i < 8; i++)
                {
                    var emitResult = new EmitResult(column,i+1,triggerid);
                    emitResults.Add(emitResult);
                }
            }
        }
    }

    [Test]
    public void LBTestASC()
    {
        string JsonFilePath = @"./fixtures/project_start_v1.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        string porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V1);
        Project project = parser.getProject();
        
        
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        LowerMachineWorker.getInstance().processBulk(emitResults);
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
        
    }
   
}