using System.IO;
using System.Linq;
using System.Reflection;
using CameraLib.Lib.Util;
using CameraLib.Lib.vo;
using Newtonsoft.Json;


namespace UnitTest.Parser;
using NUnit.Framework;

public class JsonParserTestV2 {
    private const string JsonFilePath = @"./fixtures/project_start_sample.json";
    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);

    private string? porjectJsonString;
    private ProjectParser? jparser;

    private Project project;
   
    [SetUp]
    public void Setup()
    {   
        porjectJsonString = File.ReadAllText(path);
        jparser = new ProjectParser(porjectJsonString,ProjectParser.V2);
        project = jparser.getProject();
    }

    [Test]
    public void CheckProject()
    {
        Assert.AreEqual(project.Name,"a");
        Assert.AreEqual(project.Genre.Name,GenreName.palmDate);
        Assert.AreEqual(project.Category.CatId,2);
    }
    
    [Test]
    public void ProjectBackAndForth()
    {
       var myProject= JsonConvert.DeserializeObject<Project>(JsonConvert.SerializeObject(project));
       Assert.AreEqual(JsonConvert.SerializeObject(project),JsonConvert.SerializeObject(myProject));
    }

    [Test]
    public void CheckCriteria()
    {
        
        
        
        var fullCriteria = jparser?.FullCriteria;
        var enabledCriteria = jparser?.EnabledCriteria;
        var fullCriteriaLength = 11;
        var enabledCriteriaLength = 4;
        Assert.AreEqual(fullCriteriaLength,fullCriteria?.Count);
        Assert.AreEqual(enabledCriteriaLength,enabledCriteria?.Count);
        
        
    }

    [Test]
    public void CheckOutlet()
    {
        var outlets = jparser?.Outlets;
        var outletNO = 8;
        Assert.AreEqual(outlets?.Count,outletNO);
        var expectedFilters = new string[][] {
            new string[]{"height","zl"},
            new string[]{"height","zl"}, //new string[]{"wdith","zl"},
            new string[]{},
            new string[]{},
            new string[]{},
            new string[]{},
            new string[]{},
            new string[]{}
        };
        foreach (var outlet in outlets)
        {
            if (outlet.Filters.Length == 0) continue;
            Assert.True(outlet.Filters.First().Select((filter=>filter.Criteria.Code)).ToArray().OrderBy(value=>value).SequenceEqual(expectedFilters[int.Parse(outlet.ChannelNo)-1].OrderBy(value=>value)));  
        }
        
    }
}