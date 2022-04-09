using System.IO;
using System.Reflection;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;


namespace LibUnitTest.Parser;
using NUnit.Framework;

public class JsonParserTest {
    private const string JsonFilePath = @"./config/project_start_sample.json";
    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);

    private string? porjectJsonString;
    private JsonParser? jparser;

    private Project project;
   
    [SetUp]
    public void Setup()
    {   
        porjectJsonString = File.ReadAllText(path);
        jparser = new JsonParser(porjectJsonString);
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
    }
}