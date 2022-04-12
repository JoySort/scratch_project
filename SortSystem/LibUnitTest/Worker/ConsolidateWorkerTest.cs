using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NUnit.Framework;

namespace LibUnitTest.Parser;

public class ConsolidateWorkerTest
{
    
    private const string JsonFilePath = @"./fixtures/project_apple_rec_start.json";
    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);

    private string? porjectJsonString;
    private ProjectParser? jparser;

    private Project project;
    
    private ConsolidateWorker worker = ConsolidateWorker.getInstance();
    [SetUp]
    public void setup()
    {
       
        porjectJsonString = File.ReadAllText(path);
        ProjectParser parser = new ProjectParser(porjectJsonString);
        project = parser.getProject();
        
        
    }

    [Test]
    public void appleConsolidationTest()
    {
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
        string recResultJsonFixture = @"./fixtures/apple_rec_result.json";
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);
        RecResult[] recResults = JsonConvert.DeserializeObject<RecResult[]>(File.ReadAllText(path));
        
        
        worker.Consolidate(new List<RecResult>(recResults));
        
        
        
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
    }

    [Test]
    public void pdConsolidationTest()
    {
        
    }


}