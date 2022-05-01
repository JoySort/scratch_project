using System;
using System.IO;
using System.Reflection;
using CameraLib.Lib.LowerMachine;
using CameraLib.Lib.Util;
using CameraLib.Lib.vo;
using CameraLib.Lib.Worker.Upper;


namespace UnitTest.Parser;
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
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        
    }

    [Test, Order(3)]
    public void ProjectPause()
    {
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.pause);
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        
    }
    [Test, Order(0)]
    public void ProjectStatusStopDuplicated()
    {
        //ProjectEventDispatcher.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
        Assert.Throws<Exception>(()=>ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop));

   }
    
    [Test, Order(1)]
    public void ProjectStatusPauseDuplicated()
    {
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.pause);
        Assert.Throws<Exception>(()=>ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.pause));
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
         }
    
    [Test, Order(2)]
    public void ProjectStatusDuplicated2()
    {
               
        ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        Assert.Throws<Exception>(()=>ProjectManager.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start));
        ProjectManager.getInstance().dispatchProjectStatusChangeEvent(ProjectState.stop);
    }
    
    

}