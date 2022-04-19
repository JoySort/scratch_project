using System;
using System.IO;
using System.Reflection;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using CommonLib.Lib.Worker.Upper;


namespace LibUnitTest.Parser;
using NUnit.Framework;

public class ProjectStatusChangeTest {
    private const string JsonFilePath = @"./fixtures/project_start_sample.json";
    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);

    private string? porjectJsonString;
    private ProjectParser? jparser;

    private Project project;
   
    [SetUp]
    public void Setup()
    {   
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        project = parser.getProject();
        LowerMachineWorker.getInstance();

    }

    [Test, Order(4)]
    public void ProjectStart()
    {
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        
    }

    [Test, Order(3)]
    public void ProjectPause()
    {
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.pause);
        ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        
    }
    [Test, Order(0)]
    public void ProjectStatusStopDuplicated()
    {
        //ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        Assert.Throws<Exception>(()=>ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop));

   }
    
    [Test, Order(1)]
    public void ProjectStatusPauseDuplicated()
    {
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.pause);
        Assert.Throws<Exception>(()=>ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.pause));
        ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
         }
    
    [Test, Order(2)]
    public void ProjectStatusDuplicated2()
    {
               
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        Assert.Throws<Exception>(()=>ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start));
        ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
    }
    
    

}